using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BuildingGenerator {

    public static class BlueprintRoomGenerator {

        //List<VectorLine2d> _lines = new List<VectorLine2d>();

        public static int[][] GetOutlinePatterns(Vector2[] vector2Array, int[] relevantTriangles, int[] relevantWallTypes, bool debug)
        {
            //TODO Change vector3 array to v2 array to stop chance of errors

            var lines = GetLinesFromTriangles(vector2Array, relevantTriangles, relevantWallTypes);

            if (debug)
            {

                foreach(var line in lines)
                {
                    line.DebugDraw(Color.red, 100f);
                }

                Debug.Log("Total lines: " + lines.Count);
            }



            var perimeter = GetRoomPerimeterFromLines(lines, vector2Array);

            var outputOutlines = new List<int[]>();

            for (int i = 0; i < perimeter.Count; i++)
            {
                outputOutlines.Add(perimeter[i].GetOutlinePattern());
            }

            return outputOutlines.ToArray();

        }

        static List<RoomLine> GetLinesFromTriangles(Vector2[] vector2Array, int[] triangles, int[] wallTypes)
        {
            List<Vector2> vector2 = new List<Vector2>();

            for (int i = 0; i < vector2Array.Length; i++)
            {
                vector2.Add(new Vector2(vector2Array[i].x, vector2Array[i].y));
            }

            List<int> triangleList = new List<int>(triangles);

            var lines = new List<RoomLine>();
            var duplicateLines = new List<RoomLine>();

            for (int i = 0; i < triangleList.Count; i += 3)
            {

                var lineA = new RoomLine(vector2[triangleList[i]], triangleList[i], vector2[triangleList[i + 1]], triangleList[i + 1], wallTypes[i]);
                var lineB = new RoomLine(vector2[triangleList[i + 1]], triangleList[i + 1], vector2[triangleList[i + 2]], triangleList[i + 2], wallTypes[i + 1]);
                var lineC = new RoomLine(vector2[triangleList[i + 2]], triangleList[i + 2], vector2[triangleList[i]], triangleList[i], wallTypes[i + 2]);

                //cycles through all lines and adds duplicate lines to a big ole list

                for (int x = 0; x < lines.Count; x++)
                {

                    if (lineA.isEquivilentTo(lines[x]))
                        duplicateLines.Add(lineA);

                    if (lineB.isEquivilentTo(lines[x]))
                        duplicateLines.Add(lineB);

                    if (lineC.isEquivilentTo(lines[x]))
                        duplicateLines.Add(lineC);
                }

                lines.AddRange(new RoomLine[] { lineA, lineB, lineC });
            }

            //goes through lines and deletes all duplicates that aren't walls
            for (int i = 0; i < duplicateLines.Count; i++)
            {
                lines.RemoveAll(a => (a.isEquivilentTo(duplicateLines[i]) && a.LineType == 0));
            }

            //goes through lines deletes the first occurence of remaining walls. Should leave only one
            for (int i = 0; i < duplicateLines.Count; i++)
            {
                lines.Remove(duplicateLines[i]);
            }

            /*
            for (int i = 0; i < lines.Count; i++)
            {

                lines[i].DebugDraw(Color.red, 100f);
            }
            */
            return lines;
        }

        static List<RoomOutline> GetRoomPerimeterFromLines(List<RoomLine> lines, Vector2[] vector2Array)
        {
            //debugLoop (Color.blue,100f);

            var roomPoints = new List<RoomPoint>();

            for (int i = 0; i < vector2Array.Length; i++)
            {
                roomPoints.Add(new RoomPoint(new Vector2(vector2Array[i].x, vector2Array[i].y),i));
            }

            var clockwise = true;
            var debug = true;

            LinkPoints(lines, roomPoints);


            SortPoints(clockwise, roomPoints, debug);
            roomPoints = CullDisconnectedPoints(roomPoints);

            var loop = true;
            var count = 0;

            var outputRoomOutlines = new List<RoomOutline>();

            while (loop)
            {
                var startIndex = roomPoints.FindIndex(a => a.VertIndex == lines[0].StartIndex);

                var points = WalkPoints(startIndex, clockwise, roomPoints);
                var relevantLines = WalkLines(points);
                //debugWalk(points, _clockwiseColor, 100f, clockwise);
                outputRoomOutlines.Add(new RoomOutline(points, relevantLines));

                //remove lines from the system

                for (int i = 0; i < points.Count - 1; i++)
                {
                    var lineHolder = new List<RoomLine>();
                    for (int l = 0; l < lines.Count; l++)
                    {
                        if (points[i].VertIndex == lines[l].StartIndex | points[i].VertIndex == lines[l].EndIndex)
                        {
                            if (points[i + 1].VertIndex == lines[l].StartIndex | points[i + 1].VertIndex == lines[l].EndIndex)
                            {
                                lineHolder.Add(lines[l]);
                            }
                        }
                    }

                    for (int l = 0; l < lineHolder.Count; l++)
                    {
                        lines.Remove(lineHolder[l]);
                    }
                }

                if (lines.Count == 0)
                    loop = false;

                count++;
                if (count > 100)
                    loop = false;
            }

            return outputRoomOutlines;
            //sortPoints (!clockwise);
            //debugWalk(walkPoints(startIndex, !clockwise),_antiClockwiseColor,100f, !clockwise);
        }

        static void LinkPoints(List<RoomLine> lines, List<RoomPoint> points)
        {
             //+ ", index " + points[i].VertIndex);

            for (int i = 0; i < lines.Count; i++)
            {
                //Debug.Log("Linking Line " + i);

                var line = lines[i];

                points[line.StartIndex].AddPoint(points[line.EndIndex], line);
                points[line.EndIndex].AddPoint(points[line.StartIndex], line);
            }
        }

        static void SortPoints(bool clockwise, List<RoomPoint> points, bool debug)
        {
            if (debug)
            {

                if (clockwise)
                {
                    Debug.Log("Going Clockwise...");
                }
                else
                {
                    Debug.Log("Going Anti-Clockwise...");
                }
            }

            

            for (int i = 0; i < points.Count; i++)
            {
                //Debug.Log("Sorting Point " + i + ", index " + points[i].VertIndex);
                points[i].SortPoints(clockwise);
            }
        }

        static List<RoomPoint> CullDisconnectedPoints(List<RoomPoint> points)
        {
            var outputPoints = new List<RoomPoint>();

            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].isConnected)
                    outputPoints.Add(points[i]);
            }

            return outputPoints;
        }

        static List<RoomPoint> WalkPoints(int startIndex, bool clockwise, List<RoomPoint> points)
        {


            var finalPath = new List<RoomPoint>();

            var getPoint = points[startIndex];

            finalPath.Add(getPoint);
            finalPath.Add(finalPath[0].GetFirstConnection(clockwise));


            var loop = true;
            var i = 0;
            var loopBreak = 150;

            var firstPoint = finalPath[0];
            var secondPoint = finalPath[1];
            var previousPoint = firstPoint;
            var currentPoint = secondPoint;

            while (loop)
            {

                var nextPoint = currentPoint.GetNextPoint(previousPoint);

                if (firstPoint == currentPoint && nextPoint == secondPoint)
                    return finalPath;

                finalPath.Add(nextPoint);

                previousPoint = currentPoint;
                currentPoint = nextPoint;

                i++;
                if (i >= loopBreak)
                    loop = false;
            }

            Debug.Log("No loop");
            return null;
        }

        static List<RoomLine> WalkLines(List<RoomPoint> points)
        {
            var output = new List<RoomLine>();
            for (int i = 0; i < points.Count-1; i++)
            {
                output.Add(points[i].GetNextLine(points[i + 1]));
            }

            return output;
        }

        class RoomLine {

            public Vector2 StartPoint
            { get; private set; }
            public int StartIndex
            { get; private set; }
            public Vector2 EndPoint
            { get; private set; }
            public int EndIndex
            { get; private set; }
            public int LineType
            { get; private set; }

            public RoomLine(Vector2 startPoint, int startIndex, Vector2 endPoint, int endIndex, int lineType)
            {

                StartPoint = startPoint;
                StartIndex = startIndex;
                EndPoint = endPoint;
                EndIndex = endIndex;
                LineType = lineType;

            }

            public void DebugDraw(Color color, float duration)
            {

                var startPoint = StartPoint + jitter();
                var endPoint = EndPoint + jitter();
                var midPoint = (startPoint + endPoint);
                midPoint.Scale(new Vector2(0.5f, 0.5f));

                Debug.DrawLine(startPoint, midPoint, color, duration);
                Debug.DrawLine(midPoint, endPoint, Color.blue, duration);
            }

            Vector2 jitter()
            {
                var jitt = 0.00f;
                return new Vector2(Random.Range(-jitt, jitt), Random.Range(-jitt, jitt));
            }


            public bool isEquivilentTo(RoomLine other)
            {
                return ((StartIndex == other.StartIndex && EndIndex == other.EndIndex) || (EndIndex == other.StartIndex && StartIndex == other.EndIndex));
            }
        }

        class RoomPoint {

            public float x
            { get { return Vector2.x; } set { Vector2.x = value; } }
            public float y
            { get { return Vector2.x; } set { Vector2.x = value; } }

            public int VertIndex
            { get; private set; }

            public Vector2 vec
            { get { return Vector2; } }

            public bool isConnected
            { get { return _pointLinks.Count != 0; } }

            List<RoomPoint> _pointLinks;
            List<float> _angles = new List<float>();
            List<RoomLine> _lines = new List<RoomLine>();

            Dictionary<float, RoomPoint> _pointAngle = new Dictionary<float, RoomPoint>();
            Dictionary<RoomPoint, RoomLine> _linePoint = new Dictionary<RoomPoint, RoomLine>();

            Vector2 Vector2;

            public RoomPoint(Vector2 vector2, int vertIndex)
            {
                Vector2 = vector2;
                _pointLinks = new List<RoomPoint>();
                VertIndex = vertIndex;
            }

            public void FlushPoints()
            {
                _pointLinks.Clear();
            }

            public void AddPoint(RoomPoint point, RoomLine line)
            {
                if (_pointLinks.Contains(point))
                    return;

                var angle = Util.AngleCalculator.GetAngle(point.vec - vec, Vector2.up);

                //IF THERE ARE ISSUES HERE IT'S BECAUSE WE ARE USING VECTOR2 + VECTOR3 INTERCHANGEBLY RIGHT NOW

                //Debug.Log(angle);
                _pointLinks.Add(point);
                _angles.Add(angle);
                _lines.Add(line);
                _pointAngle.Add(angle, point);
                _linePoint.Add(point, line);
            }

            public void SortPoints(bool clockwise)
            {
                _angles.Sort();
                if (clockwise)
                    _angles.Reverse();

                _pointLinks.Clear();

                for (int i = 0; i < _angles.Count; i++)
                {
                    _pointLinks.Add(_pointAngle[_angles[i]]);
                }
            }

            public RoomPoint GetNextPoint(RoomPoint point)
            {
                for (int i = 0; i < _pointLinks.Count; i++)
                {
                    if (_pointLinks[i] == point)
                    {
                        if (i + 1 == _pointLinks.Count)
                        {
                            //Debug.Log ("Next angle is " + _angles [0]);
                            return _pointLinks[0];
                        }
                        else
                            //Debug.Log("Next angle is " + _angles [i+1]);
                            return _pointLinks[i + 1];
                    }
                }

                Debug.Log("No connection, apparently");
                return null;
            }

            public RoomPoint GetFirstConnection(bool clockwise)
            {
                for (int i = 0; i < _lines.Count; i++)
                {
                    if(_lines[i].StartIndex == VertIndex)
                    {
                        //Debug.Log("it worked and there were also no consequences ");
                        return _pointLinks.Find(a =>a.VertIndex == _lines[i].EndIndex);
                    }
                }


                return clockwise ? _pointLinks[0] : _pointLinks.Last();
            }

            public RoomLine GetNextLine(RoomPoint point)
            {
                return _linePoint[point];
            }


            //	public Point2d GetNextClockwisePoint(float angle){
            //		angle = angle + 180f;
            //		if(angle > 360f) angle = angle - 360f;
            //
            //		for (int i = 0; i < _angles.Count; i++) {
            //			if (angle < _angles [i])
            //				return _pointAngle [i];
            //		}
            //		return _pointAngle [0];
            //	}
            //
            //	public float Angle(Point2d point){
            //		return _pointAngle [point];
            //	}
            //
            //	public void GetNextClockwisePoint(){
            //		
            //	}


        }

        class RoomOutline {

            public List<RoomPoint> Points
            { get; private set; }
            public List<RoomLine> Lines
            { get; private set; }

            public RoomOutline(List<RoomPoint> points, List<RoomLine> lines )
            {
                Points = points;
                Lines = lines;
            }

            public int[] GetOutlinePattern()
            {
                var outputPattern = new List<int>();

                for (int i = 0; i < Lines.Count; i++)
                {
                    outputPattern.Add(Points[i].VertIndex);
                    outputPattern.Add(Lines[i].LineType);
                }
                return outputPattern.ToArray();
            }
        }

    }
}