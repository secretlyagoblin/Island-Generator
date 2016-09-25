using UnityEngine;
using System.Collections.Generic;

public class Map {

    public Coord

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

    int[,] AddRoomLogic(int[,] map)
    {
        var wallRegions = GetRegions(map, 1);
        var wallThresholdSize = RegionSizeCutoff;

        for (int i = 0; i < wallRegions.Count; i++)
        {
            if (wallRegions[i].Count < wallThresholdSize)
            {
                for (int r = 0; r < wallRegions[i].Count; r++)
                {
                    map[wallRegions[i][r].TileX, wallRegions[i][r].TileY] = 0;
                }
            }
        }

        var roomRegions = GetRegions(map, 0);
        var roomThresholdSize = RegionSizeCutoff;

        var survivingRooms = new List<Room>();

        for (int i = 0; i < roomRegions.Count; i++)
        {
            if (roomRegions[i].Count < roomThresholdSize)
            {
                for (int r = 0; r < roomRegions[i].Count; r++)
                {
                    map[roomRegions[i][r].TileX, roomRegions[i][r].TileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegions[i], map));
            }
        }

        survivingRooms.Sort();
        survivingRooms[0].IsMainRoom = true;
        survivingRooms[0].IsAccessibleFromMainRoom = true;
        ConnectClosestRooms(survivingRooms, false, map);

        return map;
    }

    List<List<Coord>> GetRegions(int[,] map, int tileType)
    {
        var width = map.GetLength(0);
        var length = map.GetLength(1);

        var regions = new List<List<Coord>>();
        var mapFlags = new int[width, length];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    var newRegion = GetRegionTiles(map, x, y);
                    regions.Add(newRegion);

                    for (int i = 0; i < newRegion.Count; i++)
                    {
                        var tile = newRegion[i];
                        mapFlags[tile.TileX, tile.TileY] = 1;
                    }
                }
            }
        }

        //Debug.Log("There are " + regions.Count + " distinct regions.");

        return regions;
    }

    List<Coord> GetRegionTiles(int[,] map, int startX, int startY)
    {
        var width = map.GetLength(0);
        var length = map.GetLength(1);

        var tiles = new List<Coord>();
        var mapFlags = new int[width, length];
        int tileType = map[startX, startY];

        var queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            var tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.TileX - 1; x <= tile.TileX + 1; x++)
            {
                for (int y = tile.TileY - 1; y <= tile.TileY + 1; y++)
                {
                    if (IsInMapRange(map, x, y) && (y == tile.TileY || x == tile.TileX))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }

    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom, int[,] map)
    {
        var nonConnectedRooms = new List<Room>();
        var connectedRooms = new List<Room>();

        if (forceAccessibilityFromMainRoom)
        {
            for (int i = 0; i < allRooms.Count; i++)
            {
                var room = allRooms[i];
                if (room.IsAccessibleFromMainRoom)
                {
                    connectedRooms.Add(room);
                }
                else
                {
                    nonConnectedRooms.Add(room);
                }
            }
        }
        else
        {
            nonConnectedRooms = allRooms;
            connectedRooms = allRooms;
        }

        var bestDistance = 0;
        var bestTileA = new Coord();
        var bestTileB = new Coord();
        var bestRoomA = new Room();
        var bestRoomB = new Room();
        var possibleConnectionFound = false;

        for (int a = 0; a < nonConnectedRooms.Count; a++)
        {
            var roomA = nonConnectedRooms[a];
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (roomA.ConnectedRooms.Count > 0)
                {
                    continue;
                }
            }

            for (int b = 0; b < connectedRooms.Count; b++)
            {
                var roomB = connectedRooms[b];
                if (roomA == roomB || roomA.IsConnected(roomB))
                    continue;

                for (int tileIndexA = 0; tileIndexA < roomA.EdgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.EdgeTiles.Count; tileIndexB++)
                    {
                        var tileA = roomA.EdgeTiles[tileIndexA];
                        var tileB = roomB.EdgeTiles[tileIndexB];
                        var distanceBetweenRooms = (int)(Mathf.Pow(tileA.TileX - tileB.TileX, 2) + Mathf.Pow(tileA.TileY - tileB.TileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }

                    }
                }
            }
            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB, map);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB, map);
            ConnectClosestRooms(allRooms, true, map);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true, map);
        }
    }

    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB, int[,] map)
    {
        Room.ConnectRooms(roomA, roomB);

        //Debug.Log("----------- Start Line -----------");

        Debug.DrawLine(CoordToWorldPoint(tileA, map), CoordToWorldPoint(tileB, map), Color.green, 100f);

        //Debug.Log("Desired Start Point: " + tileA.TileX + " " + tileA.TileY);

        var line = GetLine(tileA, tileB);
        for (int i = 0; i < line.Count; i++)
        {

            //Debug.Log("Real Line Points " + i + " " + line[i].TileX + " " + line[i].TileY);

            var weight = RNG.Next(1, 4);
            if (weight == 3)
                weight = RNG.Next(1, 9);

            map = DrawCircle(line[i], RNG.Next(1, weight), map);
        }

        //Debug.Log("Desired End Point: " + tileB.TileX + " " + tileB.TileY);

        //Debug.Log("------------ End Line ------------");
    }

    int[,] DrawCircle(Coord c, int r, int[,] map)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    var drawX = c.TileX + x;
                    var drawY = c.TileY + y;
                    if (IsInMapRange(map, drawX, drawY))
                    {
                        map[drawX, drawY] = 0;
                    }
                }
            }
        }

        return map;
    }




}
