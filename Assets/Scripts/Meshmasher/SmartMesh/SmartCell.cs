using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MeshMasher;

namespace MeshMasher {
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

        public SmartCell(SmartNode nodeA, SmartNode nodeB, SmartNode nodeC)
        {

            Nodes = new List<SmartNode>(new SmartNode[] { nodeA, nodeB, nodeC });
            Neighbours = new List<SmartCell>();
            Lines = new List<SmartLine>();

            Center = nodeA.Vert + nodeB.Vert + nodeC.Vert;
            float scale = 1f / 3f;
            Center = new Vector3(Center.x * scale, Center.y * scale, Center.z * scale);
        }

        public void CreateNodeConnections()
        {
            foreach (var node in Nodes)
            {
                node.AddCell(this);
            }
        }

        public void CreateCellConnections()
        {

            foreach (var node in Nodes)
            {
                var neighbourCells = new List<SmartCell>(node.Cells);
                neighbourCells.Remove(this);

                foreach (var cell in neighbourCells)
                {
                    if (Nodes.Intersect(cell.Nodes).Count() == 2)
                    {
                        Neighbours.Add(cell);
                    }
                }
            }
        }

        public void CreateLineConnections()
        {
            foreach (var neighbour in Neighbours)
            {
                CreateLineConnection(neighbour);
            }

            // testing here for naked edges, ugly thing

            if (Lines.Count() != Nodes.Count())
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
                if (Lines.Count() == Nodes.Count())
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
                if (Lines.Count() == Nodes.Count())
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
                if (Lines.Count() == Nodes.Count())
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

            if (commonNodes.Count() != 2)
                return;

            var line = new SmartLine(commonNodes[0], commonNodes[1]);

            line.AddCells(this, other);

            AddLine(line);
            other.AddLine(line);

        }

        public void AddLine(SmartLine line)
        {
            Lines.Add(line);
        }


    }

}