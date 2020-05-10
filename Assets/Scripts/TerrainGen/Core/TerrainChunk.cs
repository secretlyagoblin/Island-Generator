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

    internal float _minValue = -10;
    internal float _maxValue = 0f;



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
                var val = (p.Center.Payload.Height*0) + (p.Center.Payload.EdgeDistance * 0.05f);
                if (val > _maxValue) _maxValue = val;
                Map[pt.x - (Bounds.min.x*Multiplier), pt.y - (Bounds.min.y*Multiplier)] = new StampData() { Height = val };

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

    public float[,] GetResizedHeightmap(int newSize)
    {
        var map = new float[newSize, newSize]; ;

        for (int x = 0; x < newSize; x++)
        {
            for (int y = 0; y < newSize; y++)
            {
                var normalisedX = Mathf.InverseLerp(0, newSize - 1, x);
                var normalisedY = Mathf.InverseLerp(0, newSize - 1, y);

                var num = BilinearSampleFromNormalisedVector2(new Vector2(normalisedX, normalisedY));

                map[x, y] = Mathf.InverseLerp(_minValue, _maxValue, num);
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
