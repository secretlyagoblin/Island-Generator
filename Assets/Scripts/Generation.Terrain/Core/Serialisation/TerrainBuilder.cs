using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using WanderingRoad;
using WanderingRoad.Procgen.RecursiveHex;
using WanderingRoad.Procgen.RecursiveHex.Json;

public static class TerrainGenerator
{
    // Start is called before the first frame update
    public static void BuildTerrain(LevelInfo info)
    {

        var manifest = Util.DeserialiseFile<Dictionary<Rect, Guid>>(Paths.GetHexGroupManifestPath(info.World), new ManifestSerialiser());

        var singleChunk = new Rect(-50, -50, 100, 100);

        //var formatter = new BinaryFormatter();

        var groups = manifest
            .Where(x => x.Key.Overlaps(singleChunk))
            .Select(x =>
                Util.DeserialiseFile<HexGroup>(Paths.GetHexGroupPath(info.World,x.Value.ToString()),new HexGroupConverter())
            ).ToList();

        var chunk = new TerrainChunk(singleChunk.ToBoundsInt(), groups, 4, null);
        var guid = new Guid();

        throw new NotImplementedException();

        //chunk.SerialiseFile(formatter, info.Path, "terrainChunks", guid.ToString(), "chunk");

    }
}
