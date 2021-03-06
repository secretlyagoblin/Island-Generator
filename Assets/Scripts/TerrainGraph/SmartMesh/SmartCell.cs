﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace WanderingRoad.Procgen.Meshes
{
    public class SmartCell {

        public Vector3 Center
        { get; private set; }
        public List<SmartNode> Nodes
        { get; private set; }
        public List<SmartCell> Neighbours
        { get; private set; }
        public List<SmartLine> Lines
        { get; private set; }
        public int Index
        { get; set; }

        public float[] BarycentricWeights { get; private set; }

        Vector3[] _verts;

        public SmartCell(SmartNode nodeA, SmartNode nodeB, SmartNode nodeC)
        {

            Nodes = new List<SmartNode>(3) { nodeA, nodeB, nodeC };
            Neighbours = new List<SmartCell>(3);
            Lines = new List<SmartLine>(3);

            Center = nodeA.Vert + nodeB.Vert + nodeC.Vert;
            float scale = 1f / 3f;
            Center = new Vector3(Center.x * scale, Center.y * scale, Center.z * scale);

            CalculateBarycentricWeights();
        }

        public void CreateNodeConnections()
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].AddCell(this);
            }
        }

        public void CreateCellConnections()
        {
            //var nodes = new List<SmartNode>();

            //for (int i = 0; i < Nodes.Count; i++)
            //{
            //    var node = Nodes[i];
            //
            //    for (int u = 0; u < node.Cells.Count; u++)
            //    {
            //        var neighbourCell = node.Cells[u];
            //
            //        var doubleCount = 0;
            //
            //        for (int v = 0; v < neighbourCell.Nodes.Count; v++)
            //        {
            //            if (neighbourCell.Nodes[v] == node)
            //                continue;
            //
            //            for (int e = 0; e < Nodes.Count; e++)
            //            {
            //                if (Nodes[e] == neighbourCell.Nodes[v])
            //                    doubleCount++;
            //            }
            //        }
            //
            //        
            //    }
            //}

            for (int i = 0; i < Lines.Count; i++)
            {
                var line = Lines[i];

                if (line.Neighbours.Count != 2)
                    continue;

                if (line.Neighbours[0] == this)
                    Neighbours.Add(line.Neighbours[1]);
                else if (line.Neighbours[1] == this)
                    Neighbours.Add(line.Neighbours[0]);
                else
                    Debug.Log("Not quite working");
            }


            //foreach (var node in Nodes)
            //{
            //    var neighbourCells = new List<SmartCell>(node.Cells);
            //    neighbourCells.Remove(this);
            //
            //    foreach (var cell in neighbourCells)
            //    {
            //        if (Nodes.Intersect(cell.Nodes).Count() == 2)
            //        {
            //            Neighbours.Add(cell);
            //        }
            //    }
            //}

            //Neighbours = Neighbours.Distinct().ToList();
        }

        public void CreateLineConnections()
        {
            foreach (var neighbour in Neighbours)
            {
                CreateLineConnection(neighbour);
            }

            // testing here for naked edges, ugly thing

            if (Lines.Count != Nodes.Count)
            {
                var line = new SmartLine(Nodes[0], Nodes[1]);
                var count = 0;

                foreach (var l in Lines)
                {
                    if (line.EquatesTo(l))
                    {
                        count++;
                    }
                }

                if (count == 0)
                {
                    line.AddCells(this, null);
                    Lines.Add(line);
                }
                if (Lines.Count == Nodes.Count)
                    return;

                line = new SmartLine(Nodes[1], Nodes[2]);
                count = 0;

                foreach (var l in Lines)
                {
                    if (line.EquatesTo(l))
                    {
                        count++;
                    }
                }

                if (count == 0)
                {
                    line.AddCells(this, null);
                    Lines.Add(line);
                }
                if (Lines.Count == Nodes.Count)
                    return;

                line = new SmartLine(Nodes[2], Nodes[0]);
                count = 0;

                foreach (var l in Lines)
                {
                    if (line.EquatesTo(l))
                    {
                        count++;
                    }
                }

                if (count == 0)
                {
                    line.AddCells(this, null);
                    Lines.Add(line);
                }
                if (Lines.Count == Nodes.Count)
                    return;
            }
        }

        public SmartLine GetSharedBorder(SmartCell other)
        {
            var commonLines = Lines.Intersect(other.Lines).ToList();

            if (commonLines.Count == 0)
                return null;
            else
                return commonLines[0];
        }

        void CreateLineConnection(SmartCell other)
        {

            foreach (var l in Lines)
            {
                if (l.Neighbours.Contains(other))
                {
                    return;
                }
            }

            var commonNodes = Nodes.Intersect(other.Nodes).ToList();

            if (commonNodes.Count != 2)
                return;

            var line = new SmartLine(commonNodes[0], commonNodes[1]);

            line.AddCells(this, other);

            AddLine(line);
            other.AddLine(line);

        }

        void CalculateBarycentricWeights()
        {
            // calculate vectors from point f to vertices p1, p2 and p3:
            var f1 = Nodes[0].Vert - Center;
            var f2 = Nodes[1].Vert - Center;
            var f3 = Nodes[2].Vert - Center;
            // calculate the areas and factors (order of parameters doesn't matter):
            var a = Vector3.Cross(Nodes[0].Vert - Nodes[1].Vert, Nodes[0].Vert - Nodes[2].Vert).magnitude; // main triangle area a
            var a1 = Vector3.Cross(f2, f3).magnitude / a; // p1's triangle area / a
            var a2 = Vector3.Cross(f3, f1).magnitude / a; // p2's triangle area / a 
            var a3 = Vector3.Cross(f1, f2).magnitude / a; // p3's triangle area / a

            BarycentricWeights = new float[] { a1, a2, a3 };
        }

        public void Resize()
        {
            Center = Nodes[0].Vert + Nodes[1].Vert + Nodes[2].Vert;
            float scale = 1f / 3f;
            Center = new Vector3(Center.x * scale, Center.y * scale, Center.z * scale);
        }

        public void AddLine(SmartLine line)
        {
            Lines.Add(line);
        }

        //Point in triangle

        public bool PointInCell(Vector3 point)
        {
            return PointInTriangle(point, Nodes[0].Vert, Nodes[1].Vert, Nodes[2].Vert);
        }

        private float Sign(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return (p1.x - p3.x) * (p2.z - p3.z) - (p2.x - p3.x) * (p1.z - p3.z);
        }

        private bool PointInTriangle(Vector3 pt, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            bool b1, b2, b3;

            b1 = Sign(pt, v1, v2) < 0.0f;
            b2 = Sign(pt, v2, v3) < 0.0f;
            b3 = Sign(pt, v3, v1) < 0.0f;

            return ((b1 == b2) && (b2 == b3));
        }

        public void DebugDraw(Color color, float duration)
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                Lines[i].DebugDraw(color, duration);
            }
        }

        public int[] GetNeighbourhood()
        {
            var cellIndexes = new List<int>();

            for (int i = 0; i < Nodes.Count; i++)
            {
                var node = Nodes[i];

                for (int u = 0; u < node.Cells.Count; u++)
                {
                    if (cellIndexes.Contains(node.Cells[u].Index))
                    {
                    }
                    else
                    {
                        cellIndexes.Add(node.Cells[u].Index);
                    }
                }
            }

            return cellIndexes.ToArray();
        }
    }

}