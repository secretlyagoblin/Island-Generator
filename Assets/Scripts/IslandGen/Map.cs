using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class Map {

    public int SizeX
    { get; private set; }

    public int SizeY
    { get; private set; }

    bool _isBoolMask = true;

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

    // Accessors

    public int this[int indexA, int indexB]
    {
        get { return _map[indexA, indexB]; }
        set { _map[indexA, indexB] = value; }
    }

    // Static Functions

    public static Map CloneMap(Map map)
    {
        return CreateBlankMap(map).OverwriteMapWith(map);
    }

    public static Map CreateBlankMap(Map template)
    {
        return new Map(template.SizeX, template.SizeY);
    }

    public static Map CreateHeightMap(Map[] heightData)
    {
        return CloneMap(heightData[0]).AddHeightmapLayers(heightData, 0);
    }

    public static Map BlankMap(int sizeX, int sizeY)
    {
        return new Map(sizeX, sizeY);
    }

    public static Map ApplyMask(Map mapA, Map mapB, Map mask)
    {
        return CloneMap(mapA).ApplyMask(mask, mapB);
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

    public static Map GetInvertedMap(Map map)
    {
        return CloneMap(map).InvertMap();
    }

    // General Functions

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

    public Map ApplyMask(Map maskToApply, Map overlayMap)
    {
        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                if (maskToApply[x, y] == 1)
                    _map[x, y] = overlayMap[x, y];
            }
        }

        return this;
    }

    public Map ApplyMask(Map maskToApply)
    {
        return ApplyMask(maskToApply, maskToApply);
    }

    // Boolean Fill Functions

    public Map RandomFillMap()
    {
        return RandomFillMap(0.5f);
    }

    public Map RandomFillMap(float randomFillPercent)
    {
        return RandomFillMap(randomFillPercent, 0, 1);
    }

    public Map RandomFillMap(float randomFillPercent, float perlinNoiseIntensity, float perlinScale)
    {
        _isBoolMask = true;
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
        _isBoolMask = true;
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

    // Iterative Functions that Require Bool

    public Map SmoothMap()
    {
        if (!_isBoolMask)
        {
            Debug.Log("Only works with boolean Maps");
            return this;
        }

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

    public Map RemoveSmallRegions(int regionSizeCutoff)
    {
        if (!_isBoolMask)
        {
            Debug.Log("Only works with boolean Maps");
            return this;
        }

        var wallRegions = GetRegions(_map, 1);
        var wallThresholdSize = regionSizeCutoff;

        for (int i = 0; i < wallRegions.Count; i++)
        {
            if (wallRegions[i].Count < wallThresholdSize)
            {
                for (int r = 0; r < wallRegions[i].Count; r++)
                {
                    _map[wallRegions[i][r].TileX, wallRegions[i][r].TileY] = 0;
                }
            }
        }

        var roomRegions = GetRegions(_map, 0);
        var roomThresholdSize = regionSizeCutoff;


        for (int i = 0; i < roomRegions.Count; i++)
        {
            if (roomRegions[i].Count < roomThresholdSize)
            {
                for (int r = 0; r < roomRegions[i].Count; r++)
                {
                    _map[roomRegions[i][r].TileX, roomRegions[i][r].TileY] = 1;
                }
            }
        }
        return this;
    }

    public Map AddRoomLogic()
    {
        if (!_isBoolMask)
        {
            Debug.Log("Only works with boolean Maps");
            return this;
        }

        var roomRegions = GetRegions(_map, 0);

        var survivingRooms = new List<Room>();

        for (int i = 0; i < roomRegions.Count; i++)
        {
                survivingRooms.Add(new Room(roomRegions[i],this));
        }

        survivingRooms.Sort();
        survivingRooms[0].IsMainRoom = true;
        survivingRooms[0].IsAccessibleFromMainRoom = true;
        ConnectClosestRooms(survivingRooms, false);

        return this;
    }

    public Map[] GenerateSubMaps(int divisions, float perlinScale)
    {
        if (!_isBoolMask)
        {
            Debug.Log("Only works with boolean Maps");
            return new Map[] { this };
        }


        var perlinSeed = RNG.Next(0, 1000);

        var outputList = new List<Map>();

        for (int i = 0; i <= divisions; i++)
        {
            outputList.Add(new Map(this));
        }

        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                float perlinX = perlinSeed + ((x / (float)SizeX) * perlinScale);
                float perlinY = perlinSeed + ((y / (float)SizeY) * perlinScale);

                var perlin = Mathf.PerlinNoise(perlinX, perlinY);
                perlin = perlin * divisions;

                for (int i = 0; i <= divisions; i++)
                {
                    if (perlin > i - 1 && perlin <= i)
                    {
                        outputList[i][x, y] = _map[x, y];
                    }
                    else
                    {
                        outputList[i][x, y] = 1;
                    }
                }
            }
        }

        return outputList.ToArray();
    }

    public Map InvertMap()
    {
        if (!_isBoolMask)
        {
            Debug.Log("Only works with boolean Maps... currently");
            return this;
        }

        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                _map[x, y] = _map[x, y] == 0 ? 1 : 0;
            }
        }
        return this;
    }

    // Int Fill Functions

    public Map AddHeightmapLayers(Map[] subMaps, int offset)
    {
        _isBoolMask = false;

        for (int i = 0; i < subMaps.Length; i++)
        {
            var map = subMaps[i];

            for (int x = 0; x < map.SizeX; x++)
            {
                for (int y = 0; y < map.SizeY; y++)
                {
                    if (map[x, y] == 0)
                    {
                        _map[x, y] = i+offset;
                        //Debug.Log("successfully changed map cell height to " + height);
                    }
                }
            }
        }



        return this;
    }

    public Map GetLayersFromRegions(Map[][] regions)
    {
        return GetLayersFromRegions(regions, mapTemplate, regions[0][0]);
    }

    public Map GetLayersFromRegions(List<List<Coord>> regions, int[,] mapTemplate, Coord startPoint)
    {
        var sizeX = mapTemplate.GetLength(0);
        var sizeY = mapTemplate.GetLength(1);
        var mapFlags = new int[sizeX, sizeY];
        var mapRegions = new int[sizeX, sizeY];
        var mapHeights = new int[sizeX, sizeY];
        var regionHeights = new int[regions.Count];

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeX; y++)
            {
                mapFlags[x, y] = 1;
                mapRegions[x, y] = -1;
                mapHeights[x, y] = -1;
            }
        }

        for (int i = 0; i < regions.Count; i++)
        {
            regionHeights[i] = -1;
            for (int u = 0; u < regions[i].Count; u++)
            {
                var coord = regions[i][u];
                mapRegions[coord.TileX, coord.TileY] = i;
                mapFlags[coord.TileX, coord.TileY] = 0;
            }
        }

        var queue = new Queue<Coord>();
        var startX = startPoint.TileX;
        var startY = startPoint.TileY;

        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            var tile = queue.Dequeue();
            //tiles.Add(tile);

            for (int x = tile.TileX - 1; x <= tile.TileX + 1; x++)
            {
                for (int y = tile.TileY - 1; y <= tile.TileY + 1; y++)
                {
                    if (IsInMapRange(mapTemplate, x, y) && (y == tile.TileY || x == tile.TileX))
                    {
                        if (mapFlags[x, y] == 0)
                        {
                            if (mapRegions[x, y] != mapRegions[tile.TileX, tile.TileY] && regionHeights[mapRegions[x, y]] == -1)
                            {
                                regionHeights[mapRegions[x, y]] = regionHeights[mapRegions[tile.TileX, tile.TileY]] + 1;
                            }

                            mapFlags[x, y] = 1;
                            mapHeights[x, y] = regionHeights[mapRegions[x, y]];
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        var parents = new List<GameObject>();

        //for (int i = 0; i < regions.Count; i++)
        //{
        //    var parent = new GameObject();
        //    parent.transform.parent = transform;
        //    parent.transform.localPosition = Vector3.zero;
        //    parents.Add(parent);
        //}

        var heights = regionHeights.Distinct().ToList();
        heights.Sort();
        var outputHeights = new List<int[,]>();


        for (int i = 0; i < heights.Count; i++)
        {
            outputHeights.Add(CreateNewMapFromTemplate(mapTemplate, 1));
        }

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (mapRegions[x, y] != -1 && mapHeights[x, y] != -1)
                {


                    var index = heights.IndexOf(mapHeights[x, y]);

                    var ugh = outputHeights[index];

                    ugh[x, y] = 0;
                    //var gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //gameObject.transform.parent = parents[mapRegions[x,y]].transform;
                    //gameObject.transform.localPosition = new Vector3(x - (sizeX * 0.5f), mapHeights[x, y], y - (sizeY * 0.5f));
                }
            }
        }

        return outputHeights.ToArray();



        //for (int x = 0; x < sizeX; x++)
        //{
        //    for (int y = 0; y < sizeY; y++)
        //    {
        //        var tile = mapFlags[x, y];
        //
        //        if (tile.Region == -1)
        //        {
        //
        //        } else if (regionHeights[tile.Region] != -1)
        //            tile.Height = regionHeights[tile.Region];
        //        else
        //        {
        //            var lowestHeight = int.MaxValue;
        //
        //            for (int localX = tile.TileX - 1; localX <= tile.TileX + 1; localX++)
        //            {
        //                for (int localY = tile.TileY - 1; localY <= tile.TileY + 1; localY++)
        //                {
        //                    if (IsInMapRange(mapFlags, localX, localY) && (localY == tile.TileY || localX == tile.TileX))
        //                    {
        //                        var height = regionHeights[mapFlags[localX, localY].Region];
        //                        if (height < lowestHeight && height != -1 && mapFlags[localX, localY].Region != tile.Region)
        //                            lowestHeight = height;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}



        //return null;
    }

    // Region Helper Functions

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

    // Room Helper Functions

    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom)
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
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }    

    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);

        //Debug.Log("----------- Start Line -----------");

        //Debug.DrawLine(CoordToWorldPoint(tileA, _map), CoordToWorldPoint(tileB, _map), Color.green, 100f);

        //Debug.Log("Desired Start Point: " + tileA.TileX + " " + tileA.TileY);

        var line = Coord.GetLine(tileA, tileB);
        for (int i = 0; i < line.Length; i++)
        {

            //Debug.Log("Real Line Points " + i + " " + line[i].TileX + " " + line[i].TileY);

            var weight = RNG.Next(1, 4);
            if (weight == 3)
                weight = RNG.Next(1, 9);

            DrawCircle(line[i], RNG.Next(1, weight));
        }

        //Debug.Log("Desired End Point: " + tileB.TileX + " " + tileB.TileY);

        //Debug.Log("------------ End Line ------------");
    }

    int[,] DrawCircle(Coord c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    var drawX = c.TileX + x;
                    var drawY = c.TileY + y;
                    if (IsInMapRange(_map, drawX, drawY))
                    {
                        _map[drawX, drawY] = 0;
                    }
                }
            }
        }

        return _map;
    }

    // Helper Functions

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

    Vector3 CoordToWorldPoint(Coord tile, int[,] map)
    {
        return new Vector3(-map.GetLength(0) / 2 + .5f + tile.TileX, 2, -map.GetLength(1) / 2 + .5f + tile.TileY);
    }

    int CountDensity()
    {

        var count = 0;
        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                if (_map[x, y] == 0)
                    count++;
            }
        }

        return count;
    }


    // Room Class

    class Room : IComparable<Room> {

        public List<Coord> Tiles
        { get; private set; }
        public List<Coord> EdgeTiles
        { get; private set; }
        public List<Room> ConnectedRooms
        { get; private set; }
        public int RoomSize
        { get; private set; }
        public bool IsAccessibleFromMainRoom
        { get; set; }
        public bool IsMainRoom
        { get; set; }

        public Room()
        {

        }

        public Room(List<Coord> roomTiles, Map map)
        {
            Tiles = roomTiles;
            RoomSize = Tiles.Count;
            ConnectedRooms = new List<Room>();

            EdgeTiles = new List<Coord>();
            for (int i = 0; i < Tiles.Count; i++)
            {
                var tile = Tiles[i];
                for (int x = tile.TileX - 1; x < tile.TileX + 1; x++)
                {
                    for (int y = tile.TileY - 1; y < tile.TileY + 1; y++)
                    {
                        if (x == tile.TileX || y == tile.TileY)
                        {
                            if (map[x, y] == 1)
                            {
                                EdgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }


        }

        public void SetAccessibleFromMainRoom()
        {
            if (!IsAccessibleFromMainRoom)
            {
                IsAccessibleFromMainRoom = true;
                for (int i = 0; i < ConnectedRooms.Count; i++)
                {
                    var room = ConnectedRooms[i];
                    room.IsAccessibleFromMainRoom = true;
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB)
        {
            if (roomA.IsAccessibleFromMainRoom)
            {
                roomB.SetAccessibleFromMainRoom();
            }
            else if (roomB.IsAccessibleFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }

            roomA.ConnectedRooms.Add(roomB);
            roomB.ConnectedRooms.Add(roomA);
        }

        public bool IsConnected(Room otherRoom)
        {
            return ConnectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom)
        {
            return otherRoom.RoomSize.CompareTo(RoomSize);
        }


    }
}
