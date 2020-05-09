using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WanderingRoad.Procgen.RecursiveHex;

public struct StampData
{
    public float Height;
}

public class TerrainChunk
{
    public BoundsInt Bounds;

    public StampData[,] Map;

    private List<HexGroup> _groups;



    public TerrainChunk(BoundsInt bounds, List<HexGroup> groups)
    {
        Bounds = bounds;
        Map = new StampData[bounds.size.x+1, bounds.size.y+1];

    }

    public bool ApplyPixels()
    {
        var pixels = AssociatePixels(1);

        var bloops = _groups.ConvertAll(x => x.GetNeighbourhoodDictionary());

        foreach (var item in pixels)
        {
            var g = bloops.FirstOrDefault(x => x.ContainsKey(item.Key.Index3d));
            if(g == null)
            {
                continue;
            }
            var p = g[item.Key.Index3d];

            foreach (var pt in item.Value)
            {
                Map[pt.x - Bounds.min.x, pt.y - Bounds.min.y] = new StampData() { Height = p.Center.Payload.Height };
            }            
        }

        return true;
    }

    public Dictionary<HexIndex,List<Vector2Int>> AssociatePixels(float size)
    {
        var map = new Dictionary<HexIndex, List<Vector2Int>>();

        for (int x = 0; x < Map.GetLength(0); x++)
        {
            for (int y = 0; y < Map.GetLength(1); y++)
            {
                var pixel = new Vector2Int(Bounds.min.x + x, Bounds.min.y + y);
                var hex = HexIndex.PixelToHex(pixel,size);

                if (map.ContainsKey(hex))
                {
                    map[hex].Add(pixel);
                }
                else
                {
                    map.Add(hex, new List<Vector2Int>() { pixel });
                }
            }
        }

        return map;
    }
}
