using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Nurbz;

namespace MeshMasher {
    public partial class SmartMesh {

        public List<SmartNode> Nodes { get; private set; }
        public List<SmartCell> Cells { get; private set; }
        public List<SmartLine> Lines { get; private set; }

        public SmartMesh(Mesh mesh) : this(mesh.vertices,mesh.triangles)
        {    
        }

        public SmartMesh(Vector3[] vertices, int[] triangles)
        {
            Nodes = new List<SmartNode>();
            Cells = new List<SmartCell>();
            Lines = new List<SmartLine>();

            var verts = vertices;

            for (int i = 0; i < verts.Length; i++)
            {
                Nodes.Add(new SmartNode(verts[i], i));
            }

            var tris = triangles;

            for (var x = 0; x < tris.Length; x += 3)
            {
                var cell = new SmartCell(Nodes[tris[x]], Nodes[tris[x + 1]], Nodes[tris[x + 2]]);
                cell.CreateNodeConnections();
                Cells.Add(cell);
            }

            for (int i = 0; i < Cells.Count; i++)
            {
                Cells[i].CreateCellConnections();
            }

            for (int i = 0; i < Cells.Count; i++)
            {
                Cells[i].CreateLineConnections();
                Lines.AddRange(Cells[i].Lines);
            }

            Lines = Lines.Distinct().ToList();

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
        }

        public MeshState GetMeshState()
        {
            return new MeshState()
            {
                Nodes = new int[Nodes.Count],
                Cells = new int[Cells.Count],
                Lines = new int[Lines.Count],
            };
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

            foreach (var cell in Cells)
            {
                foreach (var neighbour in cell.Neighbours)
                {

                    var scaledVector = neighbour.Center + cell.Center;
                    float scale = 0.5f;
                    scaledVector = new Vector3(scaledVector.x * scale, scaledVector.y * scale, scaledVector.z * scale);

                    Debug.DrawLine(transform.TransformPoint(cell.Center), transform.TransformPoint(scaledVector), Color.red);
                }
            }

            foreach (var line in Lines)
            {
                Debug.DrawLine(transform.TransformPoint(line.Nodes[0].Vert), transform.TransformPoint(line.Nodes[1].Vert), Color.green);
            }

        }

        public MeshState SetCellGroups(MeshState state)
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
            
        public List<List<SmartNode>> GetCellBoundingPolyLines(MeshState state, int key)
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

        public MeshState MinimumSpanningTree()
        {
            var isPartOfCurrentSortingEvent = new bool[Nodes.Count];

            var state = GetMeshState();

            var visitedNodes = new List<SmartNode>();
            var visitedLines = new List<SmartLine>();

            visitedNodes.Add(Nodes[0]);

            while (visitedNodes.Count < Nodes.Count)
            {
                var lines = visitedNodes
                    .SelectMany(n => n.Lines)
                    .Distinct()
                    .Except(visitedLines)
                    .ToList();

                var length = float.MaxValue;
                SmartLine bestLine = null;

                for (int l = 0; l < lines.Count; l++)
                {
                    var line = lines[l];

                    if (line.Length > length)
                        continue;

                    if (isPartOfCurrentSortingEvent[line.Nodes[0].Index] && isPartOfCurrentSortingEvent[line.Nodes[1].Index])
                        continue;

                    length = line.Length;
                    bestLine = line;
                }

                isPartOfCurrentSortingEvent[bestLine.Nodes[0].Index] = true;
                isPartOfCurrentSortingEvent[bestLine.Nodes[1].Index] = true;

                visitedLines.Add(bestLine);
                visitedNodes.AddRange(bestLine.Nodes);
                visitedNodes = visitedNodes.Distinct().ToList();
            }

            for (int i = 0; i < visitedLines.Count; i++)
            {
                state.Lines[visitedLines[i].Index] = 1;
            }

            return state;
        }

        public MeshState CullLeavingOnlySimpleLines(MeshState state)
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

        public MeshState WalkThroughRooms(MeshState state)
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

        public MeshState CalculateRooms(MeshState state)
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

        public MeshState RemoveLargeRooms(MeshState lineStateToUpdate,MeshState roomMap, MeshState walkMap, int maxWalkLength)
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

        public MeshState GeneratedSemiConnectedMesh(int maxCliffLength)
        {
            var state = MinimumSpanningTree();
            var walkState = WalkThroughRooms(state.Clone());
            var roomState = CalculateRooms(state.Clone());
            state = RemoveLargeRooms(state, roomState, walkState, 8);
            return state;
        }

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
    public class MeshState {
        public int[] Nodes;
        public int[] Cells;
        public int[] Lines;

        public MeshState Clone()
        {
            return new MeshState()
            {
                Nodes = (int[])Nodes.Clone(),
                Cells = (int[])Cells.Clone(),
                Lines = (int[])Lines.Clone(),
            };
        }
    }

}  
