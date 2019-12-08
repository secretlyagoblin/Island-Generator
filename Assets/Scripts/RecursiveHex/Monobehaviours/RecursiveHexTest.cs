using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RecursiveHex;
using System.Linq;

public class RecursiveHexTest : MonoBehaviour
{
    public GameObject Prefab;
    public GameObject BorderPrefab;

    // Start is called before the first frame update
    void Start()
    {
        RNG.Init("I'd kill");
        //RNG.Init();
        RandomSeedProperties.SetRandomSeed(RNG.NextFloat(-1000, 1000), RNG.NextFloat(-1000, 1000));

        var codeIdentifier = new Func<HexPayload, int>(x => x.Code);
        var connector = new Func<HexPayload, int[]>(x => x.Connections.ToArray());

        var setRegionRemapper = new Func<HexPayload, Connection, int[], HexPayload>((x,nodeStatus, connections) => { 
                var done = x;
                done.Connections = new CodeConnections(connections);
            done.ConnectionStatus = nodeStatus;
            done.Region = done.Code;
                return done;
            });

        var standardRemapper = new Func<HexPayload, Connection, int[], HexPayload>((x, nodeStatus, connections) => {
            var done = x;

            done.ConnectionStatus = nodeStatus;
            done.Connections = new CodeConnections(connections);
            return done;
        });

        //RandomSeedProperties.Disable();


        //var code = 0;

        //var finalMeshes = new HexGroup()
        //    .Subdivide()
        //    .ForEach(x => { x.Height = RNG.NextFloat(10); x.Code = code; code++; })
        //    .Subdivide()
        //    .Subdivide(x => x.Code)
        //    .ForEachHexGroup(x => x.Subdivide())
        //    .ForEachHexGroup(x => x.ToMesh());

        //var code = 0;

        var colours = new List<Color>();

        for (int i = 0; i < 1000; i++)
        {
            colours.Add(RNG.NextColor());
        }

        var layer1 = new HexGroup().ForEach(x => new HexPayload() { Code = 1, Height = 0, Color = Color.white });

        var layer2 = layer1
            .Subdivide()
            //.ForEach((x, i) => new HexPayload() { Code = 1, Height = 0, Color = Color.white })
            //.Subdivide();
            ;

        var graff = layer2.ToGraph<Levels.SingleConnectionGraph>(codeIdentifier, connector);

        layer2.MassUpdateHexes(
            graff
            .Finalise(setRegionRemapper));

        //graff.DebugDrawSubmeshConnectivity(Color.red);
        //
        //layer2.ToGameObjects(Prefab);
        //
        //return;



        var groups = Enumerable.Range(1, 7);
        //var groups = new List<int>() { 1, 3 };

        var layer3 = layer2.GetSubGroup(x => groups.Contains(x.Payload.Code)).Subdivide();



        var graph3 = layer3.ToGraph<Levels.SingleConnectionGraph>(codeIdentifier, connector);      
        layer3.MassUpdateHexes(graph3.Finalise(standardRemapper));
        //graph3.DebugDrawSubmeshConnectivity(colours[0]);
        //layer3.ToGameObjects(Prefab);

        //return;

        //var layer4 = layer3.Subdivide().Subdivide();

        //var graph4 = layer4.ToGraph<Levels.SingleConnectionGraph>(identifier, connector);
        //layer4.MassUpdateHexes(graph4.Finalise(standardRemapper));
        //graph4.DebugDrawSubmeshConnectivity(colours[0]);
        //layer4.ToGameObjects(Prefab);
        //
        //return;

        var layer4 = layer3.Subdivide();
        //layer4.ToGameObjects(Prefab);
        //var graph4 = layer4.ToGraph<Levels.NoBehaviour>(identifier, connector);
        //layer4.MassUpdateHexes(graph4.Finalise(standardRemapper));

        //return;


        var subGraphs = layer4.GetSubGroups(x => x.Payload.Region);
        
        var iterator = -1;
        subGraphs.ForEach(x =>
        {
            iterator++;

            var obj = x;
            //obj.ToGameObjects(Prefab);
            //return;

            var color = RNG.NextColor();

            var finalLayer = obj.ToGraph<Levels.SingleConnectionGraph>(u => u.Code, connector);
            var nodes = finalLayer.Finalise(standardRemapper);
            obj.MassUpdateHexes(nodes);

            obj.GetSubGroups(y => y.Payload.Code).ForEach(y => {

                var next = y.Subdivide();

                Color.RGBToHSV(color, out var h, out var s, out var v);
                var shiftedColor = Color.HSVToRGB(h + RNG.NextFloat(-0.1f, 0.1f), 1, 0.8f);

                var matt = new Material(Prefab.GetComponent<MeshRenderer>().sharedMaterial);
                matt.color = shiftedColor;

                var gobject = new GameObject();
                gobject.AddComponent<MeshFilter>().sharedMesh = next.ToMesh();
                gobject.AddComponent<MeshRenderer>().sharedMaterial = matt;


            });


            //obj.ToGameObjects(Prefab);
            //finalLayer.DebugDrawSubmeshConnectivity(colours[iterator]);
        
        
        });

        //var layer4 = layer3.Subdivide().ToGraph<Levels.NoBehaviour>(identifier, connector).DebugDrawSubmeshConnectivity(Color.white);

        // return;
        //
        // for (int i = 1; i < 8; i++)
        // {
        //     var layer = layer2.GetSubGroup(x => x.Payload.Code == i).Subdivide();
        //     var innerGraph = layer.ToGraph<Levels.HighLevelConnectivity>(identifier, connector);
        //
        //     //innerGraph.DebugDrawSubmeshConnectivity(colours[i]);
        //
        //     var bersults = innerGraph.Finalise( (x, connections) => {
        //         var done = x;
        //         done.Connections = new CodeConnections(connections);
        //         return done;
        //     });
        //
        //     layer.MassUpdateHexes(bersults);
        //
        //     var mesh = layer.Subdivide();
        //
        //     mesh.ToGraph<Levels.NoBehaviour>(identifier, connector).DebugDrawSubmeshConnectivity(colours[i]);
        //
        //     Finalise(mesh);
        // }
        //
        // return;

        //layer2.ToGameObjects(Prefab);
        //layer2.ToGameObjectsBorder(BorderPrefab);

        //var identifier = new Func<HexPayload, int>(x => x.Code);
        //var connector = new Func<HexPayload, int[]>(x => x.Connections.ToArray());

        //var graph = layer2
        //    .ToGraph(
        //        identifier,
        //        connector)
        //    .ApplyBlueprint(HighLevelConnectivity.CreateSingleRegion);
        //    //.DebugDrawSubmeshConnectivity(transform);
        //
        //var results = graph.Finally((x,connections) => {
        //    var done = x;
        //    done.Connections = new CodeConnections(connections);
        //    return done;
        //});
        //
        //layer2.MassUpdateHexes(results);
        //
        //var layer3 = layer2
        //    .Subdivide();
        //
        //
        //var graph2 = layer3.ToGraph(identifier, connector)
        //    //.DebugDraw(this.transform)
        //    .DebugDrawSubmeshConnectivity(colours[0]);




        //var layer2 = layer1.Subdivide()//.Subdivide()//.Subdivide();
        //    .ForEach((x, i) => new HexPayload()
        //    {
        //        Height = RNG.NextFloat(0, 5),
        //        Color = RNG.NextColor(),
        //        //Color = x.Index == Vector2Int.zero ? Color.white:Color.black,
        //        Code = i
        //    }).Subdivide()
        //                .ForEach((x, i) => new HexPayload()
        //                {
        //                    Height = x.Payload.Height + RNG.NextFloat(-1, 1),
        //                    Color = x.Payload.Color,
        //                    //Color = x.Index == Vector2Int.zero ? Color.white:Color.black,
        //                    Code = i
        //                }).Subdivide()
        //                                       .ForEach((x, i) => new HexPayload()
        //                                       {
        //                                           Height = x.Payload.Height + RNG.NextFloat(-0.25f, 0.25f),
        //                                           Color = x.Payload.Color,
        //                                           //Color = x.Index == Vector2Int.zero ? Color.white:Color.black,
        //                                           Code = i
        //                                       }).Subdivide();
        ;



            //var layer3 = layer2.Subdivide().Subdivide()//.Subdivide()//.Subdivide()
            //    .ForEach(x => new HexPayload()
            //    {
            //        Height = 0f,
            //        //Color = x.Payload.Color,
            //        Color = x.IsBorder?Color.black:colours[x.Payload.Code],
            //        Code = x.Payload.Code
            //    });
            //    ;

            //this.StartCoroutine(FinaliseHexgroup(
            //    layer3.GetSubGroups(x => x.Payload.Code),
            //    x => Finalise(x.Subdivide()))
            //    );

            //layer3.GetSubGroups(x => x.Payload.Code).ForEach(x=> Finalise(x.Subdivide().Subdivide()));

            //.Subdivide();
            //.Subdivide().Subdivide().Subdivide()//.Subdivide().Subdivide();
            ;

        //layer2.ToGameObjects(Prefab);
        //Finalise(layer2);
        //layer3.ToGameObjectsBorder(BorderPrefab);
        //
        //this.gameObject.GetComponent<MeshFilter>().sharedMesh = subgroup.ToMesh();//(x => x.Payload.Height);

    }

