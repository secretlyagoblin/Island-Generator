using UnityEngine;
using System.Collections.Generic;
using MeshMasher;
using System.Linq;
using System;

namespace MeshMasher {
    public class SmartLine{

        public int State
        { get; set; }
        public List<SmartNode> Nodes
        { get; private set; }
        public List<SmartCell> Neighbours
        { get; private set; }
        //public List<SmartLine> Lines
        //{ get; private set; }
        public int Index
        { get; set; }

        public float Length { get; private set; }

        public Vector3 Center { get
            {
                return Vector3.Lerp(Nodes[0].Vert, Nodes[1].Vert, 0.5f);
            } }

        public SmartLine(SmartNode nodeA, SmartNode nodeB)
        {

            Nodes = new List<SmartNode>(2){ nodeA, nodeB };
            Neighbours = new List<SmartCell>(2);
            //Lines = new List<SmartLine>();

            nodeA.AddLine(this);
            nodeB.AddLine(this);

            Length = Vector3.Distance(nodeA.Vert, nodeB.Vert);

        }

        public void AddNeighbour(SmartCell cell)
        {
            Neighbours.Add(cell);
        }

        public void AddCells(SmartCell cellA, SmartCell cellB)
        {
            Neighbours.Add(cellA);
            Neighbours.Add(cellB);
        }

        public SmartCell GetCellPartner(SmartCell cell)
        {
            if (Neighbours.Count <= 1)
            {
                return null;
            }

            if (cell == Neighbours[0])
                return Neighbours[1];
            return Neighbours[0];
        }

        public bool IsConnectedTo(SmartLine other)
        {
            if (Nodes[0] == other.Nodes[0])
                return true;
            if (Nodes[1] == other.Nodes[0])
                return true;
            if (Nodes[0] == other.Nodes[1])
                return true;
            if (Nodes[1] == other.Nodes[1])
                return true;
            return false;
        }

        public SmartNode GetOtherLine(SmartNode node)
        {

            if (node == Nodes[0])
                return Nodes[1];
            if (node == Nodes[1])
                return Nodes[0];
            Debug.Log("You are using this wrong, this works only on polylines");
            return null;
        }

        public SmartNode GetSharedNode(SmartLine other)
        {
            if (Nodes[0] == other.Nodes[0])
                return Nodes[0];
            if (Nodes[1] == other.Nodes[0])
                return Nodes[1];
            if (Nodes[0] == other.Nodes[1])
                return Nodes[0];
            if (Nodes[1] == other.Nodes[1])
                return Nodes[1];
            return null;
        }

        public SmartNode GetOtherNode(SmartNode other)
        {
            if (Nodes[0] == other)
                return Nodes[1];
            if (Nodes[1] == other)
                return Nodes[0];
            return null;
        }

        public List<SmartLine> CollectConnectedLines()
        {
            var lines = Nodes[0].Lines.ToList();
            lines.AddRange(Nodes[1].Lines);
            lines = lines.Distinct().ToList();
            lines.Remove(this);
            return lines;
        }

        public bool EquatesTo(SmartLine other)
        {
            if (other.Nodes[0] == Nodes[0] && other.Nodes[1] == Nodes[1])
                return true;
            if (other.Nodes[1] == Nodes[0] && other.Nodes[0] == Nodes[1])
                return true;
            return false;
        }

        public void DebugDraw(Color color, float duration)
        {
            Debug.DrawLine(Nodes[0].Vert, Nodes[1].Vert, color, duration);
        }

        public void DrawLine(Color color, float duration, float offset)
        {
            Debug.DrawLine(Nodes[0].Vert+(Vector3.up*offset), Nodes[1].Vert + (Vector3.up * offset), color, duration);
        }

        public void DrawLine(Color color, float duration, Vector3 offset)
        {
            Debug.DrawLine(Nodes[0].Vert + (offset), Nodes[1].Vert + (offset), color, duration);
        }

    }
}