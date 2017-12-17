﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldGen;
using System.Linq;

namespace WorldGen {

    public class WorldMesh {

        Transform _transform;
        Bounds _bounds;
        List<Region> _regions;
        Dictionary<Region, int> _regionVertexIndex = new Dictionary<Region, int>();
        MeshMasher.SmartMesh _mesh;
        WorldMeshSettings _settings;

        public WorldMesh(Transform root, List<Region> regions, WorldMeshSettings settings )
        {
            _regions = regions;
            _transform = root;
            _settings = settings;
            ReorientToXZPlaneAndUpdateBounds();
        }

        public bool Generate()
        {
            CreateSmartMesh();
            CalculateClosestNodes();

            var roomMeshState = GenerateRooms();

            SetRegionRoomSize(roomMeshState);
            //var sets = GetAdjacencySets(roomMeshState);
            //DrawRoomCells(roomMeshState);
            //DetermineTrueAdjacencies(roomMeshState, sets);
            

            //DrawRooms(roomMeshState);
            //

            var biggerRooms = ConsolidateSmallRooms(roomMeshState, _settings.MinRoomSize);
            var newSets = GetAdjacencySets(biggerRooms);
            
            //DrawRooms(biggerRooms);
            //DrawRoomOutlines(biggerRooms);
            DrawRoomCells(biggerRooms);
            //GenerateRoads();

            return TestTrueAdjacencies(biggerRooms, newSets);
        }

        //Init

        void ReorientToXZPlaneAndUpdateBounds()
        {
            _bounds = new Bounds(_regions[0].XZPos, Vector3.zero);

            for (int i = 0; i < _regions.Count; i++)
            {
                _bounds.Encapsulate(_regions[i].XZPos);
            }

            _bounds.size = _bounds.size.x > _bounds.size.z ? new Vector3(_bounds.size.x, 0, _bounds.size.x) : new Vector3(_bounds.size.z, 0, _bounds.size.z);
            _bounds.size += (Vector3.one * _bounds.size.magnitude * _settings.BoundsOffsetPercentage); // I don't think this does anything
        }

        //Generate

        void CreateSmartMesh()
        {
            _mesh = MeshMasher.DelaunayGen.FromBounds(_bounds, _settings.DelaunayBoundsRatio);
            // _mesh.DrawMesh(_transform);
            _mesh.SetCustomBuckets(10, 1, 10);
        }

        void CalculateClosestNodes()
        {
            for (int i = 0; i < _regions.Count; i++)
            {
                _regionVertexIndex.Add(_regions[i], _mesh.ClosestIndex(_regions[i].XZPos));
            }
        }

        // Roads

        void GenerateRoads()
        {

            var nodes = new List<int>();

            for (int i = 0; i < _regions.Count; i++)
            {
                var r = _regions[i];
                var a = _regionVertexIndex[r];

                for (int u = 0; u < r.Regions.Count; u++)
                {
                    var b = _regionVertexIndex[r.Regions[u]];
                    var realNodes = _mesh.ShortestWalkNode(a, b);
                    nodes.AddRange(realNodes);
                }
            }

            var connectivity = nodes.Distinct().ToArray();

            //for (int u = 0; u < distinct.Length; u++)
            //{
            //    Debug.DrawRay(_mesh.Nodes[distinct[u]].Vert, Vector3.up, Color.blue, 100f);
            //} 

            var connected = new bool[_mesh.Nodes.Count];

            for (int i = 0; i < connectivity.Length; i++)
            {
                connected[connectivity[i]] = true;
            }

            for (int i = 0; i < _mesh.Lines.Count; i++)
            {
                var l = _mesh.Lines[i];
                if (connected[l.Nodes[0].Index] == true && connected[l.Nodes[1].Index] == true)
                {
                    l.DrawLine(Color.gray, 100f, 0.2f);
                }
            }
        }

        //Rooms

        MeshMasher.MeshState GenerateRooms()
        {
            var state = _mesh.GetMeshState();

            foreach (var pair in _regionVertexIndex)
            {

                state.Nodes[pair.Value] = RNG.Next(_settings.MinRoomDiffusion, _settings.MaxRoomDiffusion);
            }

            var returnState = _mesh.ApplyRoomsBasedOnWeights(state);

            return returnState;
        }

