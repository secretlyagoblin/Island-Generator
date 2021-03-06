﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WanderingRoad.Random;
using WanderingRoad.Procgen.RecursiveHex;
using System.Data;

public struct StampData
{
    public float Height;
}

[Serializable]
public class TerrainChunk
{
    public RectInt Bounds;

    public RectInt ScaledBounds { get { return new RectInt(Bounds.min.x * Multiplier, Bounds.min.y * Multiplier, Bounds.size.x * Multiplier, Bounds.size.y * Multiplier); } }

    public StampData[,] Map;

    //private List<HexGroup> _groups;

    public int Multiplier { get; private set; }

    internal static TerrainChunk FromJson(float maxValue, float minValue, int multiplier, RectInt bounds, StampData[,] stampData)
    {
        var chunk = new TerrainChunk
        {
            Bounds = bounds,
            Multiplier = multiplier,
            Map = stampData,
            MinValue = minValue,
            MaxValue = maxValue
        };

        return chunk;
    }

    public float MinValue = float.MaxValue;
    public float MaxValue = float.MinValue;

    public TerrainChunk(RectInt bounds, List<HexGroup> groups, int multiplier, Func<float, float, HexPayload, float> func)
    {
        Bounds = bounds;
        Multiplier = multiplier;

        Map = new StampData[(bounds.size.x* Multiplier) +1, (bounds.size.y * Multiplier) +1];

        ApplyPixels(groups, multiplier, bounds, func);
    }

    private TerrainChunk()
    {
    }

    //public static TerrainChunk FromJson()
    //{
    //
    //}

    private bool ApplyPixels(List<HexGroup> groups, int multiplier, RectInt bounds, Func<float, float, HexPayload, float> func)
    {
        var hexToPixel = AssociatePixels(multiplier,bounds);
        var hexes = new Dictionary<Vector3Int, Neighbourhood>();

        groups.ForEach(x => x.GetNeighbourhoods(false).ToList().ForEach(y => hexes.Add(y.Center.Index.Index3d,y)));

        var inverseMultiplier = 1f / multiplier;

        foreach (var pixelList in hexToPixel)
        {
            if (hexes.TryGetValue(pixelList.Key.Index3d, out var p))
            {

                foreach (var pt in pixelList.Value)
                {
                    var payload = p.HexPayloadAtPosition((Vector2)pt * inverseMultiplier);

                    var val = func(pt.x, pt.y, payload);//((payload.Height * 1) + ((Mathf.Max(payload.EdgeDistance - 0.5f, 0)) * 0.5f)) * 6;// + RNG.NextFloat(-0.1f, 0.1f);
                    if (val > MaxValue) MaxValue = val;
                    if (val < MinValue) MinValue = val;
                    Map[pt.x - (Bounds.min.x * Multiplier), pt.y - (Bounds.min.y * Multiplier)] = new StampData() { Height = val };

                }
            }
            else
            {
                continue;
            }
        }

        return true;
    }

    private static Dictionary<HexIndex,List<Vector2Int>> AssociatePixels(int multiplier, RectInt bounds)
    {
        var map = new Dictionary<HexIndex, List<Vector2Int>>();

        var inverseSize = 1f / multiplier;

        for (int x = 0; x < bounds.size.x*multiplier+1; x++)
        {
            for (int y = 0; y < bounds.size.y * multiplier + 1; y++)
            {
                var pixel = new Vector2Int(bounds.min.x* multiplier + (x), bounds.min.y*multiplier + (y));
                var position = new Vector2(
                    pixel.x * inverseSize,
                    pixel.y * inverseSize);

                var hex = HexIndex.HexIndexFromPosition(position);

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

    public float[,] GetResizedHeightmap(int newSize, float minValue, float maxValue)
    {
        var map = new float[newSize, newSize]; ;

        for (int x = 0; x < newSize; x++)
        {
            for (int y = 0; y < newSize; y++)
            {
                var normalisedX = Mathf.InverseLerp(0, newSize - 1, x);
                var normalisedY = Mathf.InverseLerp(0, newSize - 1, y);

                var num = BilinearSampleFromNormalisedVector2(new Vector2(normalisedX, normalisedY));

                map[y, x] = Mathf.InverseLerp(minValue, maxValue, num);
            }
        }
        return map;
    }

    //public float[,] GetHeightmap()
    //{
    //    var floats = new float[this.Map.GetLength(0), this.Map.GetLength(1)];
    //
    //    for (int x = 0; x < this.Map.GetLength(0); x++)
    //    {
    //        for (int y = 0; y < this.Map.GetLength(1); y++)
    //        {
    //            floats[x, y] = Mathf.InverseLerp(_minValue,_maxValue,this.Map[x, y].Height);
    //        }
    //    }
    //
    //    return floats;
    //}


    public Vector3[] To1dPositionArray()
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

    public StampData[] To1dDataArray()
    {
        // Step 1: get total size of 2D array, and allocate 1D array.
        int size = Map.Length;
        StampData[] result = new StampData[size];

        // Step 2: copy 2D array elements into a 1D array.
        int write = 0;
        for (int i = 0; i <= Map.GetUpperBound(0); i++)
        {
            for (int z = 0; z <= Map.GetUpperBound(1); z++)
            {
                result[write++] = Map[i,z];
            }
        }
        // Step 3: return the new array.
        return result;
    }

    private float BilinearSampleFromNormalisedVector2(Vector2 normalisedVector)
    {

        if (normalisedVector.x > 1 | normalisedVector.y > 1 | normalisedVector.x < 0 | normalisedVector.y < 0)
        {
            Debug.Log("You aren't normal and as such are not welcome here, in this debug log");

            Debug.Log(normalisedVector);
        }

        float u = normalisedVector.x * (Map.GetLength(0) - 1);
        float v = normalisedVector.y * (Map.GetLength(1) - 1);
        int x = (int)Mathf.Floor(u);
        int y = (int)Mathf.Floor(v);
        if (u == x && u != 0)
            x--;
        if (v == y && v != 0)
            y--;
        float u_ratio = u - x;
        float v_ratio = v - y;
        float u_opposite = 1f - u_ratio;
        float v_opposite = 1f - v_ratio;


        float result = (this.Map[x, y].Height * u_opposite + this.Map[x + 1, y].Height * u_ratio) * v_opposite +
                   (this.Map[x, y + 1].Height * u_opposite + this.Map[x + 1, y + 1].Height * u_ratio) * v_ratio;
        return result;


    }
}