    IEnumerator FinaliseHexgroup(List<HexGroup> hexGroup, Action<HexGroup> func)
    {
        for (int i = 0; i < hexGroup.Count; i++)
        {
            func(hexGroup[i]);
            yield return null;
        }
    }

    private void Finalise(HexGroup group)
    {
        var gobject = new GameObject();
        gobject.name = "Subregion";
        var renderer = gobject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = this.GetComponent<MeshRenderer>().sharedMaterial;
        gobject.AddComponent<MeshFilter>().sharedMesh = group.ToMesh();
        gobject.transform.parent = this.transform;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(Vector3.up, 5f * Time.deltaTime);
    }
}
namespace Levels{

    public abstract class HexGraph : Graph<HexPayload>
    {
        public HexGraph(Vector3[] verts, int[] tris, HexPayload[] nodes, Func<HexPayload, int> identifier, Func<HexPayload, int[]> connector) : base(verts, tris, nodes, identifier, connector)
        {
        }


    }

    public class SingleConnectionGraph : HexGraph
    {
        public SingleConnectionGraph(Vector3[] verts, int[] tris, HexPayload[] nodes, Func<HexPayload, int> identifier, Func<HexPayload, int[]> connector) : base(verts, tris, nodes, identifier, connector)
        {
        }

        protected override void Generate()
        {
            for (int i = 0; i < _collection.Meshes.Length; i++)
            {
                var mesh = _collection.Meshes[i];

                if (mesh.Id < 0)
                {
                    continue;
                }

                mesh.SetConnectivity(LevelGen.States.ConnectEverything);
                mesh.SetConnectivity(LevelGen.States.RemoveUnnecessaryCriticalNodesAssumingHexGrid);
            }

            for (int i = 0; i < _nodeMetadata.Length; i++)
            {
                _nodeMetadata[i].Code = i + 1;
            }

            _collection.Bridges.LeaveSingleRandomConnection();
            _collection.MarkBridgeInterfacesAsCritical();

        }
    }

