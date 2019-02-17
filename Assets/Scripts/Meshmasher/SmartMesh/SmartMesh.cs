using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Nurbz;

namespace MeshMasher {
    public partial class SmartMesh {

        public List<SmartNode> Nodes { get; private set; }
        public List<SmartCell> Cells { get; private set; }
        public List<SmartLine> Lines { get; private set; }

        private Vector3[] _vertices;
        private int[] _triangles;

        Bounds _bounds = new Bounds();
        List<SmartNode>[,,] _buckets;

        #region constructors

        //public SmartMesh(Mesh mesh) : this(mesh.vertices,mesh.triangles)
        //{    
        //}

        public SmartMesh(Vector3[] vertices, int[] triangles)
        {
            Nodes = new List<SmartNode>(vertices.Length);
            Cells = new List<SmartCell>(triangles.Length/3);
            Lines = new List<SmartLine>(triangles.Length);

            _vertices = vertices;

            for (int i = 0; i < _vertices.Length; i++)
            {
                Nodes.Add(new SmartNode(_vertices[i], i));
                _bounds.Encapsulate(_vertices[i]);
            }

            _triangles = triangles;

            var halfEdges = new Dictionary<Coord, SmartLine>();

            for (var x = 0; x < _triangles.Length; x += 3)
            {
                var cell = new SmartCell(Nodes[_triangles[x ]], Nodes[_triangles[x + 1]], Nodes[_triangles[x + 2]]);
                cell.CreateNodeConnections();
                Cells.Add(cell);

                CreateLineConnections(_triangles, halfEdges, cell, x, x + 1);
                CreateLineConnections(_triangles, halfEdges, cell, x+1, x + 2);
                CreateLineConnections(_triangles, halfEdges, cell, x+2, x);

            }

            //Here we iterate through creating lines by creating half-lines and then searching for the inverse in the set
            //So we should be able to close off a decent chunk of this pretty early

            //here first chris





            for (int i = 0; i < Cells.Count; i++)
            {
                Cells[i].CreateCellConnections();
            }

            //for (int i = 0; i < Cells.Count; i++)
            //{
            //    Cells[i].CreateLineConnections();
            //    Lines.AddRange(Cells[i].Lines);
            //}
            //
            //Lines = Lines.Distinct().ToList();
            //
            for (int i = 0; i < Lines.Count; i++)
            {
                Lines[i].Nodes[0].Nodes.Add(Lines[i].Nodes[1]);
                Lines[i].Nodes[1].Nodes.Add(Lines[i].Nodes[0]);
            }

            for (int i = 0; i < Nodes.Count; i++)
                Nodes[i].Index = i;

            for (int i = 0; i < Cells.Count; i++)
                Cells[i].Index = i;

            for (int i = 0; i < Lines.Count; i++)
                Lines[i].Index = i;

            CreateBuckets(1, 1, 1);
        }

        #endregion        

        public MeshState<T> GetMeshState<T>()
        {
            return new MeshState<T>()
            {
                Nodes = new T[Nodes.Count],
                Cells = new T[Cells.Count],
                Lines = new T[Lines.Count],
            };
        }

        public void SetCustomBuckets(int sizeX, int sizeY, int sizeZ)
        {
            CreateBuckets(sizeX, sizeY, sizeZ);
        }

        public int[] TriangleMap()
        {
            var output = new List<int>();

            for (int i = 0; i < Cells.Count; i++)
            {
                output.AddRange(Cells[i].Nodes.Select(n => n.Index).ToList());
            }

            return output.ToArray();
        }

        public Mesh ToXZMesh()
        {
            var m = new Mesh
            {
                vertices = _vertices.Select(x => new Vector3(x.x,0,x.y)).ToArray(),
                triangles = _triangles
            };
            m.RecalculateBounds();
            m.RecalculateNormals();

            return m;
        }

        public Mesh ToXYMesh()
        {
            var m = new Mesh
            {
                vertices = _vertices,
                triangles = _triangles
            };

            m.RecalculateBounds();
            m.RecalculateNormals();

            return m;
        }

