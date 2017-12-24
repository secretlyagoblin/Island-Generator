using System.Collections;
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

        MeshMasher.MeshState _roomState;
        float[] _heightMap;

        //public 

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
            var sets = GetAdjacencySets(roomMeshState);
            //DrawRoomCells(roomMeshState);
            //TestAndApplyTrueAdjacencies(roomMeshState, sets);
            

            //DrawRooms(roomMeshState);
            //

            var biggerRooms = ConsolidateSmallRooms(roomMeshState, _settings.MinRoomSize, sets);
            var newSets = GetAdjacencySets(biggerRooms);
            
            //DrawRooms(biggerRooms);
            //DrawRoomOutlines(biggerRooms);
            
            var roads = GenerateRoads();


            var returnValue =  TestAndApplyTrueAdjacencies(biggerRooms,roads, newSets);

            //DrawRoomCells(biggerRooms);
            //DrawPotentialConnections(roomMeshState);

            _roomState = biggerRooms;
            _heightMap = GetHeightmap(biggerRooms, 20);

            return returnValue;
        }

        public List<RegionMesh> Finalise()
        {
            return new List<RegionMesh>();
        }

        public void DisplayDebugGraph()
        {
            //DrawRooms(_roomState);
            DrawRoomCells(_roomState);
            DrawLines(_roomState);
            DrawHeightMap(_roomState,_heightMap,5);
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

        List<int> GenerateRoads()
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

            var connectivity = nodes.Distinct().ToList();

            //for (int u = 0; u < distinct.Length; u++)
            //{
            //    Debug.DrawRay(_mesh.Nodes[distinct[u]].Vert, Vector3.up, Color.blue, 100f);
            //} 

            return connectivity;

            //var outputLines = new List<int>();
            //
            //var connected = new bool[_mesh.Nodes.Count];
            //
            //for (int i = 0; i < connectivity.Length; i++)
            //{
            //    connected[connectivity[i]] = true;
            //}
            //
            //for (int i = 0; i < _mesh.Lines.Count; i++)
            //{
            //    var l = _mesh.Lines[i];
            //    if (connected[l.Nodes[0].Index] == true && connected[l.Nodes[1].Index] == true)
            //    {
            //
            //
            //
            //        l.DrawLine(Color.gray, _settings.DebugDisplayTime, 1f);
            //    }
            //}
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

            //var pairBase = 0;
            
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

        MeshMasher.MeshState ConsolidateSmallRooms(MeshMasher.MeshState rooms, int minRoomSize, Dictionary<KeyValuePair<int, int>, List<int>> adjacencySets)
        {
            var consolidatedRooms = _mesh.GetMeshState();
            var smallRooms = new List<Region>();
            var oldIdDict = new Dictionary<int, Region>();
            var oldRegionDict = new Dictionary<Region, int>();

            //Create map of original regions, determine rooms smaller than min

            foreach (var pair in _regionVertexIndex)
            {
                var r = pair.Key;

                if (oldIdDict.ContainsKey(r.RoomId))
                {

                }
                else
                {
                    oldIdDict.Add(r.RoomId, r);
                    oldRegionDict.Add(r, r.RoomId);
                }


                if (r.RoomSize < minRoomSize)
                {
                    smallRooms.Add(r);
                }
            }

            //iterate small rooms, connecting as required

            while (smallRooms.Count > 0)
            {
                for (int i = 0; i < smallRooms.Count; i++)
                {
                    var r = smallRooms[i];
                    if (r.RoomSize > minRoomSize)
                        continue;

                    var neighs = r.Regions.OrderBy(x => x.RoomSize).ToList();

                    if (neighs.Count == 0)
                    {
                        Debug.Log("No neihbours found at all!");
                        r.RoomSize = minRoomSize;
                        goto roomfound;
                    }

                    var regionId = oldRegionDict[r];

                    for (int u = 0; u < neighs.Count; u++)
                    {
                        var neigh = neighs[u];

                        var neighId = oldRegionDict[neigh];

                        var a = regionId > neighId ? regionId : neighId;
                        var b = regionId > neighId ? neighId : regionId;

                        if (adjacencySets.ContainsKey(new KeyValuePair<int, int>(a, b)))
                        {
                            var neighSize = neigh.RoomSize;
                            neigh.RoomSize += r.RoomSize;
                            r.RoomId = neigh.RoomId;
                            r.RoomSize += neighSize;

                            goto roomfound;
                        }
                        else
                        {

                        }
                    }

                    r.RoomSize = minRoomSize;
                    //Debug.Log("No valid neihbour found");

                    roomfound:
                    continue;

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

        //HeightMap

        float[] GetHeightmap(MeshMasher.MeshState rooms, int iterationCount)
        {
            //could also generate an influence map from here if that mattered


            var heights = new float[_mesh.Nodes.Count];
            var heightsLocked = new bool[_mesh.Nodes.Count];
            var maxHeight = float.MinValue;
            var heightsSet = new bool[_mesh.Nodes.Count];

            var initialPropogationMap = new Queue<MeshMasher.SmartNode>();

            for (int i = 0; i < _regions.Count; i++)
            {
                var h = _regions[i].Height;
                if (h > maxHeight)
                    maxHeight = h;

                var index = _regionVertexIndex[_regions[i]];

                heights[index] = _regions[i].Height;
                //heightsLocked[index] = true;
                heightsSet[index] = true;

                for (int u = 0; u < _mesh.Nodes[index].Nodes.Count; u++)
                {
                    if (heightsSet[_mesh.Nodes[index].Nodes[u].Index])
                        continue;

                    initialPropogationMap.Enqueue(_mesh.Nodes[index].Nodes[u]);
                    heights[_mesh.Nodes[index].Nodes[u].Index] = _regions[i].Height;
                    heightsSet[_mesh.Nodes[index].Nodes[u].Index] = true;
                }
            }

            for (int i = 0; i < _mesh.Nodes.Count; i++)
            {
                if (rooms.Nodes[i] == -1)
                {
                    heightsLocked[i] = true;
                    heightsSet[i] = true;
                }
            }

            //Propogation

            while (initialPropogationMap.Count > 0)
            {
                var c = initialPropogationMap.Dequeue();

                for (int u = 0; u < c.Nodes.Count; u++)
                {
                    if (heightsSet[c.Nodes[u].Index])
                        continue;

                    initialPropogationMap.Enqueue(c.Nodes[u]);
                    heights[c.Nodes[u].Index] = heights[c.Index];
                    heightsSet[c.Nodes[u].Index] = true;
                }
            }

            //Diffusion

            var iterations = iterationCount;

            for (int i = 0; i < iterations; i++)
            {
                var newHeights = new float[_mesh.Nodes.Count];

                for (int u = 0; u < _mesh.Nodes.Count; u++)
                {
                    if (heightsLocked[u] == true)
                    {
                        newHeights[u] = heights[u];
                        continue;
                    }

                    var c = _mesh.Nodes[u];

                    var newHeight = heights[u];
                    var count = 1;

                    var highestValue = newHeight;
                    var lowestValue = newHeight;

                    for (int v = 0; v < c.Nodes.Count; v++)
                    {
                        var n = c.Nodes[v];
                        if (rooms.Nodes[n.Index] == rooms.Nodes[c.Index] || rooms.Lines[c.GetSharedLine(n).Index] == 1)
                        {
                            var sampleHeight = heights[n.Index];
                            newHeight += sampleHeight;

                            if (sampleHeight < lowestValue)
                                lowestValue = sampleHeight;

                            if (sampleHeight > highestValue)
                                highestValue = sampleHeight;

                            count++;
                        }
                    }

                    //Heightmap technique A: Average
                    //newHeights[u] = newHeight * ((float)1 / count);

                    //Heightmap technique B: Middle Point
                    newHeights[u] = (lowestValue + highestValue) / 2;


                }
                heights = newHeights;
            }

            return heights;
        }

            // Final

        bool TestAndApplyTrueAdjacencies(MeshMasher.MeshState rooms, List<int> roads, Dictionary<KeyValuePair<int, int>, List<int>> adjacencySets)
        {

            var testSets = new List<KeyValuePair<int, int>>();
            var invalidRoomConnection = 0;
            var invalidPairs = new List<KeyValuePair<int, int>>();

            //Find Adjacency Sets and cross reference against initial graph

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
                        invalidPairs.Add(pair);
                        //Debug.DrawLine(r.XZPos, n.XZPos, Color.red, _settings.DebugDisplayTime);
                        
                    }
                }
            }

            invalidPairs = invalidPairs.Distinct().ToList();

            //Repair Sets

            if (invalidPairs.Count > 0)
            {

                for (int i = 0; i < _mesh.Nodes.Count; i++)
                {

                    if (rooms.Nodes[i] != -1)
                        continue;

                    if (roads.Contains(i))
                        continue;

                    var n = _mesh.Nodes[i];
                    var codes = new List<int>();
                    var potentialPairs = new List<KeyValuePair<int, int>>();

                    for (int u = 0; u < n.Nodes.Count; u++)
                    {
                        var code = rooms.Nodes[n.Nodes[u].Index];
                        codes.Add(code);

                        for (int v = 0; v < invalidPairs.Count; v++)
                        {
                            if (code == invalidPairs[v].Key)
                                potentialPairs.Add(invalidPairs[v]);
                        }
                    }

                    potentialPairs = potentialPairs.Distinct().ToList();
                    KeyValuePair<int, int> pair = new KeyValuePair<int, int>(-1, -1);
                    var foundPair = false;

                    for (int u = 0; u < potentialPairs.Count; u++)
                    {
                        var pp = potentialPairs[u];


                        for (int v = 0; v < codes.Count; v++)
                        {
                            if (codes[v] == pp.Value)
                            {
                                pair = potentialPairs[u];
                                foundPair = true;
                                break;
                            }

                        }

                        if (!foundPair)
                            continue;

                        Debug.Log("Repaired Failure by adding corridor section");

                        invalidPairs.Remove(pair);
                        invalidRoomConnection -= 2;


                        var keyAssigned = false;
                        var valueAssigned = false;

                        for (int v = 0; v < n.Lines.Count; v++)
                        {
                            var l = n.Lines[v];
                            var otherNode = l.GetOtherNode(n);

                            var code = rooms.Nodes[otherNode.Index];

                            if (!keyAssigned)
                            {
                                if (pair.Key == code)
                                {
                                    rooms.Lines[l.Index] = 1;
                                    //l.DrawLine(Color.gray, _settings.DebugDisplayTime);
                                    //Debug.Log("Actually worked, key");
                                    keyAssigned = true;
                                }
                            }

                            if (!valueAssigned)
                            {
                                if (pair.Value == code)
                                {
                                    rooms.Lines[l.Index] = 1;
                                    //l.DrawLine(Color.gray, _settings.DebugDisplayTime);
                                    //Debug.Log("Actually worked");
                                    valueAssigned = true;
                                }
                            }

                            if (keyAssigned && valueAssigned)
                            {
                                rooms.Nodes[i] = RNG.NextFloat() < 0.5f ? pair.Key : pair.Value;
                                break;
                            }
                                

                        }
                    }

                    if(invalidPairs.Count == 0)
                    {
                        break;
                    }
                }
            }

            // Set connections between sets

            var connected = new bool[_mesh.Nodes.Count];

            for (int i = 0; i < roads.Count; i++)
            {
                connected[roads[i]] = true;
            }

            //for (int i = 0; i < _mesh.Lines.Count; i++)
            //{
            //    var l = _mesh.Lines[i];
            //    if (connected[l.Nodes[0].Index] == true && connected[l.Nodes[1].Index] == true)
            //    {
            //
            //
            //
            //        l.DrawLine(Color.gray, _settings.DebugDisplayTime, 1f);
            //    }
            //}

            //Create roads

            foreach (var item in testSets)
            {

                var set = adjacencySets[item];

                var subset = new List<int>();

                for (int i = 0; i < set.Count; i++)
                {
                    if (connected[_mesh.Lines[set[i]].Nodes[0].Index] == true && connected[_mesh.Lines[set[i]].Nodes[1].Index] == true)
                    {
                        subset.Add(set[i]);
                    }
                }

                if(subset.Count == 0)
                {
                    subset.Add(RNG.NextFromList(set));
                }

                for (int i = 0; i < subset.Count; i++)
                {
                    rooms.Lines[subset[i]] = 1;
                    //_mesh.Lines[subset[i]].DrawLine(Color.gray, _settings.DebugDisplayTime);
                }

                //var l = RNG.NextFromList(set);
                // 
                //rooms.Lines[l] = 1;

                //for (int u = 0; u < set.Count; u++)
                //{
                //    if(rooms.Lines[u] != 1)
                //    {
                //        _mesh.Lines[set[u]].DrawLine(Color.red, _settings.DebugDisplayTime);
                //    }
                //}

                //_regionVertexIndex()

                //var r = item.Key

                

            }

            if (invalidRoomConnection > 0)
            {
                Debug.Log((invalidRoomConnection/2) + " invalid connection(s)");
                return false;
            }

            return true;
        }

        //Debug

        void DrawLines(MeshMasher.MeshState roads)
        {
            for (int i = 0; i < _mesh.Lines.Count; i++)
            {
                if (roads.Lines[i] == 1)
                    _mesh.Lines[i].DrawLine(Color.gray, _settings.DebugDisplayTime);
            }
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
                        l.DrawLine(Color.HSVToRGB(colourHue, 1f, 1f), _settings.DebugDisplayTime);//, colourHue * 200);
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
                    l.DrawLine(Color.red, 100f, _settings.DebugDisplayTime);
                }
            }
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

                    if (rooms.Lines[line.Index] == 1)
                        continue;

                    if (rooms.Nodes[line.Nodes[0].Index] != rooms.Nodes[line.Nodes[1].Index])
                    {
                        Debug.DrawLine(c.Center, c.Neighbours[u].Center, Color.grey, _settings.DebugDisplayTime);
                    }
                }
            }
        }

        void DrawPotentialConnections(MeshMasher.MeshState rooms)
        {
            var lineState = new int[_mesh.Lines.Count];
            for (int i = 0; i < _mesh.Lines.Count; i++)
            {
                var l = _mesh.Lines[i];

                if (rooms.Nodes[l.Nodes[0].Index] != -1 | rooms.Nodes[l.Nodes[1].Index] != -1)
                {
                    lineState[i]++;

                    //var nodeValue = rooms.Nodes[l.Nodes[0].Index] > rooms.Nodes[l.Nodes[1].Index] ? rooms.Nodes[l.Nodes[0].Index] : rooms.Nodes[l.Nodes[1].Index];
                    //var colourHue = Mathf.InverseLerp(0f, 50f, rooms.Nodes[l.Nodes[0].Index]);
                    //l.DrawLine(Color.HSVToRGB(colourHue, 1f, 1f), 100f);//, colourHue * 200);
                }
            }

            var iterationCount = 1;

            for (int it = 1; it <= iterationCount; it++)
            {
                for (int i = 0; i < _mesh.Lines.Count; i++)
                {
                    var l = _mesh.Lines[i];
                    if (lineState[i] == it)
                        continue;

                    var lines = l.CollectConnectedLines();

                    for (int lc = 0; lc < lines.Count; lc++)
                    {
                        if (lineState[lines[lc].Index] == it)
                        {
                            lineState[i] = it + 1;
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < _mesh.Lines.Count; i++)
            {
                var l = _mesh.Lines[i];

                if (lineState[i] > 1)
                {

                    //var nodeValue = rooms.Nodes[l.Nodes[0].Index] > rooms.Nodes[l.Nodes[1].Index] ? rooms.Nodes[l.Nodes[0].Index] : rooms.Nodes[l.Nodes[1].Index];
                    var colourHue = Mathf.InverseLerp(0f, (float)iterationCount, lineState[i]);
                    l.DrawLine(Color.HSVToRGB(colourHue, 1f, 1f), _settings.DebugDisplayTime);//, colourHue * 200);
                }
            }
        }

        void DrawHeightMap(MeshMasher.MeshState rooms, float[] heights, int maxHeight)
        {

            var offset = 0.1f;
            var xOffset = new Vector3(offset, 0, 0);
            var yOffset = new Vector3(0, 0, offset);
            
            for (int i = 0; i < heights.Length; i++)
            {
                if (rooms.Nodes[i] == -1)
                    continue;
            
                var cvalue = Mathf.InverseLerp(0, maxHeight, heights[i]);
                //var color = new Color(cvalue, cvalue, cvalue);
                var color = Color.HSVToRGB(cvalue/1.5f, 1, cvalue);
            
                var pos = _mesh.Nodes[i].Vert;
                pos += (Vector3.up * heights[i]);
                Debug.DrawLine(pos - xOffset, pos + xOffset, color, _settings.DebugDisplayTime);
                Debug.DrawLine(pos - yOffset, pos + yOffset, color, _settings.DebugDisplayTime);
            }
        }
    }

}