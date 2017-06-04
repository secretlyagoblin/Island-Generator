﻿using UnityEngine;
using System.Collections.Generic;
using MeshMasher;
using Nurbz;

namespace MeshMasher {
    public class SmartNode {

        public Vector3 Vert;

        public int Index
        { get; private set; }
        public int State
        { get; set; }
        public List<SmartNode> Nodes
        { get; private set; }
        public List<SmartCell> Cells
        { get; private set; }
        public List<SmartLine> Lines
        { get; private set; }

        private List<float> _angles = new List<float>();
        private bool _anglesNeedsUpdating = false;

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
                var other = line.GetInternalNodePartner(this);
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

        public SmartLine GetNextClockwiseLineSegment(SmartLine testLine)
        {

            if (_anglesNeedsUpdating)
            {
                UpdateLineSort();
                _anglesNeedsUpdating = false;
            }

            for (int i = 0; i < Lines.Count - 1; i++)
                if (Lines[i] == testLine && Lines[i + 1].IsPartOfCurrentSortingEvent)
                    return Lines[i + 1];

            if (Lines[0].IsPartOfCurrentSortingEvent)
                return Lines[0];

            return testLine;
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