        /*

        public SmartMesh()
        {
            Nodes = new List<SmartNode>();
            Nodes.Add(new SmartNode(Vector3.zero, 0));

            Lines = new List<SmartLine>();


            for (int i = 0; i < 2; i++)
            {
                var node = new SmartNode(new Vector3(Random.Range(1f, -1f), 0, Random.Range(1f, -1f)), i);
                Lines.Add(new SmartLine(node, Nodes[0]));
            }

            Nodes[0].UpdateLineSort();

            //var gradient = new Gradient();
            //gradient.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.blue,0f), new GradientColorKey(Color.red, 1f) },new GradientAlphaKey[] { new GradientAlphaKey(1f,0f), new GradientAlphaKey(1f, 1f) });

            //var time = 0f;

            var color = Color.black;

            for (int i = 0; i < Nodes[0].Lines.Count; i++)
            {
                color.r += 0.1f;

                var nodes = Nodes[0].Lines[i].Nodes;
                Debug.DrawLine(nodes[0].Vert, nodes[1].Vert, color, 100f);
                
            }

            var nextLine = Nodes[0].GetNextClockwiseLineSegment(Nodes[0].Lines[1]);
            Debug.DrawLine(Nodes[0].Lines[1].Nodes[0].Vert, Nodes[0].Lines[1].Nodes[1].Vert, Color.green, 100f);
            Debug.DrawLine(nextLine.Nodes[0].Vert, nextLine.Nodes[1].Vert, Color.white, 100f);

        }

    */ //Debug mesh

        public void DrawMesh(Transform transform)
        {
            DrawMesh(transform, Color.red, Color.green);
        }

        public void DrawMesh(Transform transform, Color graphColor, Color inverseGraphColor)
        {
            DrawMesh(transform, graphColor, inverseGraphColor, 0f);
        }

            public void DrawMesh(Transform transform, Color graphColor, Color inverseGraphColor, float time)
        {

            foreach (var cell in Cells)
            {
                foreach (var neighbour in cell.Neighbours)
                {

                    var scaledVector = neighbour.Center + cell.Center;
                    float scale = 0.5f;
                    scaledVector = new Vector3(scaledVector.x * scale, scaledVector.y * scale, scaledVector.z * scale);

                    Debug.DrawLine(transform.TransformPoint(cell.Center), transform.TransformPoint(scaledVector), inverseGraphColor, time);
                }
            }

            foreach (var line in Lines)
            {
                Debug.DrawLine(transform.TransformPoint(line.Nodes[0].Vert), transform.TransformPoint(line.Nodes[1].Vert), graphColor, time);
            }

        }
            
        public List<List<SmartNode>> GetCellBoundingPolyLines(MeshState<int> state, int key)
        {
            var cells = new List<SmartCell>();

            for (int i = 0; i < Cells.Count; i++)
            {
                cells.Add(Cells[i]);
            }

            var isLinePartOfCurrentSortingEvent = new bool[Lines.Count];

            var unsortedLine = new List<SmartLine>();

            foreach (var cell in cells)
            {
                foreach (var line in cell.Lines)
                {
                    var partner = line.GetCellPartner(cell);
                    if (partner == null)
                    {
                        isLinePartOfCurrentSortingEvent[line.Index] = true;
                        unsortedLine.Add(line);
                    }
                    else if (state.Cells[partner.Index] != state.Cells[cell.Index])
                    {
                        unsortedLine.Add(line);
                        isLinePartOfCurrentSortingEvent[line.Index] = true;
                    }
                }
            }

            if (unsortedLine.Count == 0)
                return null;

            var listForResetting = new List<SmartLine>(unsortedLine);
            var completedPolylines = new List<SmartPolyline>();

            while (unsortedLine.Count > 0)
            {
                var curveIsOpen = true;
                var polyline = new SmartPolyline(unsortedLine.Last());
                unsortedLine.RemoveAt(unsortedLine.Count - 1);

                Debug.Log("Starting at node index " + polyline.EndPoint.Index + "...");

                while (curveIsOpen)
                {
                    var nextSection = polyline.EndPoint.GetNextClockwiseLineSegment(polyline.LineSections.Last(), state);

                    Debug.Log("I traverse to index " + polyline.EndPoint.Index + ",");
                    polyline.Intergrate(nextSection);
                    unsortedLine.Remove(nextSection);


                    curveIsOpen = !polyline.isClosed(); // THIS NEEDS TO BE INDEX BASED AND BE ABOUT TWO NODES
                }
                Debug.Log("The loop is closed.");
                completedPolylines.Add(polyline);
            }

            var outputNodeLists = new List<List<SmartNode>>();

            foreach (var poly in completedPolylines)
            {
                outputNodeLists.Add(poly.GetNodeList());
            }

            for (int i = 0; i < listForResetting.Count; i++)
            {
                isLinePartOfCurrentSortingEvent[listForResetting[i].Index] = false;
            }
            return outputNodeLists;
        }

