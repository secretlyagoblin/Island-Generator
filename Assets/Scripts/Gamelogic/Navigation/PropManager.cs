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
using System.Runtime.InteropServices;

public class PropManager : MonoBehaviour
{
    public GameState State;

    private Dictionary<Rect, Guid> _manifest;
    private readonly Queue<MinMesh[]> _chunks = new Queue<MinMesh[]>();
    private readonly Queue<Exception> _errors = new Queue<Exception>();

    Entity Prefab;

    public UnityEngine.Material Material;

    public UnityEngine.Material CliffMaterial;

    public GameObject[] SpawnProps;

    public float Threshold;

    private void Awake()
    {
        State.OnSeedChanged += BuildProps;
        //State.OnTerrainLoaded += UpdateCells;

        
        
    }

    private void Start()
    {
        //Prefab = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(PrefabLibrary)).GetSingleton<PrefabLibrary>().Sphere;
    }

    private void BuildProps(GameState state)
    {
        _manifest = Util.DeserialiseFile<Dictionary<Rect, Guid>>(Paths.GetHexGroupManifestPath(State.Seed), new ManifestSerialiser());
        UpdateCells(State);
    }

    void UpdateCells(GameState state)
    {
        var size = 40;

        var pos = Vector3.zero
            //- new Vector3(12, 0, 12)
            ;
        var rect = new Rect(new Vector2(pos.x, pos.z) - Vector2.one * size * 0.5f, Vector2.one * size);

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

                    var divisions = 3;

                    //var inverseMatrix = HexIndex.GetInverseMultiplicationMatrix(divisions); 

                    var subs = hexGroup
                    .Subdivide(2, x => x.Code)
                    .GetNeighbourhoods()
                    //.Subdivide(3, x => x.Code)
                    
                    //.Where(x =>x.Center.Payload.EdgeDistance>0.05f && x.Center.Payload.EdgeDistance < 3)

                    .Select(x => x.GetMinMesh(y => (y.Payload.Height*7) + (y.Payload.EdgeDistance > 0.5 ? (y.Payload.EdgeDistance*2)+0.25f:0),Threshold))
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

    private static float GetHexHeight(Hex hex)
    {
        return hex.Payload.Height;
    }

    private static bool GetHexConnected(Hex hexA, Hex hexB)
    {
        return Mathf.Abs(hexA.Payload.Height - hexB.Payload.Height) < 0.5;
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

            //var meshes = amount.Select(x => x.ToMesh());

            //var combineInstances = meshes.Select((x, i) => new CombineInstance() { mesh = x }).ToArray();


            var verts = new Vector3[amount.Length * 7];
            var tris = new int[amount.Length * 6*3];

            for (int i = 0; i < amount.Length; i++)
            {
                var t = i * 7;
                var a = amount[i];
                verts[t] = a.C;
                verts[t+1] = a.N1;
                verts[t+2] = a.N2;
                verts[t+3] = a.N3;
                verts[t+4] = a.N4;
                verts[t+5] = a.N5;
                verts[t+6] = a.N6;

                var tr = i * 6*3;
                var pos = i * 7;

                tris[tr] = 0 + pos;
                tris[tr +1 ] = 1 + pos;
                tris[tr +2] = 2 + pos;
                tris[tr +3] = 0+ pos;
                tris[tr +4] = 2+ pos;
                tris[tr +5] = 3+ pos;
                tris[tr +6] = 0+ pos;
                tris[tr +7] = 3+ pos;
                tris[tr +8] = 4+ pos;
                tris[tr +9] = 0 + pos;
                tris[tr +10] = 4+ pos;
                tris[tr +11] = 5+ pos;
                tris[tr +12] = 0+ pos;
                tris[tr +13] = 5+ pos;
                tris[tr +14] = 6+ pos;
                tris[tr +15] = 0+ pos;
                tris[tr +16] = 6+ pos;
                tris[tr +17] = 1 + pos;
            }

            var subset = amount.Where(x => x.NeedsWalls).ToArray();

            var edgeVerts = new Vector3[subset.Length * 12];
            var edgeUvs = new Vector2[subset.Length * 12];

            var edgeTris = new int[subset.Length * 12*3];


            for (int i = 0; i < subset.Length; i++)
            {
                var t = i * 12;
                var a = subset[i];
                edgeVerts[t + 0] = a.N1;
                edgeVerts[t + 1] = a.N2;
                edgeVerts[t + 2] = a.N3;
                edgeVerts[t + 3] = a.N4;
                edgeVerts[t + 4] = a.N5;
                edgeVerts[t + 5] = a.N6;
                edgeVerts[t + 6] = a.N1 + (Vector3.down *  10f);
                edgeVerts[t + 7] = a.N2 + (Vector3.down *  10f);
                edgeVerts[t + 8] = a.N3 + (Vector3.down *  10f);
                edgeVerts[t + 9] = a.N4 + (Vector3.down *  10f);
                edgeVerts[t + 10] = a.N5 + (Vector3.down * 10f);
                edgeVerts[t + 11] = a.N6 + (Vector3.down * 10f);

                var offset = RNG.NextFloat(0f, 35);
                offset = 0;

                edgeUvs[t + 0] = new Vector2(0   +offset,  0);
                edgeUvs[t + 1] = new Vector2(1   +offset,  0);
                edgeUvs[t + 2] = new Vector2(0   +offset,  0);
                edgeUvs[t + 3] = new Vector2(1   +offset,  0);
                edgeUvs[t + 4] = new Vector2(0   +offset,  0);
                edgeUvs[t + 5] = new Vector2(1   +offset,  0);
                edgeUvs[t + 6] = new Vector2(0  +offset, 10);
                edgeUvs[t + 7] = new Vector2(1  +offset, 10);
                edgeUvs[t + 8] = new Vector2(0  +offset, 10);
                edgeUvs[t + 9] =new Vector2(1   +offset,  10);
                edgeUvs[t + 10]= new Vector2(0  +offset, 10);
                edgeUvs[t + 11]= new Vector2(1  +offset, 10);







                var tr = i * 12 * 3;
                var pos = i * 12;

                edgeTris[tr + 0] = 6 + pos;
                edgeTris[tr + 1] = 1 + pos;
                edgeTris[tr + 2] = 0 + pos;

                edgeTris[tr + 3] = 6 + pos;
                edgeTris[tr + 4] = 7 + pos;
                edgeTris[tr + 5] = 1 + pos;

                edgeTris[tr + 6] = 7 + pos;
                edgeTris[tr + 7] = 2 + pos;
                edgeTris[tr + 8] = 1 + pos;

                edgeTris[tr + 9] =  7 + pos;
                edgeTris[tr + 10] = 8 + pos;
                edgeTris[tr + 11] = 2 + pos;

                edgeTris[tr + 12] = 8 + pos;
                edgeTris[tr + 13] = 3 + pos;
                edgeTris[tr + 14] = 2 + pos;

                edgeTris[tr + 15] = 8 + pos;
                edgeTris[tr + 16] = 9 + pos;
                edgeTris[tr + 17] = 3 + pos;

                edgeTris[tr + 18] = 9 + pos;
                edgeTris[tr + 19] = 4 + pos;
                edgeTris[tr + 20] = 3 + pos;
                
                edgeTris[tr + 21] = 9 + pos;
                edgeTris[tr + 22] = 10 + pos;
                edgeTris[tr + 23] = 4 + pos;
                
                edgeTris[tr + 24] = 10 + pos;
                edgeTris[tr + 25] = 5 + pos;
                edgeTris[tr + 26] = 4 + pos;
                
                edgeTris[tr + 27] = 10 + pos;
                edgeTris[tr + 28] = 11 + pos;
                edgeTris[tr + 29] = 5 + pos;
                
                edgeTris[tr + 30] = 11+ pos;
                edgeTris[tr + 31] = 0 + pos;
                edgeTris[tr + 32] = 5 + pos;
                
                edgeTris[tr + 33] = 11 + pos;
                edgeTris[tr + 34] = 6 + pos;
                edgeTris[tr + 35] = 0 + pos;
            }    

            var mesh = new Mesh()
            {
                vertices = verts,
                triangles = tris,
                normals = verts.Select(x => Vector3.up).ToArray(),
                uv = verts.Select(x => new Vector2(x.x, x.z)).ToArray()
                
            };

            //mesh.RecalculateNormals();







            // mesh.CombineMeshes(combineInstances, true);

            //Debug.Log($"Building {amount.Length} props");

            var obj = new GameObject();
            obj.name = "Chunk";
            obj.AddComponent<MeshRenderer>().sharedMaterial = Material;
            obj.AddComponent<MeshFilter>().mesh = mesh;

            subset.ToList().ForEach(x =>
            {
                var pos = x.C;

                if (RNG.SmallerThan(0.6f)) return;


                var prop = RNG.GetRandomItem(SpawnProps);

                var scale = RNG.NextFloat(0.8f, 1.2f);

                var ob = GameObject.Instantiate(prop);
                ob.transform.position = pos;
                ob.transform.rotation= Quaternion.Euler(0,RNG.NextFloat(1000f), 0);
                ob.transform.localScale = new Vector3(scale, scale, scale);

                ob.transform.parent = obj.transform;

            });


            var edgeMesh = new Mesh()
            {
                vertices = edgeVerts,
                triangles = edgeTris,
                uv = edgeUvs,
                normals = edgeVerts.Select(x => Vector3.up).ToArray(),
            };

            //edgeMesh.RecalculateNormals();





            // mesh.CombineMeshes(combineInstances, true);

            //Debug.Log($"Building {amount.Length} props");

            var cobj = new GameObject();
            cobj.name = "Edges";
            cobj.AddComponent<MeshRenderer>().sharedMaterial = CliffMaterial;
            cobj.AddComponent<MeshFilter>().mesh = edgeMesh;
            cobj.transform.parent = cobj.transform;
            //InstantiateElements(amount);


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

        var scale = new CompositeScale() { Value = Matrix4x4.Scale(new Vector3(1, 1, 1)) };

        for (int i = 0; i < vectors.Length; i++)
        {
            entities.SetComponentData(nativeArray[i], new Translation() { Value = vectors[i] });
            entities.AddComponentData(nativeArray[i], scale);
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
        private int _m = 1;

        public int Multiplier { get => _m; set => _m = value; }

        protected override void OnUpdate()
        {
            var query = EntityManager.CreateEntityQuery(typeof(SharedAttribute));
            var unset = new SharedAttribute() { State = SharedAttributeType.Unset };
            var set = new SharedAttribute() { State = SharedAttributeType.Set };

            query.SetSharedComponentFilter(unset);

            var collisionWorld = World.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld.CollisionWorld;

            this.Entities.With(query)
                .ForEach((ref Translation translation, ref CompositeScale scale, ref LocalToWorld localToWorld) =>
                {
                    var val = translation.Value;

                    float minY = float.MaxValue;
                    float maxY = float.MinValue;

                    for (int x = -1; x < 2; x+=1)
                    {

                        for (int y = -1; y < 2; y += 1)
                        {

                            var offset = new float3(x, 0, y) * 4;


                            var ray = new RaycastInput
                            {
                                Start = new float3(val.x, 200, val.z)+ offset,
                                End = new float3(val.x, -10, val.z)+ offset,
                                Filter = CollisionFilter.Default
                            };

                            collisionWorld.CastRay(ray, out var hit);

                            minY = hit.Position.y < minY ? hit.Position.y : minY;

                            maxY = hit.Position.y > maxY ? hit.Position.y : maxY;

                        }
                    }

                    translation.Value = new float3(val.x, (minY + maxY) * 0.5f, val.z);

                    scale.Value = new float4x4(
                        _m, 0,          0,  0,
                        0,  maxY-minY,  0,  0,
                        0,  0,          _m, 0,
                        0,  0,          0,  1
                        );





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