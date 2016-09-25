using UnityEngine;
using System.Collections;

public class Map {

    public int SizeX
    { get; private set; }

    public int SizeY
    { get; private set; }

    int[,] _map;

    public Map(Map mapTemplate)
    {
        SizeX = mapTemplate.SizeX;
        SizeY = mapTemplate.SizeY;
        _map = new int[SizeX, SizeY];
    }

    public Map(int sizeX, int sizeY)
    {
        SizeX = sizeX;
        SizeY = sizeY;
        _map = new int[SizeX, SizeY];
    }

    public Map(Map mapTemplate, int defaultValue)
    {
        SizeX = mapTemplate.SizeX;
        SizeY = mapTemplate.SizeY;
        _map = new int[SizeX, SizeY];
    }

    public Map(int sizeX, int sizeY, int defaultValue)
    {
        SizeX = sizeX;
        SizeY = sizeY;
        _map = new int[SizeX, SizeY];
    }

    public static Map CloneMap(Map map)
    {
        return BlankMap(map).OverwriteMapWith(map);
    }

    public Map OverwriteMapWith(Map map)
    {
        var newMap = new Map(map);
        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                _map[x, y] = map[x, y];
            }
        }
        return this;
    }

    public static Map BlankMap(Map template)
    {
        return new Map(template.SizeX, template.SizeY);
    }

    public static Map BlankMap(int sizeX, int sizeY)
    {
        return new Map(sizeX, sizeY);
    }

    public int this[int indexA, int indexB]
    {
        get { return _map[indexA, indexB]; }
    }

    public Map RandomFillMap()
    {
        return RandomFillMap(0.5f);
    }

    public Map RandomFillMap(float randomFillPercent)
    {
        return RandomFillMap(randomFillPercent, 0, 0, 1);
    }

    public Map RandomFillMap(float randomFillPercent, float perlinNoiseIntensity, float perlinScale)
    {
        var perlinSeed = RNG.NextFloat(0, 10000f);


        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                if (x == 0 || x == SizeX - 1 || y == 0 || y == SizeY - 1)
                {
                    _map[x, y] = 1;
                }

                float perlinX = perlinSeed + ((x / (float)SizeX) * perlinScale);
                float perlinY = perlinSeed + ((y / (float)SizeY) * perlinScale);

                var perlin = Mathf.PerlinNoise(perlinX, perlinY);
                //Debug.Log("Perlin Per Square: " + perlin);
                var randomValue = RNG.NextFloat(0f, 1f - perlinNoiseIntensity);
                perlin = (perlin * perlinNoiseIntensity);

                randomValue += perlin;

                _map[x, y] = (randomValue < randomFillPercent) ? 1 : 0;
            }
        }

        return this;
    }

    public Map CreateCircularFalloff()
    {
        var centreX = (int)(SizeX * 0.5f);
        var centreY = (int)(SizeY * 0.5f);
        var radius = Mathf.Pow(SizeY * 0.5f, 2f);

        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                if (Mathf.Pow(x - centreX, 2) + Mathf.Pow(y - centreY, 2) < radius)
                {
                    _map[x, y] = 0;
                }
                else
                {
                    _map[x, y] = 1;
                }
            }
        }

        return this;
    }

    public Map SmoothMap()
    {
        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(_map, x, y);
                if (neighbourWallTiles > 4)
                {
                    _map[x, y] = 1;
                }
                else if (neighbourWallTiles < 4)
                {
                    _map[x, y] = 0;
                }
            }
        }
        return this;
    }

    public Map SmoothMap(int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            SmoothMap();
        }
        return this;
    }

    int GetSurroundingWallCount(int[,] map, int gridX, int gridY)
    {
        int wallCount = 0;
        for (int x = gridX - 1; x <= gridX + 1; x++)
        {
            for (int y = gridY - 1; y <= gridY + 1; y++)
            {
                if (IsInMapRange(map, x, y))
                {
                    if (x != gridX || y != gridY)
                    {
                        wallCount += map[x, y];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }
        return wallCount;
    }

    bool IsInMapRange(int[,] map, int x, int y)
    {
        return x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1);
    }

    public Map ApplyMask(Map maskToApply, int maskValue)
    {
        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                if (maskToApply[x, y] == maskValue)
                    _map[x, y] = maskValue;
            }
        }

        return this;
    }


}