        public static Vector3[] NodeArrayToVector3Array(List<SmartNode> smartNodesList)
        {
            List<Vector3> vectors = new List<Vector3>();
            foreach (var smartNode in smartNodesList)
            {
                vectors.Add(smartNode.Vert);
            }

            return vectors.ToArray();
        }

        public MeshState<int> SetCellGroups(MeshState<int> state)
        {
            var sorted = new bool[Cells.Count];

            var allGroups = new List<List<SmartCell>>();
            var allCells = new List<SmartCell>(Cells);

            while (allCells.Count != 0)
            {
                var cellGroup = new List<SmartCell>();

                var cell = allCells[0];
                sorted[cell.Index] = true;

                cellGroup.Add(cell);
                allCells.Remove(cell);

                var finalCellGroup = new List<SmartCell>();
                finalCellGroup.AddRange(cellGroup);

                var finished = false;

                while (finished == false)
                {
                    var nextCellGroup = new List<SmartCell>();
                    foreach (var c in cellGroup)
                    {
                        foreach (var neigh in c.Neighbours)
                        {
                            if (!sorted[neigh.Index] && state.Cells[cell.Index] == state.Cells[c.Index])
                            {
                                nextCellGroup.Add(neigh);
                                allCells.Remove(neigh);
                                sorted[neigh.Index] = true;
                            }
                        }
                    }

                    if (nextCellGroup.Count == 0)
                        finished = true;

                    finalCellGroup.AddRange(nextCellGroup);
                    cellGroup = new List<SmartCell>(nextCellGroup);
                }

                allGroups.Add(finalCellGroup);
            }

            var count = -1;

            foreach (var group in allGroups)
            {
                count++;
                foreach (var cell in group)
                {
                    state.Cells[cell.Index] = count;
                }
            }

            return state;
        }

        public MeshState<int> MinimumSpanningTree()
        {
            return MinimumSpanningTree(GetMeshState<int>());
        }

