using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using WanderingRoad;
using WanderingRoad.Procgen.RecursiveHex;
using WanderingRoad.Procgen.RecursiveHex.Json;
using U3D.Threading.Tasks;
using Unity.Entities;
using Unity.Physics.Systems;
using WanderingRoad.Random;
using System.ComponentModel;
using Unity.Transforms;

public class PropRegion
{
    private Vector2 _center;
    private Rect _bounds;
    private Guid _guid;
    private GameState _state;
    private const int MULTIPLIER = 8;

    private NativeArray<Entity> _entities;
    private World _world = World.DefaultGameObjectInjectionWorld;

    private Entity Prefab = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(PrefabLibrary)).GetSingleton<PrefabLibrary>().Sphere;


    public PropRegion(GameState state, Rect bounds, Guid id)
    {
        _state = state;
        _bounds = bounds;
        _guid = id;

        _center = bounds.center * MULTIPLIER;

        _world.EntityManager.SetEnabled(Prefab, false);



    }

    public void Update(Vector2 sample)
    {
        //if (ContinueLoading()) return;

        //_entities = _world.EntityManager.Instantiate(Prefab, _raycastJobResult.translations.Length, Allocator.Persistent);

        var distance = Vector2.Distance(sample, _center);


        if (distance > 70)
        {
            //Debug.DrawLine(new Vector3(sample.x, 0, sample.y), new Vector3(_center.x, 0, _center.y), Color.red);

            if (_loading == LoadingStep.NotStarted)
                return;

            if(_loading == LoadingStep.Complete)
            {
                Unload();
                _loading = LoadingStep.NotStarted;
            }

        }

        if (distance > 30)
        {
            Debug.DrawLine(new Vector3(sample.x, 0, sample.y), new Vector3(_center.x, 0, _center.y), Color.blue);

            if (_loading == LoadingStep.NotStarted)
                return;
        }

        Debug.DrawLine(new Vector3(sample.x, 0, sample.y), new Vector3(_center.x, 0, _center.y), Color.white);

        ContinueLoading();

    }

    #region LOADING

    private LoadingStep _loading = LoadingStep.NotStarted;

    private Queue<Exception> _exceptions = new Queue<Exception>();

    private enum LoadingStep{ 
        NotStarted = 0,
        LoadingHexgroupFromFile = 1,
        Raycasting = 2,
        UpdatingAndEnablingPrefabs = 3,
        Complete = 10

    }

    Task _loadingHexgroupTask;
    RaycastJobResult _raycastJobResult;

    private bool ContinueLoading()
    {

        while (_exceptions.Count >0)
        {
            Debug.LogError(_exceptions.Dequeue());
        }


        switch (_loading)
        {
            case LoadingStep.NotStarted:


                Debug.Log($"Started Loading Chunk {this._guid}");

                _loadingHexgroupTask = Task.Run(LoadHexgroupFromFileAndPopulatePositions);
                _loading = LoadingStep.LoadingHexgroupFromFile;

                return true;
            case LoadingStep.LoadingHexgroupFromFile:

                if (!_loadingHexgroupTask.IsCompleted) return true;


                _raycastJobResult = InstantiateElements(_float2s);

                _float2s.Clear();

                _loading = LoadingStep.Raycasting;





                return true;

            case LoadingStep.Raycasting:

                if (!_raycastJobResult.handle.IsCompleted) return true;

                var color = Color.white;

                _raycastJobResult.handle.Complete();

                _entities = _world.EntityManager.Instantiate(Prefab, _raycastJobResult.positions.Length, Allocator.Persistent);

                _settingPropertiesTask = new SetProperties()
                {
                    entities = _entities,
                    manager = _world.EntityManager,
                    translations = _raycastJobResult.translations,
                    scale = _raycastJobResult.scales,
                    rotations = _raycastJobResult.rotations
                }.Schedule();

                _settingPropertiesTask.Complete();

                _raycastJobResult.positions.Dispose();
                _raycastJobResult.scales.Dispose();
                _raycastJobResult.rotations.Dispose();

                _loading = LoadingStep.UpdatingAndEnablingPrefabs;

                return true;

            case LoadingStep.UpdatingAndEnablingPrefabs:

                //if (!_settingPropertiesTask.IsCompleted) return true;

                //_settingPropertiesTask.Complete();

                _raycastJobResult.translations.Dispose();

                var e = _world.EntityManager;

                for (int i = 0; i < _entities.Length; i++)
                {
                    e.SetEnabled(_entities[i], true);
                }

                _loading = LoadingStep.Complete;

                Debug.Log($"Loaded Chunk {this._guid}");

                return true;

            case LoadingStep.Complete:

                

                return false;
            default:
                throw new InvalidEnumArgumentException();
        }
    }

    private JobHandle _settingPropertiesTask;

    //Getting Entities

    private List<float2> _float2s = new List<float2>();

    private void LoadHexgroupFromFileAndPopulatePositions()
    {

        try
        {

            var hexGroup = Util.DeserialiseFile<HexGroup>(
                Paths.GetHexGroupPath(_state.Seed, _guid.ToString()),
                new HexGroupConverter());

            //var subDivide = hexGroup.Subdivide(3, x => x.Code);

            var divisions = 1;

            var inverseMatrix = HexIndex.GetInverseMultiplicationMatrix(divisions);

            var subs = hexGroup
            .Subdivide(divisions, x => x.Code)
            .GetHexes()
            .Where(x => x.Payload.EdgeDistance > 0.5f && x.Payload.EdgeDistance < 3)
            //.Select(x=> x.Index.Position3d*8)
            .Select(x => inverseMatrix.MultiplyPoint(x.Index.Position3d) * MULTIPLIER)
            .Select(x => new float2(x.x,x.z))
            .ToList();

            {
                lock (_float2s)
                {
                    _float2s = subs;
                }
            }

        }
        catch(Exception ex)
        {
            _exceptions.Enqueue(ex);
        }

    }


    private struct RaycastJobResult
    {
        public NativeArray<float2> positions;
        public NativeArray<Translation> translations;
        public NativeArray<Rotation> rotations;
        public NativeArray<NonUniformScale> scales;
        public JobHandle handle;
        public CollisionWorld world;
    }

    RaycastJobResult InstantiateElements(List<float2> vectors)
    {
        if (vectors is null)
        {
            throw new ArgumentNullException(nameof(vectors));
        }

        var world = World.DefaultGameObjectInjectionWorld;
        var entities = world.EntityManager;
        //var collision = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld.CollisionWorld;

        //Debug.Log($"Casting many rays ({vectors.Length})");

        var positions = new NativeArray<float2>(vectors.ToArray(), Allocator.Persistent);

        var physicsWorld = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld;
        var realCollision = physicsWorld.CollisionWorld;
        var translations = new NativeArray<Translation>(positions.Length, Allocator.Persistent);
        var rotations = new NativeArray<Rotation>(positions.Length, Allocator.Persistent);
        var scales = new NativeArray<NonUniformScale>(positions.Length, Allocator.Persistent);


        var collisionWorld = realCollision.Clone();

        collisionWorld.BuildBroadphase(ref physicsWorld, 0f, float3.zero);

        var package = new RaycastJobResult()
        {
            positions = positions,
            translations = translations,
            rotations = rotations,
            scales = scales,

            world = collisionWorld,
            handle = new RaycastJob()
            {
                positions = positions,
                translations = translations,
                collisionWorld = collisionWorld,
                rotations = rotations,
                scales = scales
            }.Schedule(positions.Length, 128)
        };

        return package;
    }

    //public void Dispose()
    //{
    //    _entities.Dispose();
    //}

    private struct RaycastJob : IJobParallelFor
    {
        [Unity.Collections.ReadOnly] public CollisionWorld collisionWorld;
        [Unity.Collections.ReadOnly] public NativeArray<float2> positions;
        [Unity.Collections.WriteOnly] public NativeArray<Translation> translations;
        [Unity.Collections.WriteOnly] public NativeArray<Rotation> rotations;
        [Unity.Collections.WriteOnly] public NativeArray<NonUniformScale> scales;

        public void Execute(int index)
        {

            var val = positions[index];

            float minY = float.MaxValue;
            float maxY = float.MinValue;

            for (int x = -1; x < 2; x += 1)
            {

                for (int y = -1; y < 2; y += 1)
                {

                    var offset = new float3(x, 0, y) * 4;


                    var ray = new RaycastInput
                    {
                        Start = new float3(val.x, 200, val.y) + offset,
                        End = new float3(val.x, -10, val.y) + offset,
                        Filter = CollisionFilter.Default
                    };

                    collisionWorld.CastRay(ray, out var hit);

                    minY = hit.Position.y < minY ? hit.Position.y : minY;

                    maxY = hit.Position.y > maxY ? hit.Position.y : maxY;

                }
            }

            translations[index] = new Translation()
            {
                Value = new float3(val.x, minY, val.y)
            };

            var rotation = (val.x * 12432.2324f) + (val.y * 993232.122f) + minY * 21223.213324f + maxY * 232133f;



            rotations[index] = new Rotation()
            {
                Value = quaternion.AxisAngle(new float3(0, 1, 0), rotation)
            };

            scales[index] = new NonUniformScale()
            {
                Value =new float3(3, (minY + maxY) * 0.1f,3)
            };
        }
    }

    private struct SetProperties : IJob
    {
        public EntityManager manager;
        public NativeArray<Entity> entities;
        [Unity.Collections.ReadOnly] public NativeArray<Translation> translations;
        [Unity.Collections.ReadOnly] public NativeArray<Rotation> rotations;
        [Unity.Collections.ReadOnly] public NativeArray<NonUniformScale> scale;

        public void Execute()
        {
            for (int i = 0; i < entities.Length; i++)
            {
                manager.SetComponentData(entities[i], translations[i]);
                manager.SetComponentData(entities[i], rotations[i]);
                manager.SetComponentData(entities[i], scale[i]);
            }
                  
        }
    }

    //private void SetProperties(NativeArray<Entity> entities, NativeArray<Translation> translations)
    //{
    //    //public EntityManager manager;
    //    //public NativeArray<Entity> entities;
    //    //[Unity.Collections.ReadOnly] public NativeArray<Translation> translations;
    //
    //    var e = _world.EntityManager;
    //
    //
    //        for (int i = 0; i < entities.Length; i++)
    //        {
    //            e.SetComponentData(entities[i], translations[i]);
    //            e.SetEnabled(entities[i], true);
    //        }                
    //    
    //}

    #endregion

    #region UNLOADING

    public bool Unload()
    {
        Debug.Log($"Unloading Chunk {this._guid}");
        _world.EntityManager.DestroyEntity(_entities);        

        _entities.Dispose();

        return true;
    }

    #endregion
}
