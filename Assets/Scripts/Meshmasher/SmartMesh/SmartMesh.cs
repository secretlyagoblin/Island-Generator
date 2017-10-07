using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Nurbz;

namespace MeshMasher {
    public partial class SmartMesh {

        public List<SmartNode> Nodes { get; private set; }
        public List<SmartCell> Cells { get; private set; }
        public List<SmartLine> Lines { get; private set; }

        public SmartMesh(Mesh mesh)
        {

            Nodes = new List<SmartNode>();
            Cells = new List<SmartCell>();
            Lines = new List<SmartLine>();

            var verts = mesh.vertices;

            var count = -1;

            foreach (var vert in verts)
            {
                count++;
                Nodes.Add(new SmartNode(vert, count));
            }

            var tris = mesh.triangles;

            for (var x = 0; x < tris.Length; x += 3)
            {
                var cell = new SmartCell(Nodes[tris[x]], Nodes[tris[x + 1]], Nodes[tris[x + 2]]);
                cell.CreateNodeConnections();
                Cells.Add(cell);
            }

            foreach (var cell in Cells)
            {
                cell.CreateCellConnections();
            }

            foreach (var cell in Cells)
            {
                cell.CreateLineConnections();
                Lines.AddRange(cell.Lines);
            }

            Lines = Lines.Distinct().ToList();

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

        /*
        public List<SmartLine> InvertLineSelection(List<SmartLine> lines)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].IsPartOfCurrentSortingEvent = true;
            }
            for (int i = 0; i < Lines.Count; i++)
            {
                Lines[i].IsPartOfCurrentSortingEvent = false;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].IsPartOfCurrentSortingEvent = false;
                lines[i].Nodes[0].IsPartOfCurrentSortingEvent = false;
                lines[i].Nodes[1].IsPartOfCurrentSortingEvent = false;
            }

            for (int i = 0; i < Nodes.Count; i++)
            {
                var node = Nodes[i];
                if (!node.IsPartOfCurrentSortingEvent)
                    continue;

                node.Lines.Where(l => l.Nodes[0].IsPartOfCurrentSortingEvent && l.Nodes[1].IsPartOfCurrentSortingEvent).ToList().ForEach(x => x.IsPartOfCurrentSortingEvent = true);

            }

            return Lines.Where(l => l.IsPartOfCurrentSortingEvent).ToList();
        }
        */
    }

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
