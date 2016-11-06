using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class Map {

    public int SizeX
    { get; private set; }

    public int SizeY
    { get; private set; }

    float[,] _map;

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

    public Map(int sizeX, int sizeY, int defaultValue)
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
                map[x,y] = mapA[x,y] > mapB[x,y]?mapA[x,y]:mapB[x, y];
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

    public Map Normalise()
    {
        var smallestValue = float.MaxValue;
        var biggestValue = float.MinValue;

        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                var value = (_map[x, y]);
                if (biggestValue < value)
                    biggestValue = value;
                if (smallestValue > value)
                    smallestValue = value;
            }
        }

        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                var value = _map[x, y];
                _map[x, y] = RemapFloat(value, smallestValue, biggestValue, 0f, 1f);
            }
        }

        return this;
    }

    public Map Remap(float min, float max)
    {
        var smallestValue = float.MaxValue;
        var biggestValue = float.MinValue;

        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                var value = (_map[x, y]);
                if (biggestValue < value)
                    biggestValue = value;
                if (smallestValue > value)
                    smallestValue = value;
            }
        }

        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                var value = _map[x, y];
                _map[x, y] = RemapFloat(value, smallestValue, biggestValue, min, max);
            }
        }

        return this;
    }

    public Map Remap(AnimationCurve curve)
    {
        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                var value = _map[x, y];
                _map[x, y] = curve.Evaluate(value);
            }
        }

        return this;
    }

    public Map Multiply(float value)
    {
        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                var currentValue = _map[x, y];
                _map[x, y] = value * currentValue;
            }
        }

        return this;
    }

    public static Map Blend(Map mapA, Map mapB, Map blendMap)
    {
        var outputMap = Map.BlankMap(mapA);

        for (int x = 0; x < mapA.SizeX; x++)
        {
            for (int y = 0; y < mapA.SizeY; y++)
            {
                outputMap[x, y] = (mapA[x, y] * blendMap[x, y]) + (mapB[x, y] * (1 - blendMap[x, y]));
            }
        }

        return outputMap;
    }

    public Map Clamp(float min, float max)
    {
        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                _map[x, y] = Mathf.Clamp(_map[x, y], min, max);
            }
        }

        return this;
    }

    float RemapFloat(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
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
        return CreateCircularFalloff(SizeX * 0.5f);
    }

    public Map CreateCircularFalloff(float radius)
    {
        var centreX = (int)(SizeX * 0.5f);
        var centreY = (int)(SizeY * 0.5f);
        radius = Mathf.Pow(radius, 2f);

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

        var wallRegions = GetRegions(1);
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

        var roomRegions = GetRegions(0);
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

        Profiler.BeginSample("Get Regions");


        var roomRegions = GetRegions(0);

        Profiler.EndSample();

        Profiler.BeginSample("Middler");

        var survivingRooms = new List<Room>();

        for (int i = 0; i < roomRegions.Count; i++)
        {
                survivingRooms.Add(new Room(roomRegions[i],this));
        }

        survivingRooms.Sort();
        survivingRooms[0].IsMainRoom = true;
        survivingRooms[0].IsAccessibleFromMainRoom = true;

        Profiler.EndSample();

        Profiler.BeginSample("Connect Closest Rooms");

        ConnectClosestRooms(survivingRooms, false);

        Profiler.EndSample();

        return this;
    }

    public Map[] GenerateSubMaps(int divisions, float perlinScale)
    {


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

    public Map Invert()
    {
            var smallestValue = float.MaxValue;
            var biggestValue = float.MinValue;

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    var value = (_map[x, y]);
                    if (biggestValue < value)
                        biggestValue = value;
                    if (smallestValue > value)
                        smallestValue = value;
                }
            }

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    var value = _map[x, y];
                    _map[x, y] = -(value - smallestValue) + biggestValue; 
                }
            }

            return this;
        
    }

    public Map ThickenOutline (int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            ThickenOutline();
        }
        return this;
    }

    public Map ThickenOutline()
    {
        var currentSnapshot = Clone(this);

        for (int x = 1; x < SizeX-1; x++)
        {
            for (int y = 1; y < SizeY-1; y++)
            {
                if (currentSnapshot[x, y] == 0)
                {
                    int neighbourWallTiles = GetSurroundingWallCount(currentSnapshot._map, x, y);
                    if (neighbourWallTiles > 0)
                    {
                        _map[x, y] = 1;
                    }
                }
            }
        }

        return this;
    }

    // Int Fill Functions    

    public Map AddHeightmapLayers(Map[] subMaps, int offset)
    {

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

    public Map[] CreateHeightSortedSubmapsFromFloodFill(List<List<Coord>> regions)
    {
        return CreateHeightSortedSubmapsFromFloodFill(regions, regions[0][0]);
    }

    public Map[] CreateHeightSortedSubmapsFromFloodFill(List<List<Coord>> regions, Coord startPoint)
    {
        var sizeX = SizeX;
        var sizeY = SizeY;
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
                    if (IsInMapRange(x, y) && (y == tile.TileY || x == tile.TileX))
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

        var heights = regionHeights.Distinct().ToList();
        heights.Sort();
        var outputHeights = new List<Map>();


        for (int i = 0; i < heights.Count; i++)
        {
            outputHeights.Add(new Map(this,1));
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
                }
            }
        }
        return outputHeights.ToArray();
    }

    public Map[] CreateHeightSortedSubmapsFromDijkstrasAlgorithm(List<List<Coord>> regions)
    {
        var sizeX = SizeX;
        var sizeY = SizeY;
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

        var rooms = new List<Room>();

        for (int i = 0; i < regions.Count; i++)
        {
            regionHeights[i] = -1;
            for (int u = 0; u < regions[i].Count; u++)
            {
                var coord = regions[i][u];
                mapRegions[coord.TileX, coord.TileY] = i;
                mapFlags[coord.TileX, coord.TileY] = 0;
                
            }
            var room = new Room(regions[i], this);
            room.InitForDijkstra();
            rooms.Add(room);
        }

        var startPoint = regions[0][0];
        var queue = new Queue<Coord>();
        var startX = startPoint.TileX;
        var startY = startPoint.TileY;

        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            var tile = queue.Dequeue();

            for (int x = tile.TileX - 1; x <= tile.TileX + 1; x++)
            {
                for (int y = tile.TileY - 1; y <= tile.TileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.TileY || x == tile.TileX))
                    {
                        if (mapFlags[x, y] == 0)
                        {
                            if (mapRegions[x, y] != mapRegions[tile.TileX, tile.TileY] && regionHeights[mapRegions[x, y]] == -1)
                            {
                                regionHeights[mapRegions[x, y]] = regionHeights[mapRegions[tile.TileX, tile.TileY]] + 1;
                            }
                            mapFlags[x, y] = 1;
                            var roomA = rooms[mapRegions[tile.TileX, tile.TileY]];
                            var roomB = rooms[mapRegions[x, y]];
                            queue.Enqueue(new Coord(x, y));


                            if (roomA == roomB)
                            {

                            }
                            else if (roomA.IsAbstractlyConnected(roomB))
                            {

                            }
                            else
                            {
                                roomA.ConnectedRooms.Add(roomB);
                                roomB.ConnectedRooms.Add(roomA);
                            }
                        }
                    }
                }
            }
        }

        Room currentRoom = null;

        var unvistedSet = new List<Room>(rooms);
        //unvistedSet.RemoveAt(0);

        var firstIteration = true;
        var finished = false;

        var failCount = 0;

        while (!finished)
        {
            failCount++;
            if(failCount > 1000)
            {
                return null;
            }
            currentRoom = unvistedSet.Last();

            if (firstIteration)
            {
                firstIteration = false;
                currentRoom.DijkstraDistance = 0;
            }
            
            for (int i = 0; i < currentRoom.ConnectedRooms.Count; i++)
            {
                var connectedRoom = currentRoom.ConnectedRooms[i];

                if (connectedRoom.Visited)
                { }
                else
                {
                    var currentDistance = currentRoom.DijkstraDistance + 1;

                    if (currentDistance < connectedRoom.DijkstraDistance)
                        connectedRoom.DijkstraDistance = currentDistance;
                }
            }

            currentRoom.Visited = true;
            unvistedSet.Remove(currentRoom);

            if (unvistedSet.Count == 0)
            { finished = true; }
            else
            {
                unvistedSet = unvistedSet.OrderByDescending(x => x.DijkstraDistance).ToList();

                if (unvistedSet.Last().DijkstraDistance == int.MaxValue)
                {
                    finished = true;
                }
                else
                {
                    currentRoom = unvistedSet.Last();
                }
            }
        }

        var outputRegionHeights = new List<int>();

        for (int i = 0; i < rooms.Count; i++)
        {
            outputRegionHeights.Add(rooms[i].DijkstraDistance);
        }

        var heights = outputRegionHeights.Distinct().ToList();
        heights.Sort();
        var outputHeights = new List<Map>();
        var heightsDict = new Dictionary<int, Map>();


        for (int i = 0; i < heights.Count; i++)
        {
            var map = new Map(this, 1);
            outputHeights.Add(map);
            heightsDict.Add(heights[i], map);

        }

        for (int i = 0; i < rooms.Count; i++)
        {
            var map = heightsDict[rooms[i].DijkstraDistance];
            for (int u = 0; u < rooms[i].Tiles.Count; u++)
            {
                var coord = rooms[i].Tiles[u];
                map[coord.TileX, coord.TileY] = 0;
            }
        }



        return outputHeights.ToArray();

    }

    // Float Fill Functions

    public Map GetDistanceMap(int searchDistance)
    {
        var distanceMap = new Map(SizeX,SizeY,0);
        

        var currentSnapshot = Clone(this);

        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                // Get pixel
                float a = _map[x,y];

                // Distance to closest pixel which is the inverse of a
                // start on float.MaxValue so we can be sure we found something
                float distance = float.MaxValue;

                // Search coordinates, x min/max and y min/max
                int fxMin = Math.Max(x - searchDistance, 0);
                int fxMax = Math.Min(x + searchDistance, SizeX);
                int fyMin = Math.Max(y - searchDistance, 0);
                int fyMax = Math.Min(y + searchDistance, SizeY);

                for (int fx = fxMin; fx < fxMax; ++fx)
                {
                    for (int fy = fyMin; fy < fyMax; ++fy)
                    {
                        // Get pixel to compare to
                        float p = _map[fx, fy];

                        // If not equal a
                        if (a != p)
                        {
                            // Calculate distance
                            float xd = x - fx;
                            float yd = y - fy;
                            float d = Mathf.Sqrt((xd * xd) + (yd * yd));

                            // Compare absolute distance values, and if smaller replace distnace with the new oe
                            if (Math.Abs(d) < Math.Abs(distance))
                            {
                                distance = d;
                            }
                        }
                    }
                }

                if (_map[x, y] == 1)
                    distance = -distance;

                // If we found a new distance, otherwise we'll just use A 

                if (distance != float.MaxValue && distance != float.MinValue)
                {

                    // Clamp distance to -/+ 
                    distance = Mathf.Clamp(distance, -searchDistance, +searchDistance);

                    // Convert from -search,+search to 0,+search*2 and then convert to 0.0, 1.0 and invert
                    a = 1f - Mathf.Clamp((distance + searchDistance) / (searchDistance + searchDistance), 0, 1);
                }

                // Write pixel out


                        
                distanceMap[x, y] = a;


            }
        }

        _map = distanceMap._map;




        return this;
    }

    public Map PerlinFillMap(float perlinScale, int mapCoordinateX, int mapCoordinateY, float seed)
    {
        var perlinSeed = RNG.NextFloat(0, 10000f);

        if (perlinScale <= 0)
            perlinScale = 0.0001f;

        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {

                var perlinX = seed + (mapCoordinateX*SizeX) + (x / perlinScale);
                var perlinY = seed + (mapCoordinateY*SizeY) + (y / perlinScale);

                var perlin = Mathf.PerlinNoise(perlinX, perlinY);

                _map[x, y] = perlin;
            }
        }

        return this;
    }

    public Map PerlinFillMap(float perlinScale, int mapCoordinateX, int mapCoordinateY, float seed, int octaves, float persistance, float lacunarity)
    {

        if (perlinScale <= 0)
            perlinScale = 0.0001f;

        var maxNoiseHeight = float.MinValue;
        var minNoiseHeight = float.MaxValue;

        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                var amp = 1f;
                var freq = 1f;
                var noiseHeight = 0f;

                for (int i = 0; i < octaves; i++)
                {
                    var perlinX = seed + (mapCoordinateX * SizeX) + (x / perlinScale * freq);
                    var perlinY = seed + (mapCoordinateY * SizeY) + (y / perlinScale * freq);

                    var perlin = Mathf.PerlinNoise(perlinX, perlinY) * 2 - 1;
                    noiseHeight += perlin * amp;
                    amp *= persistance;
                    freq *= lacunarity;


                }

                if (noiseHeight > maxNoiseHeight)
                    maxNoiseHeight = noiseHeight;
                if (noiseHeight < minNoiseHeight)
                    minNoiseHeight = noiseHeight;


                _map[x, y] = noiseHeight;


            }
        }

        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                _map[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, _map[x, y]);
            }
        }

        return this;
    }

    // Region Helper Functions

    public List<List<Coord>> GetRegions(int tileType)
    {
        var regions = new List<List<Coord>>();
        var mapFlags = new int[SizeX, SizeY];

        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                if (mapFlags[x, y] == 0 && _map[x, y] == tileType)
                {
                    var newRegion = GetRegionTiles( x, y);
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

    List<Coord> GetRegionTiles(int startX, int startY)
    {

        var tiles = new List<Coord>();
        var mapFlags = new int[SizeX, SizeY];
        int tileType = Mathf.RoundToInt(_map[startX, startY]);

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
                    if (IsInMapRange(x, y) && (y == tile.TileY || x == tile.TileX))
                    {
                        if (mapFlags[x, y] == 0 && _map[x, y] == tileType)
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
                if (roomA == roomB || roomA.IsAbstractlyConnected(roomB))
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

    float[,] DrawCircle(Coord c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    var drawX = c.TileX + x;
                    var drawY = c.TileY + y;
                    if (IsInMapRange(drawX, drawY))
                    {
                        _map[drawX, drawY] = 0;
                    }
                }
            }
        }

        return _map;
    }

    // Helper Functions

    int GetSurroundingWallCount(float[,] map, int gridX, int gridY)
    {
        int wallCount = 0;
        for (int x = gridX - 1; x <= gridX + 1; x++)
        {
            for (int y = gridY - 1; y <= gridY + 1; y++)
            {
                if (IsInMapRange(x, y))
                {
                    if (x != gridX || y != gridY)
                    {
                        wallCount += Mathf.RoundToInt(map[x, y]);
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

    public bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < SizeX && y >= 0 && y < SizeY;
    }

    Vector3 CoordToWorldPoint(Coord tile, Map map)
    {
        return new Vector3(-map.SizeX / 2 + .5f + tile.TileX, 2, -map.SizeY / 2 + .5f + tile.TileY);
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

        public int DijkstraDistance
        { get; set; }

        public bool Visited
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

        public void InitForDijkstra()
        {
            DijkstraDistance = int.MaxValue;
            Visited = false;
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

        public bool IsAbstractlyConnected(Room otherRoom)
        {
            return ConnectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom)
        {
            return otherRoom.RoomSize.CompareTo(RoomSize);
        }

        public bool IsLiterallyConnected(Room otherRoom)
        {
            for (int a = 0; a < Tiles.Count; a++)
            {
                for (int b = 0; b < otherRoom.Tiles.Count; b++)
                {
                    var coordA = Tiles[a];
                    var coordB = otherRoom.Tiles[b];

                    var innerX = coordA.TileX - coordB.TileX;
                    var innerY = coordA.TileY - coordB.TileY;

                    innerX = Math.Abs(innerX);
                    innerY = Math.Abs(innerY);

                    if (innerX + innerY < 2)
                        return true;
                }
            }
            return false;
        }

        public Vector3 GetCenter(int SizeX, int SizeY)
        {
            var centerX = 0;
            var centerY = 0;
            for (int i = 0; i < Tiles.Count; i++)
            {
                centerX += Tiles[i].TileX;
                centerY += Tiles[i].TileY;
            }

            return new Vector3(-SizeX / 2 + .5f + ((float)centerX/Tiles.Count), 50, -SizeY / 2 + .5f + ((float)centerY / Tiles.Count));
        }


    }
}
