using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Maps {


    public partial class Map {
        // General Functions

        public Map FillWith(float value)
        {
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    _map[x, y] = value;
                }
            }

            return this;
        }

        public Map OverwriteMapWith(Map map)
        {
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

        public Map Multiply(Map other)
        {
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    var currentValue = _map[x, y];
                    _map[x, y] = _map[x, y] * other._map[x, y];
                }
            }

            return this;
        }

        public Map Add(Map other)
        {

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    _map[x, y] = _map[x, y] + other._map[x, y];
                }
            }

            return this;
        }

        public Map Clone()
        {
            return Clone(this);
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

        public Map ShiftLowestValueToZero()
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

            var displacement = -smallestValue;

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    _map[x, y] += displacement;
                }
            }

            return this;
        }

        float RemapFloat(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            var t = Mathf.InverseLerp(fromMin, fromMax, value);

            return Mathf.LerpUnclamped(toMin, toMax, t);

            //return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
        }

        // Boolean Fill Functions

        public Map FillWithBoolNoise()
        {
            return FillWithBoolNoise(0.5f);
        }

        public Map FillWithBoolNoise(float randomFillPercent)
        {
            return FillWithBoolNoise(randomFillPercent, 0, 1);
        }

        public Map FillWithNoise()
        {
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    _map[x, y] = RNG.NextFloat(0, 1);
                }
            }

            return this;
        }

        public Map FillWithBoolNoise(float randomFillPercent, float perlinNoiseIntensity, float perlinScale)
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

        public Map CreateYGradient()
        {
            var ySize = 1f / SizeY;

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    _map[x, y] = ySize * y;
                }
            }

            return this;
        }

        public Map CreateTerrainMap()
        {
            var newMap = new Map(SizeY, SizeX);

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    newMap[y, x] = this[x, y];
                }
            }

            return newMap;
        }

        // Vector Functions


        // List Fuctions

        public float[] GetColumn(int xIndex)
        {
            var outputFloat = new float[SizeY];

            for (int y = 0; y < SizeY; y++)
            {
                outputFloat[y] = _map[xIndex, y];
            }

            return outputFloat;
        }

        public float[] GetRow(int yIndex)
        {
            var outputFloat = new float[SizeX];

            for (int x = 0; x < SizeX; x++)
            {
                outputFloat[x] = _map[x, yIndex];
            }

            return outputFloat;
        }

        public Map SetRow(int xIndex, float value)
        {
            for (int y = 0; y < SizeY; y++)
            {
                _map[xIndex, y] = value;
            }

            return this;
        }

        public Map SetColumn(int yIndex, float value)
        {
            for (int x = 0; x < SizeX; x++)
            {
                _map[x, yIndex] = value;
            }

            return this;
        }

        public Map SetIndex(int x, int y, float value)
        {
            _map[x, y] = value;
            return this;
        }

        public Vector3 GetNormalisedVector3FromIndex(int xIndex, int yIndex)
        {
            var x = Mathf.InverseLerp(0, SizeX, xIndex);
            var y = Mathf.InverseLerp(0, SizeY, yIndex);

            return new Vector3(x, _map[xIndex, yIndex], y);
        }

        public float BilinearSampleFromNormalisedVector2(Vector2 normalisedVector)
        {

            if (normalisedVector.x > 1 | normalisedVector.y > 1 | normalisedVector.x < 0 | normalisedVector.y < 0)
            {
                Debug.Log("You aren't normal and as such are not welcome here, in this debug log");

                Debug.Log(normalisedVector);
            }

            float u = normalisedVector.x * (SizeX - 1);
            float v = normalisedVector.y * (SizeY - 1);
            int x = (int)Mathf.Floor(u);
            int y = (int)Mathf.Floor(v);
            if(u == x && u!= 0)
                x--;
            if(v == y && v != 0)
                y--;          
            float u_ratio = u - x;
            float v_ratio = v - y;
            float u_opposite = 1f - u_ratio;
            float v_opposite = 1f - v_ratio;


            float result = (_map[x, y] * u_opposite + _map[x + 1, y] * u_ratio) * v_opposite +
                       (_map[x, y + 1] * u_opposite + _map[x + 1, y + 1] * u_ratio) * v_ratio;
            return result;


        }

        public Vector3[] GetNormalisedVectorColumn(int xIndex)
        {
            var outputVectorArray = new Vector3[SizeY];

            for (int y = 0; y < SizeY; y++)
            {
                outputVectorArray[y] = new Vector3(xIndex / (float)SizeX, _map[xIndex, y], y / (float)SizeY);
            }

            return outputVectorArray;
        }

        public Vector3[] GetNormalisedVectorRow(int yIndex)
        {
            var outputVectorArray = new Vector3[SizeX];

            for (int x = 0; x < SizeX; x++)
            {
                outputVectorArray[x] = new Vector3(x / (float)SizeX, _map[x, yIndex], yIndex / (float)SizeY);
            }

            return outputVectorArray;
        }

        // Iterative Functions that Require Bool

        public Map BoolSmoothOperation()
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

        public Map BoolSmoothOperation(int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                BoolSmoothOperation();
            }
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

        public Map[,] GenerateNonUniformSubmapsOverlappingWherePossible(int divisions)
        {
            var divX = SizeX / divisions;
            var divY = SizeY / divisions;

            var output = new Map[divisions, divisions];

            for (int x = 0; x < divisions; x++)
            {
                for (int y = 0; y < divisions; y++)
                {
                    var startX = divX * x;
                    var startY= divY * y;
                    
                    //determines if we'll run out of map this time round
                    var sizeX = (divX * (x + 1))+1 < SizeX?divX+1:divX;
                    var sizeY = (divY * (y + 1)) + 1 < SizeY ? divY + 1 : divY;

                    var map = new Map(sizeX, sizeY);

                    for (int u = 0; u < sizeX; u++)
                    {
                        for (int v = 0; v < sizeY; v++)
                        {

                            map[u, v] = this[u + startX, v + startY];
                        }
                    }
                    output[x, y] = map;
                }
            }

            return output;
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

        public Map MirrorY()
        {
            var outputArray = new float[SizeX,SizeY];

            for (int x = 0; x < SizeX; x++)
            {
                var size = SizeY;
                for (int y = 0; y < SizeY; y++)
                {
                    size--;
                    var value = _map[x, y];
                    outputArray[x, size] = value;
                }
            }

            _map = outputArray;

            return this;

        }

        public Map Rotate90()
        {
            var outputArray = new float[SizeX, SizeY];

            for (int x = 0; x < SizeX; x++)
            {
                //var size = SizeY;
                for (int y = 0; y < SizeY; y++)
                {
                    //size--;
                    var value = _map[x, y];
                    outputArray[y, x] = value;
                }
            }

            _map = outputArray;

            return this;

        }

        public Map MirrorX()
        {
            var outputArray = new float[SizeX, SizeY];

            for (int y = 0; y < SizeY; y++)
            {
                var size = SizeX;

                for (int x = 0; x < SizeX; x++)
                {
                    size--;
                    var value = _map[x, y];
                    outputArray[size, y] = value;
                }
            }

            _map = outputArray;

            return this;

        }

        public Map ThickenOutline(int iterations)
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

            for (int x = 1; x < SizeX - 1; x++)
            {
                for (int y = 1; y < SizeY - 1; y++)
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
                            _map[x, y] = i + offset;
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
                    mapRegions[coord.x, coord.y] = i;
                    mapFlags[coord.x, coord.y] = 0;
                }
            }

            var queue = new Queue<Coord>();
            var startX = startPoint.x;
            var startY = startPoint.y;

            queue.Enqueue(new Coord(startX, startY));
            mapFlags[startX, startY] = 1;

            while (queue.Count > 0)
            {
                var tile = queue.Dequeue();
                //tiles.Add(tile);

                for (int x = tile.x - 1; x <= tile.x + 1; x++)
                {
                    for (int y = tile.y - 1; y <= tile.y + 1; y++)
                    {
                        if (IsInMapRange(x, y) && (y == tile.y || x == tile.x))
                        {
                            if (mapFlags[x, y] == 0)
                            {
                                if (mapRegions[x, y] != mapRegions[tile.x, tile.y] && regionHeights[mapRegions[x, y]] == -1)
                                {
                                    regionHeights[mapRegions[x, y]] = regionHeights[mapRegions[tile.x, tile.y]] + 1;
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
                outputHeights.Add(new Map(this, 1));
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
                    mapRegions[coord.x, coord.y] = i;
                    mapFlags[coord.x, coord.y] = 0;

                }
                var room = new Room(regions[i], this);
                room.InitForDijkstra();
                rooms.Add(room);
            }

            var startPoint = regions[0][0];
            var queue = new Queue<Coord>();
            var startX = startPoint.x;
            var startY = startPoint.y;

            queue.Enqueue(new Coord(startX, startY));
            mapFlags[startX, startY] = 1;

            while (queue.Count > 0)
            {
                var tile = queue.Dequeue();

                for (int x = tile.x - 1; x <= tile.x + 1; x++)
                {
                    for (int y = tile.y - 1; y <= tile.y + 1; y++)
                    {
                        if (IsInMapRange(x, y) && (y == tile.y || x == tile.x))
                        {
                            if (mapFlags[x, y] == 0)
                            {
                                if (mapRegions[x, y] != mapRegions[tile.x, tile.y] && regionHeights[mapRegions[x, y]] == -1)
                                {
                                    regionHeights[mapRegions[x, y]] = regionHeights[mapRegions[tile.x, tile.y]] + 1;
                                }
                                mapFlags[x, y] = 1;
                                var roomA = rooms[mapRegions[tile.x, tile.y]];
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
                if (failCount > 1000)
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
                    map[coord.x, coord.y] = 0;
                }
            }



            return outputHeights.ToArray();

        }

        // Float Fill Functions

        public Map GetDistanceMap(int searchDistance)
        {
            var distanceMap = new Map(SizeX, SizeY, 0);


            var currentSnapshot = Clone(this);

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    // Get pixel
                    float a = _map[x, y];

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

        public Map PerlinFill(float perlinScale, int mapCoordinateX, int mapCoordinateY, float seed)
        {
            //var perlinSeed = RNG.NextFloat(0, 10000f);

            if (perlinScale <= 0)
                perlinScale = 0.0001f;

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    //var perlinX = seed + (mapCoordinateX * SizeX) + (x / perlinScale);
                    //var perlinY = seed + (mapCoordinateY * SizeY) + (y / perlinScale);

                    var perlinX = (((mapCoordinateX * SizeX)+ x) / perlinScale) +seed;
                    var perlinY = (((mapCoordinateY * SizeY) + y) / perlinScale) + seed;


                    var perlin = Mathf.PerlinNoise(perlinX, perlinY);

                    _map[x, y] = perlin;
                }
            }

            return this;
        }

        public Map PerlinFillOctaves(float perlinScale, int mapCoordinateX, int mapCoordinateY, float seed, int octaves, float persistance, float lacunarity)
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

        public Map PerlinFillMap(float perlinScale, Domain noiseDomain, Coord mapTile, Vector2 mapTileSize, Vector2 seedOffset, int octaves, float persistance, float lacunarity)
        {

            if (perlinScale <= 0)
                perlinScale = 0.0001f;

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    var amp = 1f;
                    var freq = 1f;
                    var noiseHeight = 0f;

                    for (int i = 0; i < octaves; i++)
                    {

                        //find sample position between 0..1

                        float localTilePositionX = x / (float)SizeX;
                        float localTilePositionY = y / (float)SizeY;

                        //offset by tile position, for example 0.332 -> 3.332

                        localTilePositionX += mapTile.x;
                        localTilePositionY += mapTile.y;

                        //multiply by scale of the mapTile in real space, i.e. 3.332 * 1.2 = 3.9984

                        localTilePositionX *= mapTileSize.x;
                        localTilePositionY *= mapTileSize.y;

                        //add seed to offset off of zero

                        localTilePositionX += seedOffset.x;
                        localTilePositionY += seedOffset.y;

                        //multiply by perlinScale

                        float perlinX = perlinScale * localTilePositionX * freq;
                        float perlinY = perlinScale * localTilePositionY * freq;

                        var perlin = Mathf.PerlinNoise(perlinX, perlinY);

                        noiseHeight += perlin * amp;
                        amp *= persistance;
                        freq *= lacunarity;


                    }

                    noiseHeight = noiseDomain.Clamp(noiseHeight);

                    _map[x, y] = noiseHeight;


                }
            }

            return this;
        }

        public Map SmoothMap()
        {
            var nextMap = new Map(this);

            for (int gridX = 0; gridX < SizeX; gridX++)
            {
                for (int gridY = 0; gridY < SizeY; gridY++)
                {
                    var count = 0;
                    var totalValue = 0f;

                    for (int x = gridX - 1; x <= gridX + 1; x++)
                    {
                        for (int y = gridY - 1; y <= gridY + 1; y++)
                        {
                            var localX = Mathf.Clamp(x, 0, SizeX-1);
                            var localY = Mathf.Clamp(y, 0, SizeY-1);

                            var weight = 0.7f;

                            if (localX != gridX || localY != gridY)
                            {
                                weight = 1;
                            }

                            totalValue += (this[localX, localY] * weight);                            

                            count++;
                        }
                    }


                    nextMap[gridX, gridY] = (totalValue / count);
                }
            }

            _map = nextMap._map;

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

        public Map GetSteepnessMapFromTerrain(TerrainData data)
        {
            for (int gridX = 0; gridX < SizeX; gridX++)
            {
                for (int gridY = 0; gridY < SizeY; gridY++)
                {
                    var normalX = Mathf.InverseLerp(0, SizeX-1, gridX);
                    var normalY = Mathf.InverseLerp(0, SizeY - 1, gridY);
                    this[gridX,gridY] = data.GetSteepness(normalX, normalY);
                }
            }
            return this;
        }

        public Map GetBumpMap()
        {
            return GetBumpMap(false);
        }

        public Map GetAbsoluteBumpMap()
        {
            return GetBumpMap(true);
        }

        Map GetBumpMap(bool absolute)
        {
            var nextMap = new Map(this);

            for (int gridX = 0; gridX < SizeX; gridX++)
            {
                for (int gridY = 0; gridY < SizeY; gridY++)
                {
                    var count = 0;
                    var totalVector = 0f;

                    for (int x = gridX - 1; x <= gridX + 1; x++)
                    {
                        for (int y = gridY - 1; y <= gridY + 1; y++)
                        {
                            if (IsInMapRange(x, y))
                            {
                                var weight = 0.7f;

                                if (x != gridX || y != gridY)
                                {
                                    weight = 1;
                                }

                                var xDiff = gridX - x;
                                var yDiff = gridY - y;
                                var zDiff = _map[gridX, gridY] - _map[x, y];

                                var angleVector = new Vector3(xDiff, yDiff, zDiff);
                                angleVector.Normalize();

                                if (absolute)
                                {
                                    totalVector += Mathf.Abs((angleVector.z * weight));
                                }
                                else
                                {
                                    totalVector += (angleVector.z * weight);
                                }







                            }

                            count++;
                        }
                    }


                    nextMap[gridX, gridY] = (totalVector / count);
                }
            }

            _map = nextMap._map;

            return this;
        }

        public Map LerpHeightMap(Map mask, AnimationCurve falloffCurve)
        {
            var outputMapY = new Map(this);

            for (int x = 0; x < SizeX; x++)
            {
                var column = GetGridColumn(this, x, false);
                var maskColumn = GetGridColumn(mask, x, false);
                var heightmapRow = CalculateHeightmapRow(column, maskColumn, falloffCurve);
                outputMapY.ApplyHeightmapRow(heightmapRow, x, false);
            }

            var outputMapX = new Map(this);


            for (int y = 0; y < SizeY; y++)
            {
                var column = GetGridColumn(this, y, true);
                var maskColumn = GetGridColumn(mask, y, true);
                var heightmapRow = CalculateHeightmapRow(column, maskColumn, falloffCurve);
                outputMapX.ApplyHeightmapRow(heightmapRow, y, true);
            }

            var output = (outputMapX + outputMapY).Multiply(0.5f);

            _map = output._map;

            return this;
        }

        //LerpHeightmap Helper Functions

        float[] GetGridColumn(Map map, int index, bool columnA)
        {
            if (!columnA)
            {
                var length = map.SizeY;
                var outputArray = new float[length];

                for (int i = 0; i < length; i++)
                {
                    outputArray[i] = map[index, i];
                }
                return outputArray;
            }
            else
            {
                var length = map.SizeX;
                var outputArray = new float[length];

                for (int i = 0; i < length; i++)
                {
                    outputArray[i] = map[i, index];
                }
                return outputArray;
            }
        }

        float[] CalculateHeightmapRow(float[] heightmapRow, float[] heightmapMask, AnimationCurve curve)
        {

            HeightmapLerpSegment heightmapLerpSegment = null;
            var inLoop = false;

            for (int i = 0; i < heightmapRow.Length; i++)
            {

                var index = heightmapRow[i];

                if (inLoop)
                {
                    if (heightmapMask[i] != 1)
                    {
                        inLoop = false;
                        heightmapLerpSegment.Close(i, index);
                        heightmapRow = heightmapLerpSegment.Apply(heightmapRow, curve);
                    }



                }
                else
                {
                    if (heightmapMask[i] == 1)
                    {
                        inLoop = true;
                        heightmapLerpSegment = new HeightmapLerpSegment(Mathf.Max(0, i - 1), heightmapRow[Mathf.Max(0, i - 1)]);
                    }
                }
            }

            if (inLoop)
            {
                heightmapLerpSegment.Close(heightmapRow.Length, heightmapRow[heightmapRow.Length - 1]);
                heightmapRow = heightmapLerpSegment.Apply(heightmapRow, curve);
            }

            return heightmapRow;
        }

        Map ApplyHeightmapRow(float[] heightmapRow, int index, bool columnA)
        {
            if (!columnA)
            {
                var length = heightmapRow.Length;

                for (int i = 0; i < length; i++)
                {
                    _map[index, i] = heightmapRow[i];
                }
                return this;
            }
            else
            {
                var length = heightmapRow.Length;

                for (int i = 0; i < length; i++)
                {
                    _map[i, index] = heightmapRow[i];
                }
                return this;
            }
        }

        public bool IsInMapRange(int x, int y)
        {
            return x >= 0 && x < SizeX && y >= 0 && y < SizeY;
        }

        Vector3 CoordToWorldPoint(Coord tile, Map map)
        {
            return new Vector3(-map.SizeX / 2 + .5f + tile.x, 2, -map.SizeY / 2 + .5f + tile.y);
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

        // Mesh Mapping

        public Map GetHeightmapFromSquareXZMesh(Mesh mesh)
        {
            var tris = mesh.triangles;
            var verts = mesh.vertices;

            for (int i = 0; i < tris.Length; i+=3)
            {
                var v1 = verts[tris[i]];
                var v2 = verts[tris[i+1]];
                var v3 = verts[tris[i+2]];

                var xs = new List<int> { Mathf.RoundToInt(v1.x * SizeX), Mathf.RoundToInt(v2.x * SizeX), Mathf.RoundToInt(v3.x * SizeX) };
                xs.Sort();

                var xMin = xs.First();
                var xMax = xs.Last();

                xMin--;
                xMax++;

                xMin = xMin < 0 ? 0 : xMin;
                xMax = xMax > SizeX ? SizeX : xMax;
                
                var zs = new List<int> { Mathf.RoundToInt(v1.z * SizeY), Mathf.RoundToInt(v2.z * SizeY), Mathf.RoundToInt(v3.z * SizeY) };
                zs.Sort();

                var zMin = zs.First();
                var zMax = zs.Last();

                zMin--;
                zMax++;

                zMin = zMin < 0 ? 0 : zMin;
                zMax = zMax > SizeY ? SizeY : zMax;

                v1 = new Vector3(v1.x * SizeX, v1.y, v1.z * SizeY);
                v2 = new Vector3(v2.x * SizeX, v2.y, v2.z * SizeY);
                v3 = new Vector3(v3.x * SizeX, v3.y, v3.z * SizeY);

                var plane = new Plane(v1, v2, v3);

                


                for (int x = xMin; x < xMax; x++)
                {
                    for (int y = zMin; y < zMax; y++)
                    {
                        if (PointInTriangle(new Vector3(x, 0, y), v1, v2, v3))
                        {
                            var ray = new Ray(new Vector3(x,0,y), Vector3.up);
                            var dist = 0f;
                            plane.Raycast(ray, out dist);
                            this[x, y] = dist;
                                }
                    }
                }

            }

            return this;
        }

        float sign(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return (p1.x - p3.x) * (p2.z - p3.z) - (p2.x - p3.x) * (p1.z - p3.z);
        }

        bool PointInTriangle(Vector3 pt, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            bool b1, b2, b3;

            b1 = sign(pt, v1, v2) < 0.0f;
            b2 = sign(pt, v2, v3) < 0.0f;
            b3 = sign(pt, v3, v1) < 0.0f;

            return ((b1 == b2) && (b2 == b3));
        }

        //Texture Modifications

        public Texture2D ApplyToTexture(Texture2D texture)
        {
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    texture.SetPixel(x, y, new Color(_map[x, y], _map[x, y], _map[x, y]));
                }
            }

            return texture;
        }

        public Texture2D ApplyToTexture(Texture2D texture, Gradient gradient)
        {
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    texture.SetPixel(x, y, gradient.Evaluate(_map[x, y]));

                }
            }

            return texture;
        }

        public Texture2D ApplyToTexture(Texture2D texture, Gradient gradient, Map mask)
        {
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    if (mask[x, y] == 1)
                    {

                        texture.SetPixel(x, y, gradient.Evaluate(_map[x, y]));
                    }
                }
            }

            return texture;
        }

        public Map WarpMapToMatch(Map mapToWarp)
        {
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    var point = GetNormalisedVector3FromIndex(x, y);

                    _map[x, y] = mapToWarp.BilinearSampleFromNormalisedVector2(new Vector2(point.x, point.z));

                    //Debug.Log("Normalised Point: " + point + ", Value: " + _map[x, y]);
                }
            }

            return this;
        }



        public Map Display(MeshDebugStack stack)
        {
            stack.RecordMapStateToStack(this);
            return this;
        }





    }

}
