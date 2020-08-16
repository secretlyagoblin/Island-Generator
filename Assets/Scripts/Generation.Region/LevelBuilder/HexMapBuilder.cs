using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using WanderingRoad.Random;
using WanderingRoad.Procgen.RecursiveHex;
using WanderingRoad.Procgen.Topology;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WanderingRoad.Procgen.Levelgen
{
    public class HexMapBuilder
    {
        public static LevelInfo BuildHexMap(string seed)
        {
            RNG.ForceInit(seed);

            //var savePath = $"{Application.persistentDataPath}/{RNG.CurrentSeed()}";

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

            var manifest = new Dictionary<Rect, Guid>();
            var formatter = new BinaryFormatter();

            var world = RNG.CurrentSeed();

            foreach (var item in splayers)
            {
                var guid = Guid.NewGuid();
                manifest.Add(item.Bounds, guid);

                item.Bounds.DrawRect(Color.red, 100f);

                item.SerialiseFile(Paths.GetHexGroupPath(world, guid.ToString()), new RecursiveHex.Json.HexGroupConverter());
            }

            //throw new NotImplementedException();

            manifest.SerialiseFile(Paths.GetHexGroupManifestPath(world), new ManifestSerialiser());

            return new LevelInfo() { World = world };
        }



    }
}
