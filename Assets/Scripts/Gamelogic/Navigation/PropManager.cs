using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using WanderingRoad;
using WanderingRoad.Procgen.RecursiveHex;
using WanderingRoad.Procgen.RecursiveHex.Json;
using WanderingRoad.Random;

using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Jobs;

public class PropManager : MonoBehaviour
{
    public GameState State;

    private Dictionary<Rect, Guid> _manifest;
    private readonly Queue<float3[]> _chunks = new Queue<float3[]>();
    private readonly Queue<Exception> _errors = new Queue<Exception>();

    Entity Prefab;

    private void Awake()
    {
        State.OnSeedChanged += BuildProps;
        State.OnTerrainLoaded += UpdateCells;
        
    }

    private void Start()
    {
        Prefab = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(PrefabLibrary)).GetSingleton<PrefabLibrary>().Sphere;
    }

    private void BuildProps(GameState state)
    {
        _manifest = Util.DeserialiseFile<Dictionary<Rect, Guid>>(Paths.GetHexGroupManifestPath(State.Seed), new ManifestSerialiser());
    }

    void UpdateCells(GameState state)
    {
        var pos = Vector3.zero
            //- new Vector3(12, 0, 12)
            ;

        var scale = 100;

        var rect = new Rect(new Vector2(pos.x, pos.z) - Vector2.one * 1 * scale, Vector2.one * 2*scale);

        rect.DrawRect(Color.red, 100f);

        //_manifest.ToList().ForEach(x => x.Key.DrawRect(Color.blue, 100f));

        var cells = _manifest
            .Where(x => x.Key.Overlaps(rect)).ToList();

        foreach (var cell in cells)
        {
            Task.Run(() =>
            {
                try
                {
                    var hexGroup =
                     Util.DeserialiseFile<HexGroup>(
                        Paths.GetHexGroupPath(state.Seed, cell.Value.ToString()),
                        new HexGroupConverter());

                    //var subDivide = hexGroup.Subdivide(3, x => x.Code);

                    var divisions = 1;

                    var inverseMatrix = HexIndex.GetInverseMultiplicationMatrix(divisions); 

                    var subs = hexGroup
                    .Subdivide(divisions, x => x.Code)
                    .GetHexes()
                    .Where(x =>x.Payload.EdgeDistance>0.5f && x.Payload.EdgeDistance < 3)
                    //.Select(x=> x.Index.Position3d*8)
                    .Select(x => inverseMatrix.MultiplyPoint(x.Index.Position3d)*8)
                    .Select(x => new float3(x))
                    .Chunk(800)
                    .ToList()

                    ;

                    //{ lock (_errors) { _errors.Enqueue("should be loading a fuckin chunk"); } }

                    {
                        lock (_chunks)
                        {
                            subs.ForEach(x => _chunks.Enqueue(x.ToArray()));
                            //_chunks.Enqueue(subs);
                        }
                    }


                }
                catch (Exception ex)
                {
                    { lock (_errors) { _errors.Enqueue(ex); } }
                }
            });
        }
        
        //Debug.Log($"{cells.Count} overlapping cells");
        
        cells.ForEach(x => x.Key.DrawRect(Color.blue, 100f));
    }

    JobPackage _package = default;
    private bool _doPackage = false;

    void Update()
    {
        RNG.DateTimeInit();

        while (_errors.Count > 0)
            Debug.LogError(_errors.Dequeue());



        if (_chunks.Count > 0 && _doPackage == false)
        {
            //var color = RNG.NextColorBright();
            var amount = _chunks.Dequeue();

                //Debug.Log($"Building {amount.Length} props");

                _doPackage = true;

            _package = InstantiateElements(amount);


            //for (int i = 0; i < amount.Length; i++)
            //{
            //    Physics.Raycast(amount[i] * 8 + Vector3.up*50, Vector3.down, out var hit);
            //
            //    Debug.DrawRay(hit.point, Vector3.up * 5, color,100f);
            //}
        }

    }

    private struct JobPackage{
        public NativeArray<float3> positions;
        public JobHandle handle;
        public CollisionWorld world;
    }

    public void LateUpdate()
    {
        if (_doPackage)
        {
            var c = RNG.NextColorBright();
        
        
            if (_package.handle.IsCompleted)
            {

                _package.handle.Complete();

                _package.positions.ToList().ForEach(x =>
        
                    Debug.DrawRay(x, Vector3.up * 6, c, 100f)
                );
        
                _package.positions.Dispose();
                _package.world.Dispose();
                _doPackage = false;
        
                //Debug.Log("Did an iteration");
            }
        }
    }

    JobPackage InstantiateElements(float3[] vectors)
    {

        var world = World.DefaultGameObjectInjectionWorld;
        var entities = world.EntityManager;
        //var collision = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld.CollisionWorld;

        //Debug.Log($"Casting many rays ({vectors.Length})");

        var positions = new NativeArray<float3>(vectors, Allocator.TempJob);

        var physicsWorld = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld;
        var realCollision = physicsWorld.CollisionWorld;


        var collisionWorld = realCollision.Clone();

        collisionWorld.BuildBroadphase(ref physicsWorld,0f,float3.zero);

        var package = new JobPackage()
        {
            positions = positions,
            world = collisionWorld,
            handle = new Raycast() { 
                translations = positions,
                collisionWorld = collisionWorld
            }.Schedule(positions.Length,128)
        };

        //package.handle.Complete();

        //var c = RNG.NextColorBright();

        //package.positions.ToList().ForEach(x =>
        //
        //    Debug.DrawRay(x, Vector3.up * 1000, c, 100f)
        //);

        //positions.Dispose();

        return package;






        //var positionArray 
        //
        //var nativeArray = new NativeArray<Entity>(vectors.Length, Allocator.TempJob);
        //
        //entities.Instantiate(Prefab, nativeArray);


        //var handle = new MakeEntities()
        //{
        //    state = new SharedAttribute() { State = SharedAttributeType.Unset },
        //    entities = nativeArray,
        //    entityManager = entities,
        //    positions = positionArray
        //}.Schedule(100, 30);



        //var shared = new SharedAttribute() { State = SharedAttributeType.Unset };
        //
        //var scale = new CompositeScale() { Value = Matrix4x4.Scale(new Vector3(1, 1, 1)) };
        //
        //var up = new float3(0, 1, 0);
        //
        //for (int i = 0; i < vectors.Length; i++)
        //{
        //    entities.SetComponentData(nativeArray[i], new Translation() { Value = vectors[i] });
        //    entities.AddComponentData(nativeArray[i], scale);
        //    entities.AddComponentData(nativeArray[i], new Rotation() { Value = quaternion.AxisAngle(up, i * 21343.234f) });
        //    entities.AddSharedComponentData(nativeArray[i], shared);
        //}

        //nativeArray.Dispose();

        //var cast = new Raycast();

        //var handle = new DisposeArrays()
        //{
        //
        //    positions = positionArray,
        //    entityManager = entities,
        //    state = new SharedAttribute() { State = SharedAttributeType.Unset }
        //}.Schedule();

        //var entitySystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<MyComponentSystem>();

        //entitySystem.DoParallel();
    }



    public struct DisposeArrays : IJob
    {
        public NativeArray<float3> positions;
        public NativeArray<Entity> entities;
        public EntityManager entityManager;
        public SharedAttribute state;

        public void Execute()
        {
            for (int i = 0; i < entities.Length; i++)
            {
                entityManager.SetComponentData(entities[i], new Translation() { Value = positions[i] });
                entityManager.AddComponentData(entities[i], new CompositeScale() { Value = Matrix4x4.Scale(new Vector3(1, 1, 1)) });
                entityManager.AddComponentData(entities[i], new Rotation() { Value = quaternion.AxisAngle(new float3(0, 1, 0), i * 21343.234f) });
                entityManager.AddSharedComponentData(entities[i], state);
            }

            positions.Dispose();
            entities.Dispose();
        }
    }


        public struct MakeEntities : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<float3> positions;
        [ReadOnly]
        public NativeArray<Entity> entities;
        [ReadOnly]
        public EntityManager entityManager;
        [ReadOnly]
        public SharedAttribute state;

        public void Execute(int index)
        {
            var entity = entities[index];

            entityManager.SetComponentData(entity, new Translation() { Value = positions[index] });
            entityManager.AddComponentData(entity, new CompositeScale() { Value = Matrix4x4.Scale(new Vector3(1, 1, 1)) });
            entityManager.AddComponentData(entity, new Rotation() { Value = quaternion.AxisAngle(new float3(0,1,0), index * 21343.234f) });
            entityManager.AddSharedComponentData(entity, state);
        }
    }


    public struct Raycast : IJobParallelFor
    {
        [ReadOnly] public CollisionWorld collisionWorld;
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


    public struct SharedAttribute : ISharedComponentData
    {
        public SharedAttributeType State;
    }
    
    public enum SharedAttributeType
    {
        Unset,Set
    }


    /*

    public class MyComponentSystem : SystemBase
    {



        //protected override void OnUpdate() { }


        protected override void OnUpdate()
        {

            var unset = new SharedAttribute() { State = SharedAttributeType.Unset };



            var query = EntityManager.CreateEntityQuery(typeof(SharedAttribute));
            var set = new SharedAttribute() { State = SharedAttributeType.Set };

            query.SetSharedComponentFilter(unset);

            var collisionWorld = World.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld.CollisionWorld;

            var handle = new JobHandle();

            handle = this.Entities
                .WithSharedComponentFilter(unset)
                .ForEach((ref Translation translation, ref CompositeScale scale, ref LocalToWorld localToWorld) =>
                {
                    var val = translation.Value;

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

                    translation.Value = new float3(val.x, (minY + maxY) * 0.5f, val.z);

                    scale.Value = new float4x4(
                        1, 0, 0, 0,
                        0, (maxY - minY) * 0.4f, 0, 0,
                        0, 0, 1, 0,
                        0, 0, 0, 1
                        );

                }).ScheduleParallel(handle);

            this.Dependency = handle;



            //this.EntityManager.SetSharedComponentData(query, set);
        }


    }
    */


    //private class MyJob : IJobParallelFor
    //{
    //    [ReadOnly]
    //    public NativeArray<float2> Cells;
    //
    //    [ReadOnly]
    //    public CollisionWorld CollisionWorld;
    //
    //    public World World;
    //
    //    public Entity Prefab;
    //
    //    public void Execute(int index)
    //    {
    //        var xz = Cells[index];
    //
    //        var point = new RaycastInput
    //        {
    //            Start = new float3(xz.x, 50, xz.y),
    //            End = new float3(xz.x, 0, xz.y)
    //        };
    //
    //        CollisionWorld.CastRay(point, out var hit);
    //
    //        var entity = World.EntityManager.Instantiate(Prefab);
    //
    //        World.EntityManager.SetComponentData<Translation>(entity,new Translation() { Value = hit.Position });
    //    }
    //}
}