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
using Unity.Jobs.LowLevel.Unsafe;

public class PropManager : MonoBehaviour
{
    public GameState State;

    private Dictionary<Rect, Guid> _manifest;
    private readonly Queue<Vector3[]> _chunks = new Queue<Vector3[]>();
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
        var rect = new Rect(new Vector2(pos.x, pos.z) - Vector2.one * 1, Vector2.one * 2);

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

                    var subs = hexGroup
                    //.Subdivide(3, x => x.Code)
                    .GetHexes()
                    .Where(x =>x.Payload.EdgeDistance>0.05f && x.Payload.EdgeDistance < 3)
                    .Select(x => x.Index.Position3d)
                    .ToArray();

                    //{ lock (_errors) { _errors.Enqueue("should be loading a fuckin chunk"); } }

                    {
                        lock (_chunks)
                        {
                            _chunks.Enqueue(subs);
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

    void Update()
    {
        RNG.DateTimeInit();

        while (_errors.Count > 0)
            Debug.LogError(_errors.Dequeue());

        if (_chunks.Count > 0)
        {
            //var color = RNG.NextColorBright();
            var amount = _chunks.Dequeue();

            Debug.Log($"Building {amount.Length} props");

            InstantiateElements(amount);


            //for (int i = 0; i < amount.Length; i++)
            //{
            //    Physics.Raycast(amount[i] * 8 + Vector3.up*50, Vector3.down, out var hit);
            //
            //    Debug.DrawRay(hit.point, Vector3.up * 5, color,100f);
            //}
        }

    }

    void InstantiateElements(Vector3[] vectors)
    {

        var world = World.DefaultGameObjectInjectionWorld;
        var entities = world.EntityManager;
        //var collision = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld.CollisionWorld;

        var nativeArray = new NativeArray<Entity>(vectors.Length, Allocator.Temp);

        entities.Instantiate(Prefab, nativeArray);

        var shared = new SharedAttribute() { State = SharedAttributeType.Unset };

        for (int i = 0; i < vectors.Length; i++)
        {
            entities.SetComponentData(nativeArray[i], new Translation() { Value = vectors[i] });
            entities.AddSharedComponentData(nativeArray[i], shared);
        }

        nativeArray.Dispose();
    }

    public struct SharedAttribute : ISharedComponentData
    {
        public SharedAttributeType State;
    }
    
    public enum SharedAttributeType
    {
        Unset,Set
    }

    public class MyComponentSystem : ComponentSystem
    {
        CollisionWorld CollisionWorld;

        protected override void OnCreate()
        {
            base.OnCreate();

            CollisionWorld = World.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld.CollisionWorld;
        }


        protected override void OnUpdate()
        {
            var query = EntityManager.CreateEntityQuery(typeof(SharedAttribute));
            var unset = new SharedAttribute() { State = SharedAttributeType.Unset };
            var set = new SharedAttribute() { State = SharedAttributeType.Set };

            query.SetSharedComponentFilter(unset);


            this.Entities.With(query)
                .ForEach((ref Translation translation) =>
                {
                    var val = translation.Value;

                    var ray = new RaycastInput
                    {
                        Start = new float3(val.x, 200, val.z),
                        End = new float3(val.x, -10, val.z),
                        Filter = CollisionFilter.Default
                    };

                    if(CollisionWorld.CastRay(ray, out var hit))
                        translation.Value = hit.Position;
                });

            this.EntityManager.SetSharedComponentData(query, set);

            
        }
    }

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