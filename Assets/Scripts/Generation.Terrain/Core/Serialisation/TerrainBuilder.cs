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

        var hexManifest = Util.DeserialiseFile<Dictionary<Rect, Guid>>(Paths.GetHexGroupManifestPath(info.World), new ManifestSerialiser());

        var singleChunk = new Rect(-50, -50, 100, 100);

        //var formatter = new BinaryFormatter();

        var groups = hexManifest
            .Where(x => x.Key.Overlaps(singleChunk))
            .Select(x =>
                Util.DeserialiseFile<HexGroup>(Paths.GetHexGroupPath(info.World,x.Value.ToString()),new HexGroupConverter())
            ).ToList();

        var chunk = new TerrainChunk(singleChunk.ToBoundsInt(), groups, 4, SamplerComplex);
        var guid = Guid.NewGuid();

        //throw new NotImplementedException();

        var chunkManifest = new Dictionary<Rect, Guid>();
        chunkManifest.Add(singleChunk, guid);

        Util.SerialiseFile(chunkManifest, Paths.GetTerrainChunkPathManifestPath(info.World), new ManifestSerialiser());

        chunk.SerialiseFile(Paths.GetTerrainChunkPath(info.World, guid.ToString()), new TerrainChunkConverter());

    }

    private static float Sampler(float x, float y, HexPayload payload)
    {
        return payload.Height;
    }

    private static float SamplerComplex(float x, float y, HexPayload payload)
    {
        return ((payload.Height * 1) + ((Mathf.Max(payload.EdgeDistance - 0.5f, 0)) * 0.5f)) * 6;// + RNG.NextFloat(-0.1f, 0.1f)
    }

    
}
