using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Nurbz;

namespace Meshmasher
{
    public class SmartMesh
    {

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

        public List<List<SmartCell>> GetCellGroups()
        {
            foreach (var cell in Cells)
            {
                cell.Unsorted = true;
            }

            var allGroups = new List<List<SmartCell>>();
            var allCells = new List<SmartCell>(Cells);

            while (allCells.Count != 0)
            {

                var cellGroup = new List<SmartCell>();

                var cell = allCells[0];
                cell.Unsorted = false;

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
                            if (neigh.Unsorted && neigh.State == c.State)
                            {
                                nextCellGroup.Add(neigh);
                                allCells.Remove(neigh);
                                neigh.Unsorted = false;
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
                    cell.OverrideCurrentRoomSetting(count);
                }
            }


                    return allGroups;
        }

        //Take 2
        /*
        public List<List<SmartNode>> GetPolyLines(List<SmartCell> cells)
        {
            var unsortedLine = new List<SmartLine>();

            // getting all the lines that are on the boundary

            Debug.Log("I am currently finding the outline of a set of " + cells.Count + " connected cells");

            foreach (var cell in cells)
            {
                foreach (var line in cell.Lines)
                {
                    var partner = line.GetCellPartner(cell);
                    if (partner == null)
                    {
                        line.SetAsPartOfPolylineSortingEvent();
                        unsortedLine.Add(line);
                    }
                    else if (partner.Room != cell.Room)
                    {
                        unsortedLine.Add(line);
                        line.SetAsPartOfPolylineSortingEvent();
                    }
                }
            }

            Debug.Log("This has been simplified down to " + unsortedLine.Count + " lines");

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
                    var nextSection = polyline.EndPoint.GetNextClockwiseLineSegment(polyline.LineSections.Last());
                    Debug.Log("I traverse to index " + polyline.EndPoint.Index + ",");
                    polyline.intergrate(nextSection);
                    unsortedLine.Remove(nextSection);
                    curveIsOpen = !polyline.isClosed(); // THIS NEEDS TO BE INDEX BASED AND BE ABOUT TWO NODES
                }
                Debug.Log("The loop is closed.");
                completedPolylines.Add(polyline);
            }

            var outputNodeLists = new List<List<SmartNode>>();

            foreach (var poly in completedPolylines)
            {
                outputNodeLists.Add(poly.getNodeList());
            }

            for (int i = 0; i < listForResetting.Count; i++)
            {
                listForResetting[i].RemoveFromSortingEvent();
            }
            return outputNodeLists;
        }
        */

        public List<List<SmartNode>> GetPolyLines(List<SmartCell> cells)
        {
            var unsortedLine = new List<SmartLine>();

            foreach (var cell in cells)
            {
                foreach (var line in cell.Lines)
                {
                    var partner = line.GetCellPartner(cell);
                    if (partner == null)
                    {
                        line.SetAsPartOfPolylineSortingEvent();
                        unsortedLine.Add(line);
                    }
                    else if (partner.Room != cell.Room)
                    {
                        unsortedLine.Add(line);
                        line.SetAsPartOfPolylineSortingEvent();
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
                    var nextSection = polyline.EndPoint.GetNextClockwiseLineSegment(polyline.LineSections.Last());

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
                outputNodeLists.Add(poly.getNodeList());
            }

            for (int i = 0; i < listForResetting.Count; i++)
            {
                listForResetting[i].RemoveFromSortingEvent();
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
    }  
}