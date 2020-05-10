using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using WanderingRoad.Core.Random;
using WanderingRoad.Procgen.RecursiveHex;
using WanderingRoad.Procgen.Topology;
using WanderingRoad.Core;

namespace WanderingRoad.Procgen.Levelgen
{
    public class RecursiveHexTest : MonoBehaviour
    {
        public GameObject Prefab;
        public GameObject BorderPrefab;
        public Mesh PreviewMesh;

        public bool Preview = true;

        private List<HexGroupVisualiser> _gizmosHexGroups;// = new HexGroupVisualiser(PreviewMesh,)

        // Start is called before the first frame update
        void Start()
        {
            _gizmosHexGroups = new List<HexGroupVisualiser>() { };//HexGroupVisualiser(PreviewMesh);

            //RNG.Init("I'd kill fill zill");
            //cool seed "4/27/2020 7:45:28 PM"
            // orphan node to test.. "4/27/2020 5:56:43 PM"
            RNG.Init("3/15/2020 5:58:48 PM");
            // dead end to test... RNG.Init("5/2/2020 5:04:24 PM");
            //RNG.DateTimeInit();
            //RecursiveHex.RandomSeedProperties.Disable();

            var regionIdentifier = new Func<HexPayload, int>(x => x.Region);
            var codeIdentifier = new Func<HexPayload, int>(x => x.Code);
            var interiorExterior = new Func<HexPayload, int>(x => x.ConnectionStatus == Connection.NotPresent? 1:2);
            var connector = new Func<HexPayload, int[]>(x => x.Connections.ToArray());

            var setRegionRemapper = new Func<HexPayload, Connection, int[], HexPayload>((x, nodeStatus, connections) =>
            {
                var done = x;
                done.Connections = new CodeConnections(connections);
                done.ConnectionStatus = nodeStatus;
                done.Region = done.Code;
                return done;
            });

            var standardRemapper = new Func<HexPayload, Connection, int[], HexPayload>((x, nodeStatus, connections) =>
            {
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

            var layer2 = layer1;


                RandomXY.SetRandomSeed(RNG.NextFloat(-1000, 1000), RNG.NextFloat(-1000, 1000));

                //var splayers = new List<HexGroup>(){layer1
                //    .Subdivide(4, codeIdentifier)
                //    .ApplyGraph<Levels.HighLevelConnectivity>(codeIdentifier, connector,true)
                //    .ForEach(x => new HexPayload(x.Payload) { Region = x.Payload.Code })
                //    .Subdivide(8, codeIdentifier)
                //    .ApplyGraph<InterconnectionLogic>(true)
                //                        .ForEach(x => new HexPayload(x.Payload)
                //    {
                //        //Color = x.Payload.ConnectionStatus == Connection.Present ? randomColor : x.Payload.ConnectionStatus == Connection.Critical ? randomColor : randomColorDark//RNG.NextColorBright()
                //        Color = x.Payload.ConnectionStatus == Connection.NotPresent ? new Color(x.Payload.Height * 0.1f, x.Payload.Height * 0.1f, x.Payload.Height * 0.1f) : new Color(x.Payload.Height * 0.3f, 0.6f, x.Payload.Height * 0.3f),
                //        Height = (x.Payload.ConnectionStatus == Connection.NotPresent ? x.Payload.Height+3 : 1f) + RandomXY.GetOffset(x.Index.Position3d.x,x.Index.Position3d.z).Distance
                //    })
                //.Subdivide(2,codeIdentifier)
                //.ApplyGraph<TestBed>(codeIdentifier, connector, false)
                //.ApplyGraph<ApplyBounds>(interiorExterior, connector, false)
                //.ApplyGraph<PostprocessTerrain>(codeIdentifier, connector, true)
                //.Subdivide(2, codeIdentifier)
                //};

                var splayers = layer1
                    .Subdivide(4, codeIdentifier)
                    .ApplyGraph<Levels.HighLevelConnectivity>(codeIdentifier, connector, true)
                    .ForEach(x => new HexPayload(x.Payload) { Region = x.Payload.Code })
                    .Subdivide(8, codeIdentifier)
                    .ApplyGraph<InterconnectionLogic>(false)
                    .GetSubGroups(x => x.Payload.Region)
                    //.Select(x => x.Subdivide(2, codeIdentifier))
                    ;


                //var splayers = layer1
                //    .Subdivide(4, codeIdentifier)
                //    .ApplyGraph<HighLevelConnectivity>(codeIdentifier, connector)
                //    .ForEach(x => new HexPayload(x.Payload) { Region = x.Payload.Code })
                //    .Subdivide(8, codeIdentifier)
                //    .ApplyGraph<TestBed>(codeIdentifier, connector, false)
                //    //.ApplyGraph<ApplyBounds>(interiorExterior, connector, true)
                //    .GetSubGroups(x => x.Payload.Region)
                //    .Select(x => x
                //        
                //        //.Subdivide(3, codeIdentifier)
                //        //.Subdivide(2, codeIdentifier)
                //        );

                var color = RNG.NextColor();

            var chunks = new TerrainChunkCollection(splayers, 64, 1);

            var terrain = TerrainBuilder.BuildTerrain(chunks);

            //var pts = chunks.GetPositions();
            //
            //for (int i = 0; i < pts.Length; i++)
            //{
            //    for (int u = 0; u < pts[i].Length; u++)
            //    {
            //        var pos = pts[i][u];
            //
            //        if (pos.y == 0)
            //            continue;
            //        var gob = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //        gob.transform.position = pos;
            //    }
            //}


                //foreach (var layer in splayers)
                //{
                //    var group = new HexGroupVisualiser(PreviewMesh);
                //
                //    var randomColor = RNG.SimilarColor(color);
                //    var randomColorDark = RNG.SimilarColor(randomColor);
                //
                //    //layer.ForEach(x => new HexPayload(x.Payload)
                //    //{
                //    //    //Color = x.Payload.ConnectionStatus == Connection.Present ? randomColor : x.Payload.ConnectionStatus == Connection.Critical ? randomColor : randomColorDark//RNG.NextColorBright()
                //    //    Color = x.Payload.ConnectionStatus == Connection.NotPresent ? new Color(x.Payload.Height * 0.1f, x.Payload.Height * 0.1f, x.Payload.Height * 0.1f) : new Color(x.Payload.Height * 0.3f, 0.6f, x.Payload.Height * 0.3f),
                //    //    Height = (x.Payload.ConnectionStatus == Connection.NotPresent ? x.Payload.Height+3 : 1f) + RandomXY.GetOffset(x.Index.Position3d.x,x.Index.Position3d.z).Distance
                //    //}); ;
                //
                //    layer.Bounds.DrawBounds(Color.white,100f);
                //
                //    //Debug.Log(layer.Bounds);
                //
                //    layer.ForEach(x => new HexPayload(x.Payload)
                //    {
                //        //Color = x.Payload.ConnectionStatus == Connection.Present ? randomColor : x.Payload.ConnectionStatus == Connection.Critical ? randomColor : randomColorDark//RNG.NextColorBright()
                //        Color = x.Payload.ConnectionStatus == Connection.Present ? randomColor : x.Payload.ConnectionStatus == Connection.Critical ? randomColor : randomColor,//RNG.NextColorBright()
                //        //Color = x.Payload.ConnectionStatus == Connection.Present ? Color.white : x.Payload.ConnectionStatus == Connection.Critical ? Color.white : Color.black,//RNG.NextColorBright()
                //        //Color = x.Payload.ConnectionStatus == Connection.NotPresent ? new Color(x.Payload.Height * 0.1f, x.Payload.Height * 0.1f, x.Payload.Height * 0.1f) : new Color(x.Payload.Height * 0.3f, 0.6f, x.Payload.Height * 0.3f),
                //        //Height = (x.Payload.Height + 1)*5
                //        Height = Mathf.RoundToInt((x.Payload.Height*4 + (x.Payload.EdgeDistance*1))*15 + (x.Payload.ConnectionStatus == Connection.NotPresent?(3+this.NoiseAtIndex(x) * 20) :0)) 
                //    });
                //
                //
                //    group.HexGroup = layer;
                //
                //    _gizmosHexGroups.Add(group);
                //}




                   //.Subdivide(2, codeIdentifier)
                   ////.Subdivide(4, codeIdentifier)
                   //.ForEach(x => new HexPayload(x.Payload)
                   //                 {
                   //    Color = x.Payload.ConnectionStatus == Connection.Present ? Color.white : x.Payload.ConnectionStatus == Connection.Critical ? Color.white : Color.black//RNG.NextColorBright()
                   //,
                   //                     Height = RNG.NextFloat(30)
                   //                 })

                ;

                //var anotherLayer = splayer.MassUpdateHexes(graphe.Finalise(standardRemapper))                    
                // .Subdivide(5, codeIdentifier)
                // //.Subdivide(3)
                //.ForEach(x => new HexPayload(x.Payload)
                //{
                //    Color = Color.white//Connection.Present ? Color.white : Color.black//RNG.NextColorBright()
                //,
                //    Height = RNG.NextFloat(30)
                //});
                //.Subdivide(2, codeIdentifier)
                //.ForEach(x => new HexPayload()
                //{
                //    Color = x.Payload.Color.grayscale + RNG.NextFloat(-0.0f, 0.0f) < 0.7 ? Color.black : Color.white
                //});
                //.Subdivide(3)//.Subdivide(3);
                //.ForEach(x => {
                //    var a = x.Payload;
                //    a.Color = RNG.NextColor();
                //    return a;
                //}).Subdivide(3).Subdivide(1);
                ;
                    //.ForEach((x, i) => new HexPayload() { Code = 1, Height = 0, Color = Color.white })
                    //.Subdivide();
                    ;

                //var mesh = splayer.ToMesh();
                //var (vertices, triangles) = splayer.ToNetwork(x => 0);
                //
                //var offset = 0.0f;
                //
                //vertices = vertices.Select(x => x + RNG.NextVector3(-offset, offset)+ Vector3.up).ToArray();
                //
                //offset = 0.1f;
                //
                //for (int t = 0; t < triangles.Length; t+=3)
                //{
                //    var localOffset = RNG.NextVector3(-offset, offset);
                //    var color = RNG.NextColorBright();
                //    Debug.DrawLine(vertices[triangles[t]]+ localOffset, vertices[triangles[t + 1]] + localOffset, color, 100f);
                //    Debug.DrawLine(vertices[triangles[t+1]] + localOffset, vertices[triangles[t + 2]] + localOffset, color, 100f);
                //    Debug.DrawLine(vertices[triangles[t+2]] + localOffset, vertices[triangles[t]] + localOffset, color, 100f);
                //
                //}
                
                
                
                //this.transform.GetComponent<MeshFilter>().sharedMesh = mesh;



                //needs refactoring
                //layerfruu.
                //layerfruu.ToGameObjects(Prefab);
                //layerfruu.ToGameObjectsBorder(BorderPrefab);

                //this.GetComponent<MeshFilter>().sharedMesh = layer2.ToMesh();
            

            return;

        }

        private void OnDrawGizmos()
        {
            //for (int i = 0; i < _gizmosHexGroups.Count; i++)
            //{
            //    _gizmosHexGroups[i].DrawGizmos();
            //}
            ////_gizmosHexGroup.DrawGizmos();
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
            if (Preview)
            {
                for (int i = 0; i < _gizmosHexGroups.Count; i++)
                {
                    _gizmosHexGroups[i].DrawMeshes();
                }
            }
            
            //this.transform.Rotate(Vector3.up, 5f * Time.deltaTime);
        }

        private float NoiseAtIndex(Hex hex)
        {
            var pos = hex.Index.Position2d;
            var perlin = Mathf.PerlinNoise(
                pos.x * 0.25324f,
                pos.y * 0.25324f
                );

            var multiplier = Mathf.InverseLerp(0, 6, hex.Payload.EdgeDistance);

            return perlin*multiplier;

        }
    }
    namespace Levels
    {




    }
}
