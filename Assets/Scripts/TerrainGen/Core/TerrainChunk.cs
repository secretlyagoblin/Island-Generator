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
    public BoundsInt ScaledBounds { get { return new BoundsInt(Bounds.min.x * Multiplier, Bounds.min.y * Multiplier, 0, Bounds.size.x * Multiplier, Bounds.size.y * Multiplier, 0); } }

    public StampData[,] Map;

    private List<HexGroup> _groups;

    public int Multiplier { get; private set; }



    public TerrainChunk(BoundsInt bounds, List<HexGroup> groups, int multiplier)
    {
        Bounds = bounds;
        Multiplier = multiplier;
        Map = new StampData[(bounds.size.x* Multiplier) +1, (bounds.size.y * Multiplier) +1];
        _groups = groups;

    }

    public bool ApplyPixels()
    {
        var pixels = AssociatePixels();

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
                Map[pt.x - (Bounds.min.x*Multiplier), pt.y - (Bounds.min.y*Multiplier)] = new StampData() { Height = p.Center.Payload.Height + (p.Center.Payload.EdgeDistance*0.2f) };
            }            
        }

        return true;
    }

    public Dictionary<HexIndex,List<Vector2Int>> AssociatePixels()
    {
        var map = new Dictionary<HexIndex, List<Vector2Int>>();

        var inverseSize = 1f / Multiplier;

        for (int x = 0; x < Map.GetLength(0); x++)
        {
            for (int y = 0; y < Map.GetLength(1); y++)
            {
                var pixel = new Vector2Int(Bounds.min.x*Multiplier + (x), Bounds.min.y*Multiplier + (y));
                var hex = HexIndex.PixelToHex(pixel, inverseSize);

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

    public Vector3[] To1DArray()
    {
        // Step 1: get total size of 2D array, and allocate 1D array.
        int size = Map.Length;
        Vector3[] result = new Vector3[size];

        // Step 2: copy 2D array elements into a 1D array.
        int write = 0;
        for (int i = 0; i <= Map.GetUpperBound(0); i++)
        {
            for (int z = 0; z <= Map.GetUpperBound(1); z++)
            {
                result[write++] =  new Vector3(i+(this.Bounds.min.x*Multiplier), Map[i, z].Height*5,z + (this.Bounds.min.y*Multiplier));
            }
        }
        // Step 3: return the new array.
        return result;
    }
}
