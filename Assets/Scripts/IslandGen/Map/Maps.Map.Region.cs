﻿using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Maps {

    public partial class Map {
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
                        _map[wallRegions[i][r].x, wallRegions[i][r].y] = 0;
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
                        _map[roomRegions[i][r].x, roomRegions[i][r].y] = 1;
                    }
                }
            }
            return this;
        }

        public Map AddRoomLogic()
        {

            //UnityEngine.Profiling.Profiler.BeginSample("Get Regions");


            var roomRegions = GetRegions(0);

            //UnityEngine.Profiling.Profiler.EndSample();

            //UnityEngine.Profiling.Profiler.BeginSample("Middler");

            var survivingRooms = new List<Room>();

            for (int i = 0; i < roomRegions.Count; i++)
            {
                survivingRooms.Add(new Room(roomRegions[i], this));
            }

            survivingRooms.Sort();
            survivingRooms[0].IsMainRoom = true;
            survivingRooms[0].IsAccessibleFromMainRoom = true;

            //UnityEngine.Profiling.Profiler.EndSample();

            //UnityEngine.Profiling.Profiler.BeginSample("Connect Closest Rooms");

            ConnectClosestRooms(survivingRooms, false);

            //UnityEngine.Profiling.Profiler.EndSample();

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
                        var newRegion = GetRegionTiles(x, y);
                        regions.Add(newRegion);

                        for (int i = 0; i < newRegion.Count; i++)
                        {
                            var tile = newRegion[i];
                            mapFlags[tile.x, tile.y] = 1;
                        }
                    }
                }
            }

            //Debug.Log("There are " + regions.Count + " distinct regions.");

            return regions;
        }

        public Map GetFootprintOutline()
        {
            var coords = GetRegionTiles(0, 0);
            var fillValue = this[0, 0];
            var newMap = BlankMap(this).FillWith(fillValue == 0 ? 1 : 0);

            for (int i = 0; i < coords.Count; i++)
            {
                var coord = coords[i];
                newMap[coord.x, coord.y] = fillValue;
            }

            return newMap;
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

                for (int x = tile.x - 1; x <= tile.x + 1; x++)
                {
                    for (int y = tile.y - 1; y <= tile.y + 1; y++)
                    {
                        if (IsInMapRange(x, y) && (y == tile.y || x == tile.x))
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
                            var distanceBetweenRooms = (int)(Mathf.Pow(tileA.x - tileB.x, 2) + Mathf.Pow(tileA.y - tileB.y, 2));

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

        float[,] DrawCircle(Coord c, int r, float value)
        {
            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    if (x * x + y * y <= r * r)
                    {
                        var drawX = c.x + x;
                        var drawY = c.y + y;
                        if (IsInMapRange(drawX, drawY))
                        {
                            _map[drawX, drawY] = value;
                        }
                    }
                }
            }

            return _map;
        }

        float[,] DrawCircle(Coord c, int r)
        {
            return DrawCircle(c, r, 0);
        }

        public Map DrawLine(Coord a, Coord b, int thickness, float value)
        {
            var line = Coord.GetLine(a, b);
            for (int i = 0; i < line.Length; i++)
            {
                DrawCircle(line[i], thickness, value);
            }
            return this;
        }

        public Map DrawLine(Coord a, Coord b, int thickness, float startValue, float endValue)
        {
            var line = Coord.GetLine(a, b);


            for (int i = 0; i < line.Length; i++)
            {
                var point = line[i];
                var distA = Vector2.Distance(a.Vector2, point.Vector2);
                var distB = Vector2.Distance(b.Vector2, point.Vector2);
                var t = Mathf.InverseLerp(0f, distA + distB, distA);

                DrawCircle(line[i], thickness,Mathf.Lerp(startValue,endValue,t ));
            }
            return this;
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

        //Room Class

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
                    for (int x = tile.x - 1; x < tile.x + 1; x++)
                    {
                        for (int y = tile.y - 1; y < tile.y + 1; y++)
                        {
                            if (x == tile.x || y == tile.y)
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

                        var innerX = coordA.x - coordB.x;
                        var innerY = coordA.y - coordB.y;

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
                    centerX += Tiles[i].x;
                    centerY += Tiles[i].y;
                }

                return new Vector3(-SizeX / 2 + .5f + ((float)centerX / Tiles.Count), 50, -SizeY / 2 + .5f + ((float)centerY / Tiles.Count));
            }


        }

        // Heightmap Helper Class

        class HeightmapLerpSegment {

            int _startIndex;
            int _endIndex;

            float _startValue;
            float _endValue;

            public HeightmapLerpSegment(int startIndex, float startValue)
            {

                _startIndex = startIndex;
                _startValue = startValue;
            }

            public void Close(int endIndex, float endValue)
            {

                _endIndex = endIndex;
                _endValue = endValue;



            }

            public float[] Apply(float[] array, AnimationCurve curve)
            {

                var size = _endIndex - _startIndex;

                for (int i = _startIndex; i < _endIndex; i++)
                {


                    if (_startValue == -1 && _endValue == -1)
                    {
                        array[i] = -1;
                    }
                    else if (_startValue == -1 && _endValue != -1)
                    {
                        array[i] = _endValue;
                    }
                    else if (_startValue != -1 && _endValue == -1)
                    {
                        array[i] = _startValue;
                    }
                    else if (_startValue != -1 && _endValue != -1)
                    {
                        array[i] = Mathf.Lerp(_startValue, _endValue, curve.Evaluate((float)(i - _startIndex) / size));
                    }

                }

                return array;
            }


        }
    }
}