    public class HighLevelConnectivity : HexGraph
    {
    public HighLevelConnectivity(
        Vector3[] verts,
        int[] tris,
        HexPayload[] nodes,
        Func<HexPayload, int> identifier,
        Func<HexPayload, int[]> connector) : base(verts, tris, nodes, identifier, connector) { }

    protected override void Generate()
    {    

            for (int i = 0; i < _collection.Meshes.Length; i++)
            {
                var mesh = _collection.Meshes[i];

                if (mesh.Id < 0)
                {
                    continue;
                }

                mesh.SetConnectivity(LevelGen.States.DikstraWithRooms);
            }

            for (int i = 0; i < _nodeMetadata.Length; i++)
            {
                _nodeMetadata[i].Code = i + 1;
            }
    }
}

public class NoBehaviour : HexGraph
    {
    public NoBehaviour(Vector3[] verts,
        int[] tris,
        HexPayload[] nodes,
        Func<HexPayload, int> identifier,
        Func<HexPayload, int[]> connector) : base(verts, tris, nodes, identifier, connector) { }

    protected override void Generate()
    {

            throw new Exception("NoBehaviour HexGraph should never be finalised, it's for preview only.");
    }
}

    public class MinimiseCriticalNodes : HexGraph
    {
        public MinimiseCriticalNodes(Vector3[] verts,
            int[] tris,
            HexPayload[] nodes,
            Func<HexPayload, int> identifier,
            Func<HexPayload, int[]> connector) : base(verts, tris, nodes, identifier, connector) { }

        protected override void Generate()
        {
            for (int i = 0; i < _collection.Meshes.Length; i++)
            {
                var mesh = _collection.Meshes[i];

                if (mesh.Id < 0)
                {
                    continue;
                }

                mesh.SetConnectivity(LevelGen.States.ConnectEverything);
                mesh.SetConnectivity(LevelGen.States.RemoveUnnecessaryCriticalNodesAssumingHexGrid);
            }

            for (int i = 0; i < _nodeMetadata.Length; i++)
            {
                _nodeMetadata[i].Code = i + 1;
            }

            _collection.Bridges.LeaveSingleRandomConnection();
        }
    }
}
