using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Meshmasher;

namespace Meshmasher {

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

        public List<SmartNode> getNodeList()
        {
            var outputNodeList = new List<SmartNode>();

            var node = LineSections[0].GetSharedNode(LineSections[0 + 1]);
            outputNodeList.Add(LineSections[0].GetInternalNodePartner(node));

            for (var x = 0; x < LineSections.Count - 1; x++)
            {
                outputNodeList.Add(LineSections[x].GetSharedNode(LineSections[x + 1]));
            }

            outputNodeList.Add(LineSections.Last().GetInternalNodePartner(outputNodeList.Last()));

            return outputNodeList;
        }
    }

}
