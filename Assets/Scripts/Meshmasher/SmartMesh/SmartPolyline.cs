using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MeshMasher;

namespace MeshMasher {

    class SmartPolyline {
        public SmartNode StartPoint;
        public SmartNode EndPoint;
        public List<SmartLine> LineSections
        { get; private set; }

        public SmartPolyline(SmartLine line)
        {
            LineSections = new List<SmartLine>();
            LineSections.Add(line);
            StartPoint = line.Nodes[0];
            EndPoint = line.Nodes[1];
        }

        public bool isClosed()
        {
            if (StartPoint == EndPoint)
            {
                return true;
            }
            return false;
        }

        public bool isConnectedTo(SmartLine other)
        {
            if (StartPoint == other.Nodes[0])
                return true;
            if (EndPoint == other.Nodes[0])
                return true;
            if (StartPoint == other.Nodes[1])
                return true;
            if (EndPoint == other.Nodes[1])
                return true;
            return false;
        }

        public void Intergrate(SmartLine line)
        {
            if (line.Nodes[0] == StartPoint)
            {
                StartPoint = line.Nodes[1];
                LineSections.Insert(0, line);
            }
            else if (line.Nodes[1] == StartPoint)
            {
                StartPoint = line.Nodes[0];
                LineSections.Insert(0, line);
            }
            else if (line.Nodes[0] == EndPoint)
            {
                EndPoint = line.Nodes[1];
                LineSections.Add(line);
            }
            else if (line.Nodes[1] == EndPoint)
            {
                EndPoint = line.Nodes[0];
                LineSections.Add(line);
            }
        }

        public List<SmartNode> GetNodeList()
        {
            var outputNodeList = new List<SmartNode>();

            var node = LineSections[0].GetSharedNode(LineSections[0 + 1]);
            outputNodeList.Add(LineSections[0].GetOtherLine(node));

            for (var x = 0; x < LineSections.Count - 1; x++)
            {
                outputNodeList.Add(LineSections[x].GetSharedNode(LineSections[x + 1]));
            }

            outputNodeList.Add(LineSections.Last().GetOtherLine(outputNodeList.Last()));

            return outputNodeList;
        }

        public void ForceClockwiseXZ()
        {
            var points = new List<Vector2>();
            var nodeList = GetNodeList();

            for (int i = 0; i < nodeList.Count; i++)
            {
                var n = nodeList[i];
                points.Add(new Vector2(n.Vert.x, n.Vert.z));
            }

            if (IsClockwise(points))
                return;
            else
            {
                LineSections.Reverse();
                var s = StartPoint;
                StartPoint = EndPoint;
                EndPoint = s;
            }

        }

        bool IsClockwise(List<Vector2> vectors)
        {
            var total = 0f;
            for (int i = 0; i < vectors.Count - 1; i++)
            {
                var a = vectors[i];
                var b = vectors[i + 1];
                total += ((b.x - a.x) * (a.y + b.y));
            }

            var c = vectors[vectors.Count - 1];
            var d = vectors[0];
            total += ((d.x - c.x) * (d.y + c.y));

            //Debug.Log(total);

            return (total >= 0f);
        }
    }

}