        void DrawRooms(MeshMasher.MeshState rooms)
        {
            for (int i = 0; i < _mesh.Lines.Count; i++)
            {
                var l = _mesh.Lines[i];

                if (rooms.Nodes[l.Nodes[0].Index] != -1 && rooms.Nodes[l.Nodes[1].Index] != -1)
                {

                    if (rooms.Nodes[l.Nodes[0].Index] == rooms.Nodes[l.Nodes[1].Index])
                    {
                        var colourHue = Mathf.InverseLerp(0f, 50f, rooms.Nodes[l.Nodes[0].Index]);
                        l.DrawLine(Color.HSVToRGB(colourHue, 1f, 1f), 100f);//, colourHue * 200);
                        //l.DrawLine(Color.white, 100f);//, colourHue * 200);
                    }

                    //var nodeValue = rooms.Nodes[l.Nodes[0].Index] > rooms.Nodes[l.Nodes[1].Index] ? rooms.Nodes[l.Nodes[0].Index] : rooms.Nodes[l.Nodes[1].Index];
                    //var colourHue = Mathf.InverseLerp(0f, 50f, rooms.Nodes[l.Nodes[0].Index]);
                    //l.DrawLine(Color.HSVToRGB(colourHue, 1f, 1f), 100f);//, colourHue * 200);

                }
            }
        }

        void DrawRoomOutlines(MeshMasher.MeshState rooms)
        {
            var boundaries = _mesh.DrawRoomOutlines(rooms);

            for (int i = 0; i < _mesh.Lines.Count; i++)
            {
                var l = _mesh.Lines[i];
                if (boundaries.Nodes[l.Nodes[0].Index] == 1 && boundaries.Nodes[l.Nodes[1].Index] == 1)
                {
                    l.DrawLine(Color.red, 100f, 0.1f);
                }
            }
        }

        Dictionary<KeyValuePair<int, int>, List<int>> GetAdjacencySets(MeshMasher.MeshState rooms)
        {
            //var boundaries = _mesh.DrawRoomOutlines(rooms);

            var relationshipBags = new Dictionary<KeyValuePair<int,int>, List<int>>();
            var pairCount = 0;

            for (int i = 0; i < _mesh.Lines.Count; i++)
            {
                var l = _mesh.Lines[i];
                var a = rooms.Nodes[l.Nodes[0].Index];
                var b = rooms.Nodes[l.Nodes[1].Index];

                if (a == -1 | b == -1)
                    continue;

                if (a == b)
                    continue;

                var pair = a > b ? new KeyValuePair<int, int>(a, b) : new KeyValuePair<int, int>(b, a);

                if (relationshipBags.ContainsKey(pair))
                {
                    relationshipBags[pair].Add(l.Index);
                }
                else
                {
                    relationshipBags.Add(pair, new List<int>() { l.Index });
                    pairCount++;
                }
            }

            var pairBase = 0;
            
            //foreach (var r in relationshipBags)
            //{
            //    var color = Color.HSVToRGB(Mathf.InverseLerp(0, pairCount, pairBase),1,1);
            //    color = RNG.GetRandomColor();
            //
            //    foreach (var l in r.Value)
            //    {
            //        _mesh.Lines[l].DrawLine(color, 100f);
            //    }
            //
            //    pairBase++;
            //}

            return relationshipBags;


        }

        void DrawRoomCells(MeshMasher.MeshState rooms)
        {
            //var boundaries = _mesh.DrawRoomOutlines(rooms);

            for (int i = 0; i < _mesh.Cells.Count; i++)
            {
                var c = _mesh.Cells[i];

                for (int u = 0; u < c.Neighbours.Count; u++)
                {
                    var line = c.GetSharedBorder(c.Neighbours[u]);
                    if(rooms.Nodes[line.Nodes[0].Index] != rooms.Nodes[line.Nodes[1].Index])
                    {
                        Debug.DrawLine(c.Center, c.Neighbours[u].Center, Color.grey, _settings.DebugDisplayTime);
                    }
                }
            }
        }

