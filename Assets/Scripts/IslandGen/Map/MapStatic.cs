using UnityEngine;
using System.Collections.Generic;

public partial class Map
{
    //Contains constructiors, accessors, static functions and math functions

    public int SizeX
    { get; private set; }

    public int SizeY
    { get; private set; }

    float[,] _map;

    public float[,] FloatArray
    { get { return Clone(this).Normalise()._map; } }

    public Map(Map mapTemplate)
    {
        SizeX = mapTemplate.SizeX;
        SizeY = mapTemplate.SizeY;
        _map = new float[SizeX, SizeY];
    }

    public Map(int sizeX, int sizeY)
    {
        SizeX = sizeX;
        SizeY = sizeY;
        _map = new float[SizeX, SizeY];
    }

    public Map(Map mapTemplate, int defaultValue)
    {
        SizeX = mapTemplate.SizeX;
        SizeY = mapTemplate.SizeY;
        _map = new float[SizeX, SizeY];

        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                _map[x, y] = defaultValue;
            }
        }
    }

    public Map(int sizeX, int sizeY, float defaultValue)
    {
        SizeX = sizeX;
        SizeY = sizeY;
        _map = new float[SizeX, SizeY];

        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                _map[x, y] = defaultValue;
            }
        }
    }

    // Accessors

    public float this[int indexA, int indexB]
    {
        get { return _map[indexA, indexB]; }
        set { _map[indexA, indexB] = value; }
    }

    // Static Functions

    public static Map Clone(Map map)
    {
        return BlankMap(map).OverwriteMapWith(map);
    }

    public static List<Coord> GetCenters(List<List<Coord>> coords)
    {
        var returnCoords = new List<Coord>();

        for (int i = 0; i < coords.Count; i++)
        {
            var averageX = 0;
            var averageY = 0;

            for (int u = 0; u < coords[i].Count; u++)
            {
                averageX += coords[i][u].TileX;
                averageY += coords[i][u].TileY;
            }

            averageX /= coords[i].Count;
            averageY /= coords[i].Count;

            returnCoords.Add(new Coord(averageX, averageY));
        }

        return returnCoords;
    }

    public static Map BlankMap(Map template)
    {
        return new Map(template.SizeX, template.SizeY);
    }

    public static Map CreateHeightMap(Map[] heightData)
    {
        return Clone(heightData[0]).AddHeightmapLayers(heightData, 0);
    }

    public static Map BlankMap(int sizeX, int sizeY)
    {
        return new Map(sizeX, sizeY);
    }

    public static Map ApplyMask(Map mapA, Map mapB, Map mask)
    {
        return Clone(mapA).ApplyMask(mask, mapB);
    }

    public static bool MapsAreSameDimensions(Map mapA, Map mapB)
    {
        return mapA.SizeX == mapB.SizeX && mapA.SizeY == mapB.SizeY;

    }

    public static Map BooleanUnion(Map mapA, Map mapB)
    {
        //Need a check here to avoid failure

        if (!MapsAreSameDimensions(mapA, mapB))
        {
            Debug.Log("Maps are not the same size!");
            return null;
        }

        var outputMap = new Map(mapA);

        for (int x = 0; x < mapA.SizeX; x++)
        {
            for (int y = 0; y < mapA.SizeY; y++)
            {
                outputMap[x, y] = (mapA[x, y] == 0 | mapB[x, y] == 0) ? 0 : 1;
            }
        }
        return outputMap;
    }

    public static Map BooleanIntersection(Map mapA, Map mapB)
    {
        //Need a check here to avoid failure

        if (!MapsAreSameDimensions(mapA, mapB))
        {
            Debug.Log("Maps are not the same size!");
            return null;
        }

        var outputMap = new Map(mapA);

        for (int x = 0; x < mapA.SizeX; x++)
        {
            for (int y = 0; y < mapA.SizeY; y++)
            {
                outputMap[x, y] = (mapA[x, y] == 0 && mapB[x, y] == 0) ? 0 : 1;
            }
        }
        return outputMap;
    }

    public static Map BooleanDifference(Map mapA, Map mapB)
    {
        //Need a check here to avoid failure

        if (!MapsAreSameDimensions(mapA, mapB))
        {
            Debug.Log("Maps are not the same size!");
            return null;
        }

        var outputMap = new Map(mapA);

        for (int x = 0; x < mapA.SizeX; x++)
        {
            for (int y = 0; y < mapA.SizeY; y++)
            {
                outputMap[x, y] = (mapA[x, y] == 0) ? 1 : mapB[x, y];
            }
        }
        return outputMap;
    }

    public Map BooleanMapFromThreshold(float threshold)
    {
        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                _map[x, y] = _map[x, y] <= threshold ? 0 : 1;
            }
        }

        return this;
    }

    public static Map GetInvertedMap(Map map)
    {
        return Clone(map).Invert();
    }

    public static Map HigherResult(Map mapA, Map mapB)
    {
        var map = Clone(mapA);

        for (int x = 0; x < mapA.SizeX; x++)
        {
            for (int y = 0; y < mapA.SizeY; y++)
            {
                map[x, y] = mapA[x, y] > mapB[x, y] ? mapA[x, y] : mapB[x, y];
            }
        }

        return map;
    }

    // Math Functions

    public static Map operator +(Map a, Map b)
    {
        var outputMap = new Map(a);

        for (int x = 0; x < a.SizeX; x++)
        {
            for (int y = 0; y < a.SizeY; y++)
            {
                outputMap[x, y] = a[x, y] + b[x, y];
            }
        }

        return outputMap;
    }

    public static Map operator -(Map a, Map b)
    {
        var outputMap = new Map(a);

        for (int x = 0; x < a.SizeX; x++)
        {
            for (int y = 0; y < a.SizeY; y++)
            {
                outputMap[x, y] = a[x, y] - b[x, y];
            }
        }

        return outputMap;
    }

    public static Map operator *(Map a, Map b)
    {
        var outputMap = new Map(a);

        for (int x = 0; x < a.SizeX; x++)
        {
            for (int y = 0; y < a.SizeY; y++)
            {
                outputMap[x, y] = a[x, y] * b[x, y];
            }
        }

        return outputMap;
    }

    public static Map operator /(Map a, Map b)
    {
        var outputMap = new Map(a);

        for (int x = 0; x < a.SizeX; x++)
        {
            for (int y = 0; y < a.SizeY; y++)
            {
                outputMap[x, y] = a[x, y] / b[x, y];
            }
        }

        return outputMap;
    }
}
    