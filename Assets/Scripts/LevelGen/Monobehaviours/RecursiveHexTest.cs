using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using WanderingRoad.Core.Random;
using WanderingRoad.Procgen.RecursiveHex;
using WanderingRoad.Procgen.Topology;
using WanderingRoad.Core;
using System.IO;

namespace WanderingRoad.Procgen.Levelgen
{
    public class RecursiveHexTest : MonoBehaviour
    {
        public GameObject Prefab;
        public GameObject BorderPrefab;
        public Mesh PreviewMesh;

        public bool Preview = true;

        private List<HexGroupVisualiser> _gizmosHexGroups;// = new HexGroupVisualiser(PreviewMesh,)

        private string _savePath = "";

        // Start is called before the first frame update
        void Start()
        {

            
            _gizmosHexGroups = new List<HexGroupVisualiser>() { };//HexGroupVisualiser(PreviewMesh);

            RNG.Init("I'd kill fill zill");
            //cool seed "4/27/2020 7:45:28 PM"
            // orphan node to test.. "4/27/2020 5:56:43 PM"
            //RNG.Init("3/15/2020 5:58:48 PM");
            // dead end to test... RNG.Init("5/2/2020 5:04:24 PM");
            //RNG.DateTimeInit();
            //RecursiveHex.RandomSeedProperties.Disable();

            _savePath = $"{Application.persistentDataPath}/{RNG.CurrentSeed()}";

            var layers = Directory.Exists(_savePath) ? GroupsFromFile() : GenerateGroups();

            var color = RNG.NextColor();

            var chunks = new TerrainChunkCollection(layers, 64, 4, CalculateNoise);

            var terrain = TerrainBuilder.BuildTerrain(chunks);

            //var propsets = getLayers.ConvertAll(x => new RegionProps(x, new PropGen()));
            //
            //var allProps = new List<PropData>();

            return;

        }

        private List<HexGroup> GenerateGroups()
        {
            Debug.Log("Creating files!");


            var regionIdentifier = new Func<HexPayload, int>(x => x.Region);
            var codeIdentifier = new Func<HexPayload, int>(x => x.Code);
            var interiorExterior = new Func<HexPayload, int>(x => x.ConnectionStatus == Connection.NotPresent ? 1 : 2);
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

            var chunkPath = $"{_savePath}/Chunks";



            foreach (var item in splayers)
            {
                var path = $"{chunkPath}/{Guid.NewGuid()}.json";
                var info = new System.IO.FileInfo(path);

                if (!info.Exists)
                    Directory.CreateDirectory(info.Directory.FullName);

                System.IO.File.WriteAllText(path, JsonUtility.ToJson(item.ToSerialisable()));
                Debug.Log($"Json written to {path}");
            }

            return splayers;
        }

        private List<HexGroup> GroupsFromFile()
        {
            Debug.Log("Known level seed! Skipping file creation and loading from disk!");

            return Directory.GetFiles($"{_savePath}/Chunks").Select(x => new HexGroup(JsonUtility.FromJson<SerialisableHexGroup>(File.ReadAllText(x)))).ToList();
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

        private float CalculateNoise(float x, float y, HexPayload payload)
        {
            var offset = 0.01332f;
            var scale = 0.0745f;
            x += offset;
            y += offset;

            var noise = Mathf.PerlinNoise(x * scale, y * scale);

            offset = 0.03332f;
            scale = 0.1545f;
            x += offset;
            y += offset;

            var noise2 = Mathf.PerlinNoise(x * scale, y * scale);

            var edgeDistance = Mathf.Max(payload.EdgeDistance + (RNG.CoinToss()?RNG.NextFloat(-0.1f, 0.1f):0) - 0.5f, 0f);
            var distance = Mathf.InverseLerp(0, 2, edgeDistance);
            noise *= distance;
            var distance2 = Mathf.InverseLerp(0, 5, edgeDistance);
            noise += (distance2*noise2*0.3f);

            var height = (payload.Height * 4) + (edgeDistance * 2f) + (noise * 6) ;



            return height;
        }
    }

    public class PropGen : IDeterminePropRelationships
    {
        public bool GetFarProps(Dictionary<Vector3Int, Neighbourhood> hoods, List<PropData> propDataToWrite)
        {
            propDataToWrite.Clear();

            foreach (var item in hoods)
            {
                var payload = item.Value.Center.Payload;
                if (payload.EdgeDistance < 0.75f || payload.EdgeDistance >2)
                    continue;

                var data = new PropData()
                {
                    Position = item.Value.Center.Index.Position2d,
                    HeightGuide = 2,
                    Rotation = RNG.NextFloat(360),
                    Yaw = 0,
                    PropType = PropType.Backdrop
                };

                propDataToWrite.Add(data);
            }

            return true;

        }

        public bool GetCloseProps(Dictionary<Vector3Int, Neighbourhood> hoods, List<PropData> propDataToWrite)
        {
            throw new NotImplementedException();
        }
    }
}