        void SetRegionRoomSize(MeshMasher.MeshState rooms)
        {
            var lengthDict = new Dictionary<int, int>();

            for (int i = 0; i < rooms.Nodes.Length; i++)
            {
                var code = rooms.Nodes[i];
                if (lengthDict.ContainsKey(code))
                {
                    var value = lengthDict[code];
                    value++;
                    lengthDict[code] = value;
                }
                else
                {
                    lengthDict.Add(code, 1);
                }
            }

            

            foreach (var pair in _regionVertexIndex)
            {
                var r = pair.Key;

                r.RoomId = rooms.Nodes[pair.Value];
                r.RoomSize = lengthDict[r.RoomId];
            }
        }

        bool TestTrueAdjacencies(MeshMasher.MeshState rooms, Dictionary<KeyValuePair<int, int>, List<int>> adjacencySets)
        {


            var testSets = new List<KeyValuePair<int, int>>();
            var invalidRoomConnection = 0;


            for (int i = 0; i < _regions.Count; i++)
            {
                var r = _regions[i];

                
                for (int u = 0; u < r.Regions.Count; u++)
                {
                    var n = r.Regions[u];

                    if (r.RoomId == n.RoomId)
                        continue;

                    var pair = r.RoomId > n.RoomId ? new KeyValuePair<int, int>(r.RoomId, n.RoomId) : new KeyValuePair<int, int>(n.RoomId, r.RoomId);

                    if (adjacencySets.ContainsKey(pair))
                    {

                        if (testSets.Contains(pair))
                        {

                        }
                        else
                        {
                            testSets.Add(pair);
                        }
                    }
                    else
                    {
                        invalidRoomConnection++;
                        Debug.DrawLine(r.XZPos, n.XZPos, Color.red, _settings.DebugDisplayTime);
                        
                    }
                }
            }

            foreach (var item in testSets)
            {

                var set = adjacencySets[item];
                var l = RNG.NextFromList(set);

                //_regionVertexIndex()

                //var r = item.Key

                //_mesh.Lines[l].DrawLine(Color.white, 100f);

            }

            if (invalidRoomConnection > 0)
            {
                Debug.Log(invalidRoomConnection + " invalid connection(s)");
                return false;
            }

            return true;
        }

        MeshMasher.MeshState ConsolidateSmallRooms(MeshMasher.MeshState rooms, int minRoomSize)
        {
            var consolidatedRooms = _mesh.GetMeshState();
            var smallRooms = new List<Region>();
            var oldIdDict = new Dictionary<int, Region>();

            foreach (var pair in _regionVertexIndex)
            {
                var r = pair.Key;


                if (oldIdDict.ContainsKey(r.RoomId))
                {

                }
                else
                {
                    oldIdDict.Add(r.RoomId, r);
                }


                if (r.RoomSize < minRoomSize)
                {
                    smallRooms.Add(r);
                }
            }

            while (smallRooms.Count > 0)
            {
                for (int i = 0; i < smallRooms.Count; i++)
                {
                    var r = smallRooms[i];
                    if (r.RoomSize > minRoomSize)
                        continue;



                    var neigh = r.Regions.OrderBy(x => x.RoomSize).LastOrDefault();
                    if(neigh == null)
                    {
                        r.RoomSize = minRoomSize;
                    }
                    else
                    {
                        var neighSize = neigh.RoomSize;
                        neigh.RoomSize += r.RoomSize;
                        r.RoomId = neigh.RoomId;
                        r.RoomSize += neighSize;
                    }

                }

                smallRooms.Clear();

                for (int i = 0; i < _regions.Count; i++)
                {
                    var r = _regions[i];
                    if (r.RoomSize < minRoomSize)
                    {
                        smallRooms.Add(r);
                    }
                }
            }

            for (int i = 0; i < rooms.Nodes.Length; i++)
            {
                var roomCode = rooms.Nodes[i];

                if (oldIdDict.ContainsKey(roomCode))
                {
                    consolidatedRooms.Nodes[i] = oldIdDict[roomCode].RoomId;
                }
                else
                {
                    consolidatedRooms.Nodes[i] = roomCode;
                }



            }

            return consolidatedRooms;

        }

    }

}