using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphTransformers
{
    public static partial class MeshStateBehaviours
    {
        /*
        public static MeshState<int> SetCellGroups(MeshState<int> state)
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
            return MinimumSpanningTree(CreateMeshState<bool>());
        }

        public MeshState<int> MinimumSpanningTree(MeshState<bool> state)
        {
            var isPartOfCurrentSortingEvent = new bool[Nodes.Count];
            var nodeCount = 0;
            var newState = CreateMeshState<int>();
            var visitedNodesList = new List<SmartNode>();
            var visitedNodes = (int[])newState.Nodes.Clone();

            var linesToVisit = new List<int>();

            for (int i = 0; i < state.Lines.Length; i++)
            {
                if (state.Lines[i])
                {
                    linesToVisit.Add(i);
                }
            }

            SmartNode firstNode = null;

            for (int i = 0; i < linesToVisit.Count; i++)
            {
                if (Nodes[i].Lines.Count == 0)
                {
                    nodeCount++;
                    visitedNodes[i] = 1;
                }
                else if (
                    Nodes[i].Lines.ConvertAll(x => (state.Lines[x.Index] == true) ? 0 : 1).Sum() == 0)
                {
                    nodeCount++;
                    visitedNodes[i] = 1;
                }
                else if (state.Nodes[i])
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

            for (int i = 0; i < state.Lines.Length; i++)
            {
                if (state.Lines[i])
                {
                    visitedLines[i] = 1;
                }
            }

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
                            state.Nodes[n.Lines[u].GetOtherNode(n).Index] == false)
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
                try
                {
                    isPartOfCurrentSortingEvent[bestLine.Nodes[0].Index] = true;
                }
                catch
                {
                    Debug.Log("Whey");
                }

                isPartOfCurrentSortingEvent[bestLine.Nodes[1].Index] = true;
                visitedLines[bestLine.Index] = 1;

                for (int i = 0; i < bestLine.Nodes.Count; i++)
                {
                    var n = bestLine.Nodes[i];
                    if (visitedNodes[n.Index] == 0)
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
                if (state.Lines[Lines[i].Index] == 0)
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

                if (currentLines.Count > 2)
                {
                    currentLines.Take(currentLines.Count - 2).ToList().ForEach(x => isLinePartOfCurrentSortingEvent[x.Index] = false);
                }
            }

            for (int i = 0; i < Lines.Count; i++)
            {
                state.Lines[i] = isLinePartOfCurrentSortingEvent[i] ? 1 : 0;

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

        public MeshState<int> RemoveLargeRooms(MeshState<int> lineStateToUpdate, MeshState<int> roomMap, MeshState<int> walkMap, int maxWalkLength)
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

                for (int u = 0; u < keys.Count; u += divisionSize)
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

        public MeshState<int> GenerateSemiConnectedMesh(int maxCliffLength, MeshState<bool> outline)
        {
            var state = MinimumSpanningTree(outline);
            var walkState = WalkThroughRooms(state.Clone());
            var roomState = CalculateRooms(state.Clone());
            state = RemoveLargeRooms(state, roomState, walkState, 8);
            return state;
        }

        public MeshState<int> ApplyRoomsBasedOnWeights(MeshState<int> state)
        {
            var newState = CreateMeshState<int>();
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

        public MeshState<int> DrawRoomOutlines(MeshState<int> state)
        {
            var newState = CreateMeshState<int>();
            var nodeOwnership = new int[Nodes.Count];

            for (int i = 0; i < Nodes.Count; i++)
            {
                nodeOwnership[i] = -1;
            }

            for (int i = 0; i < Nodes.Count; i++)
            {
                for (int u = 0; u < Nodes[i].Nodes.Count; u++)
                {
                    if (state.Nodes[Nodes[i].Index] != state.Nodes[Nodes[i].Nodes[u].Index] && state.Nodes[Nodes[i].Index] > state.Nodes[Nodes[i].Nodes[u].Index])
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
            var newState = CreateMeshState<int>();
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
            var newState = CreateMeshState<int>();

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

                while (loop.Count < RNG.Next(8, 14))
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

        
        public MeshState<bool> GetBorderNodes()
        {
            var state = CreateMeshState<bool>();

            for (int i = 0; i < Lines.Count; i++)
            {
                var l = Lines[i];

                if(l.Neighbours.Count != 2)
                {
                    state.Lines[i] = true;
                    state.Nodes[l.Nodes[0].Index] = true;
                    state.Nodes[l.Nodes[1].Index] = true;
                }
                else
                {
                    state.Lines[i] = false;
                }                
            }

            return state;
        }

                public void DrawRoads(Transform transform, MeshState<int> connections)
        {
            DrawRoads(transform, connections, Color.red, 0f);
        }

        public void DrawRoads(Transform transform, MeshState<int> connections, Color graphColor, float time)
        {
            foreach (var line in Lines)
            {
                if(connections.Lines[line.Index] == 1)
                {
                    Debug.DrawLine(transform.TransformPoint(line.Nodes[0].Vert), transform.TransformPoint(line.Nodes[1].Vert), graphColor, time);
                }                
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


        */
    }
}