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

public class PropRegion
{
    private Vector2 _center;
    private Rect _bounds;
    private Guid _guid;
    private GameState _state;
    private const int MULTIPLIER = 8;

    private NativeArray<Entity> _entities;


    public PropRegion(GameState state, Rect bounds, Guid id)
    {
        _state = state;
        _bounds = bounds;
        _guid = id;

        _center = bounds.center * MULTIPLIER;

    }

    public void Update(Vector2 sample)
    {
        if (ContinueLoading()) return;


        //if (_propState == PropRegionState.Loading)
        //{
        //    if (!_runningTask.IsCompleted)
        //        return;
        //
        //    _propState = PropRegionState.Loaded;
        //}
        //
        //var distance = Vector3.Distance(sample, _center);
        //PropRegionState targetState;
        //
        //
        //if (distance > 50)
        //{
        //    switch (_propState)
        //    {
        //        case PropRegionState.Unloaded:
        //            break;
        //        case PropRegionState.Loaded:
        //            break;
        //        case PropRegionState.Displayed:
        //            break;
        //        default:
        //            break;
        //    }
        //}
        //else
        //{
        //    switch (_propState)
        //    {
        //        case PropRegionState.Unloaded:
        //            targetState = PropRegionState.Loading;
        //            _runningTask = Task.Run(LoadHexgroupFromFileAndPopulatePositions).;
        //
        //
        //            break;
        //        case PropRegionState.Loaded:
        //            break;
        //        case PropRegionState.Displayed:
        //            break;
        //        default:
        //            break;
        //    }
        //}
    }




    #region ENTITIES

    private LoadingStep _loading = LoadingStep.NotStarted;

    private Queue<Exception> _exceptions = new Queue<Exception>();

    private enum LoadingStep{ 
        NotStarted = 0,
        LoadingHexgroupFromFile = 1,
        Raycasting = 2,
        BuildingEntities = 3,
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

                _loadingHexgroupTask = Task.Run(LoadHexgroupFromFileAndPopulatePositions);
                _loading = LoadingStep.LoadingHexgroupFromFile;

                return true;
            case LoadingStep.LoadingHexgroupFromFile:

                if (!_loadingHexgroupTask.IsCompleted) return true;

                _raycastJobResult = InstantiateElements(_float3s);

                //_float3s.Clear();

                _loading = LoadingStep.Raycasting;





                return true;

            case LoadingStep.Raycasting:

                if (!_raycastJobResult.handle.IsCompleted) return true;

                var color = Color.white;

                _raycastJobResult.handle.Complete();

                //_raycastJobResult.positions.ToList().ForEach(x =>
                //
                //    Debug.DrawRay(x, Vector3.up * 6, color, 100f)
                //);


                //_entities = new NativeArray<Entity>(_raycastJobResult.positions.Length, Allocator.Persistent);
                //
                //var mananger = World.DefaultGameObjectInjectionWorld;
                //
                //mananger.EntityManager.Instantiate(
                //
                //mananger.EntityManager.SetComponentData()
                //
                //
                //
                _raycastJobResult.positions.Dispose();
                _raycastJobResult.world.Dispose();

                _loading = LoadingStep.BuildingEntities;

                return true;

            case LoadingStep.BuildingEntities:

                _loading = LoadingStep.Complete;

                return true;

            case LoadingStep.Complete:
                return false;
            default:
                throw new InvalidEnumArgumentException();
        }
    }

    private Task _runningTask;

    //Getting Entities

    private List<float3> _float3s = new List<float3>();

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
            .Select(x => new float3(x))
            .ToList();

            {
                lock (_float3s)
                {
                    _float3s = subs;
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
        public NativeArray<float3> positions;
        public JobHandle handle;
        public CollisionWorld world;
    }

    RaycastJobResult InstantiateElements(List<float3> vectors)
    {
        if (vectors is null)
        {
            throw new ArgumentNullException(nameof(vectors));
        }

        var world = World.DefaultGameObjectInjectionWorld;
        var entities = world.EntityManager;
        //var collision = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld.CollisionWorld;

        //Debug.Log($"Casting many rays ({vectors.Length})");

        var positions = new NativeArray<float3>(vectors.ToArray(), Allocator.Persistent);

        var physicsWorld = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld;
        var realCollision = physicsWorld.CollisionWorld;


        var collisionWorld = realCollision.Clone();

        collisionWorld.BuildBroadphase(ref physicsWorld, 0f, float3.zero);

        var package = new RaycastJobResult()
        {
            positions = positions,
            world = collisionWorld,
            handle = new RaycastJob()
            {
                translations = positions,
                collisionWorld = collisionWorld
            }.Schedule(positions.Length, 128)
        };

        return package;
    }

    private struct RaycastJob : IJobParallelFor
    {
        [Unity.Collections.ReadOnly] public CollisionWorld collisionWorld;
        public NativeArray<float3> translations;

        public void Execute(int index)
        {

            var translation = translations[index];


            var val = translation;

            float minY = float.MaxValue;
            float maxY = float.MinValue;

            for (int x = -1; x < 2; x += 1)
            {

                for (int y = -1; y < 2; y += 1)
                {

                    var offset = new float3(x, 0, y) * 4;


                    var ray = new RaycastInput
                    {
                        Start = new float3(val.x, 200, val.z) + offset,
                        End = new float3(val.x, -10, val.z) + offset,
                        Filter = CollisionFilter.Default
                    };

                    collisionWorld.CastRay(ray, out var hit);

                    minY = hit.Position.y < minY ? hit.Position.y : minY;

                    maxY = hit.Position.y > maxY ? hit.Position.y : maxY;

                }
            }

            translations[index] = new float3(val.x, (minY + maxY) * 0.5f, val.z);
        }
    }

    #endregion
}
