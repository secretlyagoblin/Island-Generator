using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
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

        hexManifest.Select(x => x.Key).ToList().ForEach(x => x.DrawRect(Color.red, 100f));

        var hexes = hexManifest.ToDictionary(x => x.Value, x =>Util.DeserialiseFile<HexGroup>(Paths.GetHexGroupPath(info.World, x.Value.ToString()), new HexGroupConverter()));

        var total = hexManifest.First().Key;

        foreach (var bounds in hexManifest)
        {
            total = total.Encapsulate(bounds.Key);
        }

        total.DrawRect(Color.white, 100f);

        var size = 100;
        var oneOverSize = 1f / size;

        var minX = Mathf.FloorToInt(total.xMin * oneOverSize);
        var maxX = Mathf.CeilToInt(total.xMax * oneOverSize);
        var minY = Mathf.FloorToInt(total.yMin * oneOverSize);
        var maxY = Mathf.CeilToInt(total.yMax * oneOverSize);

        var rects = new List<Rect>();

        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                rects.Add( new Rect(x * size, y * size, size, size));
            }
        }

        rects.ForEach(x => x.DrawRect(Color.blue, 100f));

        

        var terrains = rects.Select(x =>
            new TerrainChunk(x.ToBoundsInt(),
            hexManifest.Where(y => y.Key.Overlaps(x)).Select(y => hexes[y.Value]).ToList(),
            8,
            SamplerComplex)).ToList();

        var chunkManifest = new TerrainManifest();

        for (int i = 0; i < rects.Count; i++)
        {
            var rect = rects[i];
            var terrain = terrains[i];
            var guid = Guid.NewGuid();

            terrain.SerialiseFile(Paths.GetTerrainChunkPath(info.World, guid.ToString()), new TerrainChunkConverter());
            chunkManifest.MaxHeight = chunkManifest.MaxHeight < terrain.MaxValue ? terrain.MaxValue : chunkManifest.MaxHeight;
            chunkManifest.MinHeight = chunkManifest.MinHeight > terrain.MinValue ? terrain.MinValue : chunkManifest.MinHeight;
            chunkManifest.Terrains.Add(rect, guid);
        }

        Util.SerialiseFile(chunkManifest, Paths.GetTerrainChunkPathManifestPath(info.World), new TerrainManifestSerialiser());     

        

    }

    private static float Sampler(float x, float y, HexPayload payload)
    {
        return payload.Height;
    }

    private static float SamplerComplex(float x, float y, HexPayload payload)
    {
        return ((payload.Height * 1) + ((Mathf.Max(payload.EdgeDistance - 0.5f, 0)) * 0.5f)) * 14;// + RNG.NextFloat(-0.1f, 0.1f)
    }    
}

public class TerrainManifest
{
    public Dictionary<Rect, Guid> Terrains = new Dictionary<Rect, Guid>();
    public float MaxHeight = float.MinValue;
    public float MinHeight = float.MaxValue;
}