        public MeshState<int> MinimumSpanningTree(MeshState<int> state)
        {
            var isPartOfCurrentSortingEvent = new bool[Nodes.Count];

            var nodeCount = 0;

            SmartNode firstNode = null;



            var newState = GetMeshState<int>();

            var visitedNodesList = new List<SmartNode>();
            //var visitedLines = new List<SmartLine>();

            var visitedNodes = (int[])newState.Nodes.Clone();

            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].Lines.Count == 0)
                {
                    nodeCount++;
                    visitedNodes[i] = 1;
                }
                else if (state.Nodes[i] == 1)
                {
                    visitedNodes[i] = 1;
                    nodeCount++;
                }
                else if (firstNode == null)
                {
                    firstNode = Nodes[i];
                }
            }

            var visitedLines = (int[])newState.Lines.Clone();
            var visitedLinesIteration = (int[])newState.Lines.Clone();

            visitedNodesList.Add(firstNode);

            visitedNodes[firstNode.Index] = 1;

            var lines = new List<SmartLine>();
            var iteration = 0;

            while (visitedNodesList.Count < Nodes.Count - nodeCount)
            {
                lines.Clear();
                iteration++;                

                for (int i = 0; i < visitedNodesList.Count; i++)
                {
                    var n = visitedNodesList[i];

                    for (int u = 0; u < n.Lines.Count; u++)
                    {
                        if (visitedLines[n.Lines[u].Index] == 0 && 
                            visitedLinesIteration[n.Lines[u].Index] != iteration &&
                            state.Nodes[n.Lines[u].GetOtherNode(n).Index] != 1)
                        {
                            lines.Add(n.Lines[u]);
                            visitedLinesIteration[n.Lines[u].Index] = iteration;
                        }
                    }
                }                 

                var length = float.MaxValue;
                SmartLine bestLine = null;

                for (int l = 0; l < lines.Count; l++)
                {
                    var line = lines[l];

                    if (line.Length > length)
                        continue;

                    if (isPartOfCurrentSortingEvent[line.Nodes[0].Index] && 
                        isPartOfCurrentSortingEvent[line.Nodes[1].Index])
                        continue;

                    length = line.Length;
                    bestLine = line;
                }

                isPartOfCurrentSortingEvent[bestLine.Nodes[0].Index] = true;
                isPartOfCurrentSortingEvent[bestLine.Nodes[1].Index] = true;

                visitedLines[bestLine.Index] = 1;

                for (int i = 0; i < bestLine.Nodes.Count; i++)
                {
                    var n = bestLine.Nodes[i];
                    if(visitedNodes[n.Index] ==0 )
                    {
                        visitedNodesList.Add(n);
                        visitedNodes[n.Index] = 1;
                    }
                }
            }
            newState.Lines = visitedLines;

            return newState;
        }

        public MeshState<int> CullLeavingOnlySimpleLines(MeshState<int> state)
        {
            var isNodePartOfCurrentSortingEvent = new bool[Nodes.Count];
            var isLinePartOfCurrentSortingEvent = new bool[Lines.Count];

            for (int i = 0; i < Nodes.Count; i++)
            {
                isNodePartOfCurrentSortingEvent[Nodes[i].Index] = false;
            }

            for (int i = 0; i < Lines.Count; i++)
            {
                if(state.Lines[Lines[i].Index] == 0)
                isLinePartOfCurrentSortingEvent[Lines[i].Index] = false;
                else
                {
                    isLinePartOfCurrentSortingEvent[Lines[i].Index] = true;
                    isNodePartOfCurrentSortingEvent[Lines[i].Nodes[0].Index] = true;
                    isNodePartOfCurrentSortingEvent[Lines[i].Nodes[0].Index] = true;
                }
            }

            for (int i = 0; i < Nodes.Count; i++)
            {
                var node = Nodes[i];
                if (!isNodePartOfCurrentSortingEvent[Nodes[i].Index])
                    continue;

                var currentLines = node.Lines.Where(l => isLinePartOfCurrentSortingEvent[l.Index]).OrderBy(x => RNG.NextFloat()).ToList();

                if(currentLines.Count > 2)
                {
                    currentLines.Take(currentLines.Count - 2).ToList().ForEach(x => isLinePartOfCurrentSortingEvent[x.Index] = false);
                }
            }

            for (int i = 0; i < Lines.Count; i++)
            {
                state.Lines[i] = isLinePartOfCurrentSortingEvent[i]?1:0;

            }

            return state;
        }

        public MeshState<int> WalkThroughRooms(MeshState<int> state)
        {
            var hasBeenCounted = new bool[Cells.Count];



            var count = 0;

            //test this first

            while (hasBeenCounted.Where(c => c).Count() != hasBeenCounted.Length)
            {
                var startCell = Cells.FirstOrDefault(c => hasBeenCounted[c.Index] == false);
                var cellsToIterateOver = new List<SmartCell>();

                for (int i = 0; i < Cells.Count; i++)
                {
                    var neihbourCount = 0;
                    var validNeighbour = 0;
                    for (int u = 0; u < Cells[i].Neighbours.Count; u++)
                    {
                        if (state.Lines[Cells[i].GetSharedBorder(Cells[i].Neighbours[u]).Index] == 0 && hasBeenCounted[Cells[i].Neighbours[u].Index] == false)
                        {
                            neihbourCount++;
                            validNeighbour = u;
                        }
                    }

                    if (neihbourCount == 1)
                    {
                        startCell = Cells[i].Neighbours[validNeighbour];
                        goto end;
                    }
                }

                end:

                cellsToIterateOver.Add(startCell);

                while (cellsToIterateOver.Count > 0)
                {
                    var nextList = new List<SmartCell>();

                    for (int i = 0; i < cellsToIterateOver.Count; i++)
                    {
                        var cell = cellsToIterateOver[i];
                        hasBeenCounted[cell.Index] = true;
                        state.Cells[cell.Index] = count;

                        var cells = cell.Neighbours.Where(n => state.Lines[cell.GetSharedBorder(n).Index] == 0 && hasBeenCounted[n.Index] == false).ToList();
                        nextList.AddRange(cells);
                    }
                cellsToIterateOver = nextList.Distinct().ToList();

                    count++;
                }
            }    

            return state;
        }

        public MeshState<int> CalculateRooms(MeshState<int> state)
        {
            var hasBeenCounted = new bool[Cells.Count];

            var count = 0;

            //test this first

            while (hasBeenCounted.Where(c => c).Count() != hasBeenCounted.Length)
            {
                var startCell = Cells.FirstOrDefault(c => hasBeenCounted[c.Index] == false);
                var cellsToIterateOver = new List<SmartCell>();

                for (int i = 0; i < Cells.Count; i++)
                {
                    var neihbourCount = 0;
                    var validNeighbour = 0;
                    for (int u = 0; u < Cells[i].Neighbours.Count; u++)
                    {
                        if (state.Lines[Cells[i].GetSharedBorder(Cells[i].Neighbours[u]).Index] == 0 && hasBeenCounted[Cells[i].Neighbours[u].Index] == false)
                        {
                            neihbourCount++;
                            validNeighbour = u;
                        }
                    }

                    if (neihbourCount == 1)
                    {
                        startCell = Cells[i].Neighbours[validNeighbour];
                        goto end;
                    }
                }

                end:

                cellsToIterateOver.Add(startCell);

                while (cellsToIterateOver.Count > 0)
                {
                    var nextList = new List<SmartCell>();

                    for (int i = 0; i < cellsToIterateOver.Count; i++)
                    {
                        var cell = cellsToIterateOver[i];
                        hasBeenCounted[cell.Index] = true;
                        state.Cells[cell.Index] = count;

                        var cells = cell.Neighbours.Where(n => state.Lines[cell.GetSharedBorder(n).Index] == 0 && hasBeenCounted[n.Index] == false).ToList();
                        nextList.AddRange(cells);
                    }
                    cellsToIterateOver = nextList.Distinct().ToList();
                }

                count++;




            }









            return state;
        }

        public MeshState<int> RemoveLargeRooms(MeshState<int> lineStateToUpdate,MeshState<int> roomMap, MeshState<int> walkMap, int maxWalkLength)
        {
            var distinct = roomMap.Cells.Distinct().ToList();

            for (int i = 0; i < distinct.Count; i++)
            {
                var cells = Cells.Where(c => roomMap.Cells[c.Index] == distinct[i]).ToList();
                var dict = new Dictionary<int, List<SmartCell>>();

                for (int u = 0; u < cells.Count; u++)
                {
                    if (dict.ContainsKey(walkMap.Cells[cells[u].Index]))
                    {
                        dict[walkMap.Cells[cells[u].Index]].Add(cells[u]);
                    }
                    else
                    {
                        dict.Add(walkMap.Cells[cells[u].Index], new List<SmartCell>() { cells[u] });
                    }
                }

                var keys = dict.Keys.ToList();
                keys.Sort();
                var size = keys.Last();

                if (size < maxWalkLength)
                    continue;

                var divisions = Mathf.CeilToInt(size / maxWalkLength);
                var divisionSize = Mathf.CeilToInt(size / divisions);

                for (int u = 0; u < keys.Count; u+= divisionSize)
                {
                    for (int v = 0; v < dict[keys[u]].Count; v++)
                    {
                        var innerCell = dict[keys[u]][v];

                        innerCell.Neighbours.ForEach(n =>
                        {
                            if (roomMap.Cells[n.Index] == roomMap.Cells[innerCell.Index] && walkMap.Cells[n.Index] > walkMap.Cells[innerCell.Index])
                            {
                                lineStateToUpdate.Lines[innerCell.GetSharedBorder(n).Index] = 1;
                            }
                        });
                    }
                }


            }

            return lineStateToUpdate;
        }

        public MeshState<int> GenerateSemiConnectedMesh(int maxCliffLength)
        {
            var state = MinimumSpanningTree();
            var walkState = WalkThroughRooms(state.Clone());
            var roomState = CalculateRooms(state.Clone());
            state = RemoveLargeRooms(state, roomState, walkState, 8);
            return state;
        }

        public MeshState<int> GenerateSemiConnectedMesh(int maxCliffLength, MeshState<int> outline)
        {
            var state = MinimumSpanningTree(outline);
            var walkState = WalkThroughRooms(state.Clone());
            var roomState = CalculateRooms(state.Clone());
            state = RemoveLargeRooms(state, roomState, walkState, 8);
            return state;
        }

        public MeshState<int> ApplyRoomsBasedOnWeights(MeshState<int> state)
        {
            var newState = GetMeshState<int>();
            var diffusionWeight = new int[Nodes.Count];
            var nodeOwnership = new int[Nodes.Count];
            var nodeTraversed = new bool[Nodes.Count];
            var boundaryNodes = new List<SmartNode>();



            var roomIndex = -1;

            for (int i = 0; i < Nodes.Count; i++)
            {
                diffusionWeight[i] = (state.Nodes[i]);
                nodeOwnership[i] = -1;
                if(state.Nodes[i] > 0)
                {
                    boundaryNodes.Add(Nodes[i]);
                    roomIndex++;
                    nodeTraversed[i] = true;
                    nodeOwnership[i] = roomIndex;
                }
            }

            while (boundaryNodes.Count > 0)
            {
                var nextIteration = new List<SmartNode>();
                for (int i = 0; i < boundaryNodes.Count; i++)
                {
                    var b = boundaryNodes[i];
                    var room = nodeOwnership[b.Index];
                    var weight = diffusionWeight[b.Index] -1;

                    for (int u = 0; u < b.Nodes.Count; u++)
                    {
                        var neigh = b.Nodes[u];
                        var n = neigh.Index;
                        if(nodeTraversed[n] == false | diffusionWeight[n] <= diffusionWeight[b.Index] && nodeOwnership[n] !=room )
                        {
                            nodeOwnership[n] = room;
                            nextIteration.Add(neigh);
                            diffusionWeight[n] = weight;
                            nodeTraversed[n] = true;
                        }              

                    }
                }
                boundaryNodes = nextIteration;
            }

            for (int i = 0; i < nodeOwnership.Length; i++)
            {
                nodeOwnership[i] = diffusionWeight[i] <= 0?-1: nodeOwnership[i];
            }

            newState.Nodes = nodeOwnership;

            return newState;



        }

        public MeshState<int> DrawRoomOutlines(MeshState<int> state)
        {
            var newState = GetMeshState<int>();
            var nodeOwnership = new int[Nodes.Count];

            for (int i = 0; i < Nodes.Count; i++)
            {
                nodeOwnership[i] = -1;
            }

            for (int i = 0; i < Nodes.Count; i++)
            {
                for (int u = 0; u < Nodes[i].Nodes.Count; u++)
                {
                    if(state.Nodes[Nodes[i].Index] != state.Nodes[Nodes[i].Nodes[u].Index] && state.Nodes[Nodes[i].Index] > state.Nodes[Nodes[i].Nodes[u].Index])
                    {
                        nodeOwnership[i] = 1;
                    }
                }
            }

            newState.Nodes = nodeOwnership;

            return newState;

        }

        public MeshState<int> SecretSauceRoomsBasedOnWeights(MeshState<int> state)
        {
            var newState = GetMeshState<int>();
            var diffusionWeight = new int[Nodes.Count];
            var nodeOwnership = new int[Nodes.Count];
            var nodeTraversed = new bool[Nodes.Count];
            var boundaryNodes = new List<SmartNode>();

            var roomIndex = -1;

            for (int i = 0; i < Nodes.Count; i++)
            {
                diffusionWeight[i] = (state.Nodes[i]);
                nodeOwnership[i] = -1;
                if (state.Nodes[i] > 0)
                {
                    boundaryNodes.Add(Nodes[i]);
                    roomIndex++;
                    nodeTraversed[i] = true;
                    nodeOwnership[i] = roomIndex;
                }
            }

            while (boundaryNodes.Count > 0)
            {
                var nextIteration = new List<SmartNode>();
                for (int i = 0; i < boundaryNodes.Count; i++)
                {
                    var b = boundaryNodes[i];
                    var room = nodeOwnership[b.Index];
                    var weight = diffusionWeight[b.Index] - 1;

                    for (int u = 0; u < b.Nodes.Count; u++)
                    {
                        var neigh = b.Nodes[u];
                        var n = neigh.Index;
                        if (nodeTraversed[n] == false | diffusionWeight[n] <= diffusionWeight[b.Index] && nodeOwnership[n] != room)
                        {
                            nodeOwnership[n] = room;
                            nextIteration.Add(neigh);
                            diffusionWeight[n] = weight;
                            nodeTraversed[n] = true;
                        }
                    }
                }
                boundaryNodes = nextIteration;
            }

            for (int i = 0; i < nodeOwnership.Length; i++)
            {
                nodeOwnership[i] = diffusionWeight[i] <= 0 ? -1 : nodeOwnership[i];
            }

            newState.Nodes = nodeOwnership;

            return newState;
        }

        public MeshState<int> BubbleMesh(int iterations)
        {
            var newState = GetMeshState<int>();

            for (int u = 0; u < iterations; u++)
            {
                var loop = new List<SmartCell>() { RNG.NextFromList(Cells) };
                //var lines = new List<SmartLine>(loop[0].Lines);
                //var internalLines = new List<SmartLine>();

                var lines = new List<int>();

                newState.Cells[loop[0].Index] = iterations;

                for (int i = 0; i < loop[0].Lines.Count; i++)
                {
                    lines.Add(loop[0].Lines[i].Index);
                    newState.Lines[loop[0].Lines[i].Index]++;

                }

                while (loop.Count < RNG.Next(8,14))
                {
                    var c = RNG.NextFromList(loop);
                    var n = RNG.NextFromList(c.Neighbours);

                    if (newState.Cells[n.Index] >= 1)
                    {

                    }
                    else
                    {
                        newState.Cells[n.Index] = iterations;
                        for (int i = 0; i < n.Lines.Count; i++)
                        {
                            lines.Add(n.Lines[i].Index);
                            newState.Lines[n.Lines[i].Index]++;
                        }
                        loop.Add(n);

                        Debug.DrawLine(c.Center, n.Center, Color.white, 100f);
                    }
                }

                //var currentLine = loop[0].Lines[0];

                for (int i = 0; i < lines.Count; i++)
                {
                    if (newState.Lines[lines[i]] == 1)
                    {
                        Lines[lines[i]].DebugDraw(Color.red, 100f);
                    }
                    else
                    {
                        //Lines[lines[i]].DrawLine(Color.white, 100f);
                    }
                }
            }

            return newState;
        }

        //public MeshState

        public int[] ShortestWalkNode(int startIndex, int endIndex)
        {
            var nodesToEvaluate = new Dictionary<int, aStarHelper>();
            var evaluatedNodes = new Dictionary<int, aStarHelper>();
            var startVert = Nodes[startIndex].Vert;
            var endVert = Nodes[endIndex].Vert;
            var shortCircuit = -1;

            var output = new List<int>();

            nodesToEvaluate.Add(startIndex, new aStarHelper()
            {
                gCost = 0,
                hCost = 0
            });

            while(shortCircuit <= 99999)
            {
                shortCircuit++;

                float shortestDist = float.MaxValue;
                int shortestNode = 0;

                foreach (var node in nodesToEvaluate)
                {
                    if(node.Value.fCost < shortestDist)
                    {
                        shortestNode = node.Key;
                        shortestDist = node.Value.fCost;
                    }
                }

                var currentKey = shortestNode;
                var currentValue = nodesToEvaluate[currentKey];
                var currentNode = Nodes[currentKey];
                nodesToEvaluate.Remove(currentKey);
                evaluatedNodes.Add(currentKey, currentValue);

                if(currentKey == endIndex)
                {
                    var parent = currentKey;
                    while(parent != startIndex)
                    {
                        output.Add(parent);
                        parent= evaluatedNodes[parent].parent;
                    }

                    output.Add(startIndex);
                    return output.ToArray();
                }

                for (int i = 0; i < Nodes[currentKey].Nodes.Count; i++)
                {
                    var neigh = Nodes[currentKey].Nodes[i];
                    if (evaluatedNodes.ContainsKey(neigh.Index))
                        continue;

                    var gCost = currentValue.gCost + Vector3.Distance(currentNode.Vert, neigh.Vert);

                    if (nodesToEvaluate.ContainsKey(neigh.Index))
                    {
                        var star = nodesToEvaluate[neigh.Index];
                        if (gCost > star.gCost)
                            continue;

                        star.gCost = gCost;
                        star.parent = currentKey;
                    }
                    else
                    {
                        nodesToEvaluate.Add(neigh.Index, new aStarHelper()
                        {
                            //can be updated to include weights on lines by searching for actual line weights
                            gCost = gCost,
                            hCost = Vector3.Distance(neigh.Vert, endVert),
                            parent = currentKey
                        });
                    }


                }

            }

            Debug.Log("Short Circuit, Lad");

            return new int[0];



        }

        public void Resize(Bounds bounds)
        {
            var oldMin = _bounds.min;
            var oldMax = _bounds.max;
            var newMin = bounds.min;
            var newMax = bounds.max;

            for (int i = 0; i < Nodes.Count; i++)
            {
                var n = Nodes[i].Vert;

                var x = Mathf.InverseLerp(oldMin.x, oldMax.x, n.x);
                var y = Mathf.InverseLerp(oldMin.y, oldMax.y, n.y);
                var z = Mathf.InverseLerp(oldMin.z, oldMax.z, n.z);

                x = Mathf.Lerp(newMin.x, newMax.x, x);
                y = Mathf.Lerp(newMin.y, newMax.y, y);
                z = Mathf.Lerp(newMin.z, newMax.z, z);

                Nodes[i].Vert = new Vector3(x, y, z);
            }

            for (int i = 0; i < Cells.Count; i++)
            {
                Cells[i].Resize();
            }



            _bounds = bounds;
        }

        public int ClosestIndex(Vector3 testPoint)
        {
            if (!_bounds.Contains(testPoint))
            {
                testPoint = _bounds.ClosestPoint(testPoint);
            }

            var min = _bounds.min;
            var max = _bounds.max;

            var lengthX = _buckets.GetLength(0);
            var lengthY = _buckets.GetLength(1);
            var lengthZ = _buckets.GetLength(2);

            var pointX = Mathf.FloorToInt(Mathf.InverseLerp(min.x, max.x, testPoint.x) * (lengthX-1));
            var pointY = Mathf.FloorToInt(Mathf.InverseLerp(min.y, max.y, testPoint.y) * (lengthY-1));
            var pointZ = Mathf.FloorToInt(Mathf.InverseLerp(min.z, max.z, testPoint.z) * (lengthZ-1));

            var minDist = float.MaxValue;
            SmartNode node = null;

            for (int x = pointX; x < Mathf.Min(lengthX,pointX+2); x++)
            {
                for (int y = pointY; y < Mathf.Min(lengthY, pointY + 2); y++)
                {
                    for (int z = pointZ; z < Mathf.Min(lengthZ, pointZ + 2); z++)
                    {
                        for (int i = 0; i < _buckets[x,y,z].Count; i++)
                        {
                            var dist = Vector3.Distance(testPoint, _buckets[x, y, z][i].Vert);
                            if(dist < minDist)
                            {
                                minDist = dist;
                                node = _buckets[x, y, z][i];
                            }
                        }
                    }
                }
            }

            if (node == null)
            {
                Debug.Log("I'm in hell");
            }

            //Debug.DrawLine(testPoint, node.Vert, Color.green, 100f);

            return node.Index;
        }

        public void CreateLineConnections(int[] tris,Dictionary<Coord, SmartLine> halfEdges, SmartCell cell, int a, int b)
        {
            var halfEdge = new Coord(tris[a], tris[b]);
            var inverseHalfEdge = new Coord(halfEdge.y, halfEdge.x);

            if (halfEdges.ContainsKey(inverseHalfEdge))
            {
                var line = halfEdges[inverseHalfEdge];
                line.AddNeighbour(cell);
                cell.AddLine(line);

                halfEdges.Remove(inverseHalfEdge);
            }
            else
            {
                var line = new SmartLine(Nodes[tris[a]], Nodes[tris[b]]);
                cell.AddLine(line);
                Lines.Add(line);
                line.AddNeighbour(cell);

                halfEdges.Add(halfEdge, line);
            }
        }

        public MeshState<int> GetBorderNodes()
        {
            var state = GetMeshState<int>();

            for (int i = 0; i < Lines.Count; i++)
            {
                var l = Lines[i];

                if(l.Neighbours.Count != 2)
                {
                    state.Lines[i] = 1;
                    state.Nodes[l.Nodes[0].Index] = 1;
                    state.Nodes[l.Nodes[1].Index] = 1;
                }
                else
                {
                    state.Lines[i] = 0;
                }                
            }

            return state;
        }

        void CreateBuckets(int divX, int divY, int divZ)
        {
            _buckets = new List<SmartNode>[divX, divY, divZ];

            for (int x = 0; x < divX; x++)
            {
                for (int y = 0; y < divY; y++)
                {
                    for (int z = 0; z < divZ; z++)
                    {
                        _buckets[x, y, z] = new List<SmartNode>();
                    }
                }
            }



            var min = _bounds.min;
            var max = _bounds.max;

            for (int i = 0; i < Nodes.Count; i++)
            {
                var n = Nodes[i].Vert;

                var x = Mathf.InverseLerp(min.x, max.x, n.x) * (divX-1);
                var y = Mathf.InverseLerp(min.y, max.y, n.y) * (divY-1);
                var z = Mathf.InverseLerp(min.z, max.z, n.z) * (divZ-1);

                var intX = Mathf.RoundToInt(x);
                var intY = Mathf.RoundToInt(y);
                var intZ = Mathf.RoundToInt(z);


                    _buckets[intX, intY, intZ].Add(Nodes[i]);


                
            }
        }

        internal class aStarHelper {
            public float gCost;
            public float hCost;
            public float fCost { get { return gCost + hCost; } }
            public int parent;
        }

    }

    /// <summary>
    /// Smartmesh functions operate on the current mesh using MeshStates to hold the current state of the mesh. 
    /// This allows the current mesh to be analyised for various conditions.
    /// </summary>
    public class MeshState<T> {
        public T[] Nodes;
        public T[] Cells;
        public T[] Lines;

        public MeshState<T> Clone()
        {
            return new MeshState<T>()
            {
                Nodes = (T[])Nodes.Clone(),
                Cells = (T[])Cells.Clone(),
                Lines = (T[])Lines.Clone(),
            };
        }



    }
}  
