using UnityEngine;
using System.Collections.Generic;
using MeshMasher;
using Nurbz;
using System.Linq;

namespace MeshMasher {
    public class SmartNode {

        public Vector3 Vert;

        //public int State
        //{ get; set; }
        //public float Weight
        //{
        //    get; set;
        //}

        public List<SmartNode> Nodes
        { get; private set; }
        public List<SmartCell> Cells
        { get; private set; }
        public List<SmartLine> Lines
        { get; private set; }
        public int Index
        { get; set; }

        private List<float> _angles = new List<float>();
        private bool _anglesNeedsUpdating = false;

        private bool _fanNeedsUpdating = true;

        public SmartNode(Vector3 vert, int index)
        {
            this.Vert = vert;
            this.Index = index;

            Nodes = new List<SmartNode>();
            Cells = new List<SmartCell>();
            Lines = new List<SmartLine>();
        }

        public void AddCell(SmartCell cell)
        {
            Cells.Add(cell);
        }

        public void AddLine(SmartLine line)
        {
            Lines.Add(line);
            _anglesNeedsUpdating = true;
        }

        public void UpdateLineSort()
        {
            List<SmartAndAbstractLine> lines = new List<SmartAndAbstractLine>();

            foreach (var line in Lines)
            {
                var other = line.GetOtherLine(this);
                lines.Add(new SmartAndAbstractLine(line, new Line3(Vert, other.Vert)));
            }

            lines.Sort((x, y) => x.Line.FindDegree().CompareTo(y.Line.FindDegree()));

            Lines.Clear();

            foreach (var line in lines)
            {
                Lines.Add(line.Smartline);
                _angles.Add(line.Line.FindDegree());
            }





        }

        public SmartLine GetSharedLine(SmartNode other)
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                var l = Lines[i];
                if (l.GetOtherNode(this) == other)
                    return l;
            }

            return null;
        }

        public SmartLine GetNextClockwiseLineSegment(SmartLine testLine, MeshState<int> state)
        {

            if (_anglesNeedsUpdating)
            {
                UpdateLineSort();
                _anglesNeedsUpdating = false;
            }

            for (int i = 0; i < Lines.Count - 1; i++)
                if (Lines[i] == testLine && state.Lines[Lines[i + 1].Index] == 1)
                    return Lines[i + 1];

            if (state.Lines[Lines[0].Index] == 1)
                return Lines[0];

            return testLine;
        }

        void UpdateFan()
        {
            Cells = Cells.OrderBy(x => Mathf.Atan2(x.Center.x - Vert.x, x.Center.z - Vert.z)).ToList();
        }

        public void DisplayFanTriangles(Color color, float time)
        {
            if (_fanNeedsUpdating)
                UpdateFan();

            var first = Cells[0].Center;

            for (int i = 1; i < Cells.Count-1; i++)
            {
                Debug.DrawLine(first, Cells[i].Center, color, time);
                Debug.DrawLine(Cells[i].Center, Cells[i+1].Center, color, time);
                Debug.DrawLine(Cells[i+1].Center, first, color, time);
            }
        }

        public bool PointInNodeFan(Vector3 point)
        {
            if (_fanNeedsUpdating)
                UpdateFan();

            var first = Cells[0].Center;

            for (int i = 1; i < Cells.Count - 1; i++)
            {
                if (PointInTriangle(point, first, Cells[i].Center, Cells[i + 1].Center))
                    return true;

            }
            return false;
        }

        float sign(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return (p1.x - p3.x) * (p2.z - p3.z) - (p2.x - p3.x) * (p1.z - p3.z);
        }

        bool PointInTriangle(Vector3 pt, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            bool b1, b2, b3;

            b1 = sign(pt, v1, v2) < 0.0f;
            b2 = sign(pt, v2, v3) < 0.0f;
            b3 = sign(pt, v3, v1) < 0.0f;

            return ((b1 == b2) && (b2 == b3));
        }

        private class SmartAndAbstractLine {
            public SmartLine Smartline;
            public Line3 Line;
            public SmartAndAbstractLine(SmartLine a, Line3 b)
            {
                Smartline = a;
                Line = b;
            }
        }



    }

}