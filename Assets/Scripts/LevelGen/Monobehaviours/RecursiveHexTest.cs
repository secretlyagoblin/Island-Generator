using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using WanderingRoad.Core.Random;
using WanderingRoad.Procgen.RecursiveHex;
using WanderingRoad.Procgen.Topology;

namespace WanderingRoad.Procgen.Levelgen
{
    public class RecursiveHexTest : MonoBehaviour
    {
        public GameObject Prefab;
        public GameObject BorderPrefab;
        public Mesh PreviewMesh;

        public bool Preview = true;

        private HexGroupVisualiser _gizmosHexGroup;// = new HexGroupVisualiser(PreviewMesh,)

        // Start is called before the first frame update
        void Start()
        {
            _gizmosHexGroup = new HexGroupVisualiser(PreviewMesh);

            //RNG.Init("I'd kill fill zill");
            RNG.Init("3/15/2020 5:58:48 PM");
            //RNG.DateTimeInit();
            //RecursiveHex.RandomSeedProperties.Disable();

            var regionIdentifier = new Func<HexPayload, int>(x => x.Region);
            var codeIdentifier = new Func<HexPayload, int>(x => x.Code);
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

            for (int i = 0; i < 1; i++)
            {

                RandomXY.SetRandomSeed(RNG.NextFloat(-1000, 1000), RNG.NextFloat(-1000, 1000));

                var splayer = layer1
                    .Subdivide(4, codeIdentifier)
                    .ApplyGraph<HighLevelConnectivity>(codeIdentifier,connector)
                    .Subdivide(8, codeIdentifier)
                    //.
                    //.Subdivide(4, codeIdentifier)
                    //.ForEach(x => new HexPayload(x.Payload)
                    // {
                    //     Color = x.Payload.ConnectionStatus == Connection.Present ? Color.white : x.Payload.ConnectionStatus == Connection.Critical ? Color.green:Color.black//RNG.NextColorBright()
                    //,
                    //     Height = RNG.NextFloat(30)
                    // })
                    .ApplyGraph<TestBed>(codeIdentifier, connector,true)
                    .Subdivide(2, codeIdentifier)
                    //.Subdivide(4, codeIdentifier)
                    .ForEach(x => new HexPayload(x.Payload)
                                     {
                        Color = x.Payload.ConnectionStatus == Connection.Present ? Color.white : x.Payload.ConnectionStatus == Connection.Critical ? Color.white : Color.black//RNG.NextColorBright()
                    ,
                                         Height = RNG.NextFloat(30)
                                     })

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
                _gizmosHexGroup.HexGroup = splayer;
                //layerfruu.ToGameObjectsBorder(BorderPrefab);

                //this.GetComponent<MeshFilter>().sharedMesh = layer2.ToMesh();
            }

            return;

        }

        private void OnDrawGizmos()
        {
            if (_gizmosHexGroup == null)
                return;

            //_gizmosHexGroup.DrawGizmos();
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
                this._gizmosHexGroup.DrawMeshes();
            }
            
            //this.transform.Rotate(Vector3.up, 5f * Time.deltaTime);
        }
    }
    namespace Levels
    {




    }
}
