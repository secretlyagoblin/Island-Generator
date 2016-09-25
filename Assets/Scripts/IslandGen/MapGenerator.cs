using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

public class MapGenerator
    : MonoBehaviour {

    //REMINDER: 1 = Cell on, 0 = Cell off

    [Range(0, 100)]
    public int RandomFillPercent;

    [Range(0, 100)]
    public int NoiseIntensity;

    [Range(0.01f, 10f)]
    public float RandomMapPerlinScale;
    [Range(0.01f, 10f)]
    public float SubzoneGenerationPerlinScale;

    public string Seed;
    public bool UseRandomSeed;
    public int RegionSizeCutoff;

    public Material Material;

    public int Size;

    [Range(0, 10)]
    public int Iterations;

    MeshGenerator _meshGen = new MeshGenerator();

    //Monobehaviour Stuff

    void Start()
    {
        RNG.Init(DateTime.Now.ToString());        

        GenerateMap(Size,Size);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            GenerateMap(Size, Size);
        }
    }

    //Map Gen Functions

    void GenerateMap(int width, int height)
    {
        if (UseRandomSeed)
        {
            Seed = RNG.Next().ToString();
        }
        RNG.ForceInit(Seed);



        var map = new int[width, height];
        var propHeights = new int[width, height];
        RandomFillMap(map);
        for (int i = 0; i < Iterations; i++)
        {
            SmoothMap(map);
        }

        AddRoomLogic(map);


        //var map2 = new int[width, height];
        //var propHeights2 = new int[width, height];
        //RandomFillMap(map2);
        //for (int i = 0; i < Iterations; i++)
        //{
        //    SmoothMap(map2);
        //}
        //
        //map2 = BooleanDifference(map, map2);

        //AddRoomLogic(map2);

        //CreateMesh(GetInvertedMap(map), 5);
        //CreateMesh(GetInvertedMap(map2), 10);


        var subMaps = GenerateSubMaps(6, map);
        var heightMap = CreateNewMapFromTemplate(map);

        var subRegions = new List<List<Coord>>();

        for (int i = 0; i < subMaps.Count; i++)
        {
            SetPropMapHeights(subMaps[i], heightMap, (i));

            subRegions.AddRange(GetRegions(subMaps[i], 0));
        
            //for (int u = 0; u < regions.Count; u++)
            //{
            //    CreateMesh(GetInvertedMap(ResizedMapFromRegion(regions[u])), i);
            //}
        
            //CreateMesh(GetInvertedMap(subMaps[i]), i );
        }



        var layers = GetLayersFromRegions(subRegions, map);



        for (int i = 0; i < layers.Length; i++)
        {
            SetPropMapHeights(layers[i], heightMap, i);
            var count = CountDensity(layers[i]);
            
            //if (count > cellCount)
            //{
            //    cellCount = count;
            //    keyLayer = layers[i];
            //    keyHeight = i;
            //}

            CreateMesh(GetInvertedMap(layers[i]), i);
        }

        CreateTrees(heightMap, 7, 0.4f);

        //MAP TWO ******************************************************

        //var map2 = new int[width, height];
        //propHeights = new int[width, height];
        //RandomFillMap(map2);
        //map2 = BooleanUnion(map2, layers.Last());
        //
        //for (int i = layers.Length-10; i <layers.Length; i++)
        //{
        //    map2 = BooleanDifference(layers[i], map2);
        //}
        //
        //
        //for (int i = 0; i < Iterations; i++)
        //{
        //    SmoothMap(map2);
        //}
        //
        //
        //
        //AddRoomLogic(map2);
        //
        //
        //
        ////map2 = BooleanDifference(layers.Last(), map2);
        //
        //var coord = FindClosestPointInB(layers.Last(), map2);
        //
        //
        //subMaps = GenerateSubMaps(6, map2);
        //heightMap = CreateNewMapFromTemplate(map2);
        //
        //subRegions = new List<List<Coord>>();
        //
        //var layerCountFinal = layers.Count();
        //var finalLayer = layers.Last();
        //
        //for (int i = 0; i < subMaps.Count(); i++)
        //{
        //    subRegions.AddRange(GetRegions(subMaps[i], 0));
        //}
        //
        //layers = GetLayersFromRegions(subRegions, map2, coord);
        //
        //for (int i = 0; i < layers.Length; i++)
        //{
        //    SetPropMapHeights(layers[i], heightMap, i + layerCountFinal);
        //
        //
        //    CreateMesh(GetInvertedMap(layers[i]), i + layerCountFinal);
        //}
        //
        //CreateTrees(heightMap, 7, 0.4f);


    }

    Coord FindClosestPointInB(int[,] mapA, int[,] mapB)
    {
        if (!MapsAreSameDimensions(mapA, mapB))
        {
            Debug.Log("Failed in FindClosestPoint because maps were not valid");
            return new Coord(0, 0);
        }

        Debug.Log("Why are you using this it's blatantly not working properly");

        var width = mapA.GetLength(0);
        var length = mapA.GetLength(1);

        var roomACoords = new List<Coord>();
        var roomBCoords = new List<Coord>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                if (mapA[x, y] == 0)
                {
                    roomACoords.Add(new Coord(x, y));
                }

                if (mapB[x, y] == 0)
                {
                    roomBCoords.Add(new Coord(x, y));
                }
            }
        }

        var bestDistance = int.MaxValue;
        var bestTileA = new Coord();
        var bestTileB = new Coord();

        Debug.Log("I get here");

        var roomA = new Room(roomACoords, mapA);


        var roomB = new Room(roomBCoords, mapB);

        for (int tileIndexA = 0; tileIndexA < roomA.EdgeTiles.Count; tileIndexA++)
        {
            for (int tileIndexB = 0; tileIndexB < roomB.EdgeTiles.Count; tileIndexB++)
            {
                var tileA = roomA.EdgeTiles[tileIndexA];
                var tileB = roomB.EdgeTiles[tileIndexB];
                var distanceBetweenRooms = (int)(Mathf.Pow(tileA.TileX - tileB.TileX, 2) + Mathf.Pow(tileA.TileY - tileB.TileY, 2));
        
                if (distanceBetweenRooms < bestDistance)
                {
                    bestDistance = distanceBetweenRooms;
                    Debug.Log("Best Distance: " + bestDistance);
                    bestTileA = tileA;
                    bestTileB = tileB;
                }
        
            }
        }

        Debug.DrawLine(CoordToWorldPoint(bestTileA,mapA), CoordToWorldPoint(bestTileB, mapB), Color.red, 100f);





        return bestTileB;
    }

    int CountDensity(int[,] map)
    {
        var count = 0;
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y] == 0)
                    count++;
            }
        }

        return count;
    }

    int[,] CreateNewMapFromTemplate(int[,] map)
    {
        return CreateNewMapFromTemplate(map, -1);
    }

    int[,] CreateNewMapFromTemplate(int[,] map, int blankValue)
    {
        var outputMap = new int[map.GetLength(0), map.GetLength(1)];

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                outputMap[x, y] = blankValue;
            }
        }
        return outputMap;
    }

    void SetPropMapHeights(int[,] map, int[,] heightMap, int height)
    {

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y] == 0)
                {
                    heightMap[x, y] = height;
                    //Debug.Log("successfully changed map cell height to " + height);
                }
            }
        }
    }

    //TODO take out radius or do something to it

    void RandomFillMap(int[,] map)
    {
        var width = map.GetLength(0);
        var height = map.GetLength(1);

        var centreX = (int)(width * 0.5f);
        var centreY = (int)(height * 0.5f);
        var radius = Mathf.Pow(height * 0.5f, 2f);   

        var perlinSeed = RNG.NextFloat(0,1000);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1;
                }
                else if (Mathf.Pow(x - centreX, 2) + Mathf.Pow(y - centreY, 2) < radius)
                {
                    float perlinX = perlinSeed + ((x / (float)width) * RandomMapPerlinScale);
                    float perlinY = perlinSeed + ((y / (float)height) * RandomMapPerlinScale);

                    var perlin = Mathf.PerlinNoise(perlinX, perlinY);
                    //Debug.Log("Perlin Per Square: " + perlin);
                    var randomValue = RNG.Next(0, 100 - NoiseIntensity);
                    perlin = (perlin * NoiseIntensity);
              
                    randomValue += (int)perlin;

                    map[x, y] = (randomValue < RandomFillPercent) ? 1 : 0;

                } else {
                    map[x, y] = 1;
                }
            }
        }
    }

    void SmoothMap(int[,] map)
    {
        var width = map.GetLength(0);
        var height = map.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(map, x, y);
                if (neighbourWallTiles > 4)
                {
                    map[x, y] = 1;
                }
                else if (neighbourWallTiles < 4)
                {
                    map[x, y] = 0;
                }
            }
        }
    }

    int[,] GetInvertedMap(int[,] map)
    {
        var returnMap = new int[map.GetLength(0), map.GetLength(1)];

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                returnMap[x, y] = map[x, y] == 0 ? 1 : 0;
            }
        }
        return returnMap;
    }

    //Room Connection Functions

    int[,] AddRoomLogic(int[,] map)
    {
        var wallRegions = GetRegions(map,1);
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

        var roomRegions = GetRegions(map,0);
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

    int[,] ResizedMapFromRegion(List<Coord> regions)
    {
        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = 0;
        var maxY = 0;

        for (int i = 0; i < regions.Count; i++)
        {
            var x = regions[i].TileX;
            var y = regions[i].TileY;

            if (x < minX)
                minX = x;
            if (y < minY)
                minY = y;
            if (x > maxX)
                maxX = x;
            if (y > maxY)
                maxY = y;
        }

        var rangeX = maxX - minX;
        var rangeY = maxY - minY;

        var map = new int[rangeX+2, rangeY+2]; //might be wrong

        map = CreateNewMapFromTemplate(map, 1);

        for (int i = 0; i < regions.Count; i++)
        {
            var x = regions[i].TileX - minX +1;
            var y = regions[i].TileY - minY +1;

            map[x, y] = 0;
        }

        return map;
    }

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

        public Room(List<Coord> roomTiles, int[,] map)
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

    //Helper Functions

    bool IsInMapRange(int[,] map, int x, int y)
    {
        return x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1);
    }

    int[,] DrawCircle(Coord c, int r, int[,] map)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if(x*x + y*y <= r * r)
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

    int[,] BooleanUnion(int[,] mapA, int[,] mapB)
    {
        //Need a check here to avoid failure

        if (!MapsAreSameDimensions(mapA, mapB))
        {
            Debug.Log("Maps are not the same size!");
            return null;
        }

        var outputMap = new int[mapA.GetLength(0), mapA.GetLength(1)];

        for (int x = 0; x < mapA.GetLength(0); x++)
        {
            for (int y = 0; y < mapA.GetLength(1); y++)
            {
                outputMap[x, y] = (mapA[x, y] == 0 | mapB[x, y] == 0) ? 0 : 1;
            }
        }
        return outputMap;
    }

    int[,] BooleanIntersection(int[,] mapA, int[,] mapB)
    {
        //Need a check here to avoid failure

        if (!MapsAreSameDimensions(mapA, mapB))
        {
            Debug.Log("Maps are not the same size!");
            return null;
        }

        var outputMap = new int[mapA.GetLength(0), mapA.GetLength(1)];

        for (int x = 0; x < mapA.GetLength(0); x++)
        {
            for (int y = 0; y < mapA.GetLength(1); y++)
            {
                outputMap[x, y] = (mapA[x, y] == 0 && mapB[x, y] == 0) ? 0 : 1;
            }
        }
        return outputMap;
    }

    int[,] BooleanDifference(int[,] mapA, int[,] mapB)
    {
        //Need a check here to avoid failure

        if (!MapsAreSameDimensions(mapA, mapB))
        {
            Debug.Log("Maps are not the same size!");
            return null;
        }

        var outputMap = new int[mapA.GetLength(0), mapA.GetLength(1)];

        for (int x = 0; x < mapA.GetLength(0); x++)
        {
            for (int y = 0; y < mapA.GetLength(1); y++)
            {
                outputMap[x, y] = (mapA[x, y] == 0) ? 1 : mapB[x, y];
            }
        }
        return outputMap;
    }

    bool MapsAreSameDimensions(int[,] mapA, int[,] mapB)
    {
        return mapA.GetLength(0) == mapB.GetLength(0) && mapA.GetLength(1) == mapB.GetLength(1);
        
    }

    List<Coord> GetLine(Coord from, Coord to)
    {
        var line = new List<Coord>();

        var x = from.TileX;
        var y = from.TileY;

        var dx = to.TileX - x;
        var dy = to.TileY - y;

        var step = Math.Sign(dx);
        var gradientStep = Math.Sign(dy);

        var longest = Mathf.Abs(dx);
        var shortest = Mathf.Abs(dy);

        var inverted = false;
        
        if(longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        var gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }
        return line;
    }

    Vector3 CoordToWorldPoint(Coord tile, int[,] map)
    {
        return new Vector3(-map.GetLength(0) / 2 + .5f + tile.TileX, 2, -map.GetLength(1) / 2 + .5f + tile.TileY);
    }

    struct Coord {
        public int TileX
        { get; private set; }
        public int TileY
        { get; private set; }

        public Coord(int x, int y)
        {
            TileX = x;
            TileY = y;
        }
    }

    //Subdividing Map

    List<int[,]> GenerateSubMaps(int divisions, int[,] map)
    {
        var width = map.GetLength(0);
        var length = map.GetLength(0);

        var perlinSeed = RNG.Next(0, 1000);

        var outputList = new List<int[,]>();

        for (int i = 0; i <= divisions; i++)
        {
            outputList.Add(new int[width, length]);
        }
    
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                float perlinX = perlinSeed + ((x / (float)width) * SubzoneGenerationPerlinScale);
                float perlinY = perlinSeed + ((y / (float)length) * SubzoneGenerationPerlinScale);

                var perlin = Mathf.PerlinNoise(perlinX, perlinY);
                perlin = perlin * divisions;

                for (int i = 0; i <= divisions; i++)
                {
                    if(perlin>i-1 && perlin <= i)
                    {
                        outputList[i][x, y] = map[x,y];
                    }
                    else
                    {
                        outputList[i][x, y] = 1;
                    }
                }
            }
        }

        return outputList;
    }

    void CreateMesh(int[,] subber, int height)
    {
        var mesh = _meshGen.GenerateMesh(subber, 1f, RNG.Next());

        var parent = new GameObject();
        parent.name = "Geometry Layer " + height;
        parent.transform.parent = transform;
        parent.transform.localPosition = Vector3.zero+(Vector3.up* height);

        var renderer = parent.AddComponent < MeshRenderer>();
        var material = new Material(Material);
        material.color = new Color(material.color.r + (RNG.Next(-20, 20) * 0.01f), material.color.g, material.color.b + (RNG.Next(-20, 20) * 0.01f));
        renderer.material = material;
        var filter = parent.AddComponent<MeshFilter>();
        var collider = parent.AddComponent<MeshCollider>();

        collider.sharedMesh = mesh;
        filter.mesh = mesh;

    }

    //Prop Placement

    void CreateTrees(int[,] heightMap, float perlinScale, float treeCutoff)
    {
        var width = heightMap.GetLength(0);
        var length = heightMap.GetLength(1);

        var treePositions = new List<Vector3>();

        var perlinSeed = RNG.NextFloat(0,1000);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                float perlinX = perlinSeed + (((float)x / (float)width) * perlinScale);
                float perlinY = perlinSeed + (((float)y / (float)length) * perlinScale);

                var perlin = Mathf.PerlinNoise(perlinX, perlinY);
                //Debug.Log(perlinSample);

                if (heightMap[x,y] != -1 && perlin > treeCutoff)
                {
                    if(perlin > 0.8 && RNG.Next(0, 3) < 1)
                    {
                        treePositions.Add(new Vector3(x - (width * 0.5f), heightMap[x, y], y - (length * 0.5f)));
                    } else if(RNG.Next(0, 10) < 1)
                        treePositions.Add( new Vector3(x-(width*0.5f), heightMap[x,y], y- (length * 0.5f)));
                }
                else if (heightMap[x, y] != -1 && RNG.Next(0, 100) < 1)
                {
                    treePositions.Add(new Vector3(x - (width * 0.5f), heightMap[x, y], y - (length * 0.5f)));
                }
            }
        }

        var meshes = TreeCreatorAndBatcher.CreateTreeMeshesFromPositions(1f, 0.3f, 4f, 0.3f,1f, treePositions.ToArray());

        for (int i = 0; i < meshes.Length; i++)
        {
            var mesh = meshes[i];

            var parent = new GameObject();
            parent.name = "Tree Mesh " + i;
            parent.transform.parent = transform;
            parent.transform.localPosition = Vector3.zero;

            var renderer = parent.AddComponent<MeshRenderer>();
            var material = new Material(Material);
            material.color = new Color(material.color.r + (RNG.Next(-20, 20) * 0.01f), 1f, material.color.b + (RNG.Next(-20, 20) * 0.01f));
            renderer.material = material;
            var filter = parent.AddComponent<MeshFilter>();

            filter.mesh = mesh;
        }        
    }

    //Debug

    void DebugZone(int[,] map, float height)
    {
        var parent = new GameObject();
        parent.transform.parent = transform;
        parent.transform.localPosition = Vector3.zero;

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y] == 1)
                {
                    var gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    gameObject.transform.parent = parent.transform;
                    gameObject.transform.localPosition = new Vector3(x - (map.GetLength(0) * 0.5f), height, y - (map.GetLength(1) * 0.5f));
                }
            }
        }
    }

    //A* through zonez


    int[][,] GetLayersFromRegions(List<List<Coord>> regions, int[,] mapTemplate)
    {
        return GetLayersFromRegions(regions, mapTemplate, regions[0][0]);
    }

    int[][,] GetLayersFromRegions(List<List<Coord>> regions, int[,] mapTemplate, Coord startPoint)
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
                            if(mapRegions[x,y] != mapRegions[tile.TileX, tile.TileY] && regionHeights[mapRegions[x, y]] == -1)
                            {
                                regionHeights[mapRegions[x, y]] = regionHeights[mapRegions[tile.TileX, tile.TileY]]+1;
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
}