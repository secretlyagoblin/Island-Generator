using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Nurbz {

    public class Polyline2 {

        public List<Vector2> Vectors
        { get; private set; }
        public bool Closed
        { get; private set; }

        public Vector2 start
        {
            get
            {
                return Vectors[0];
            }
        }

        public Vector2 end
        {
            get
            {
                return Vectors[Vectors.Count-1];
            }
        }

        public Polyline2 (Vector2[] vectors, bool closed)
        {
            Vectors = vectors.ToList();
            Closed = closed;
        }

        public Polyline2(List<Vector2> vectors, bool closed)
        {
            Vectors = vectors;
            Closed = closed;
        }

        public void DebugDraw(Color color, float time)
        {
            for (var i = 0; i < Vectors.Count - 1; i++)
            {
                Debug.DrawLine(Vectors[i], Vectors[i + 1], color, time);
            }

            if (Closed)
            {
                Debug.DrawLine(Vectors[Vectors.Count - 1], Vectors[0], color, time);
            }
        }

        public void AddToEnd(Vector2 point)
        {
            Vectors.Add(point);
            SetClosed();
        }

        public void AddToStart(Vector2 point)
        {
            Vectors.Insert(0,point);
            SetClosed();

        }

        void SetClosed()
        {
            if (start == end)
                Closed = true;
            else
                Closed = false;
        }

        public void DebugDraw(Gradient gradient, float time)
        {
            var stepSize = 1f / Vectors.Count;
            var stepTotal = 0f;

            for (var i = 0; i < Vectors.Count - 1; i++)
            {
                Debug.DrawLine(Vectors[i], Vectors[i + 1], gradient.Evaluate(stepTotal), time);
                stepTotal += stepSize;
            }

            if (Closed)
            {
                Debug.DrawLine(Vectors[Vectors.Count - 1], Vectors[0], gradient.Evaluate(1f), time);
            }
        }

        public Polyline2 OffsetInPlane(float distance)
        {
            var lines = GetLines();
            var verts = new List<Vector2>();

            if (Closed)
            {

                verts.Add(GetOffsetIntersection(lines[lines.Length - 1], lines[0], distance));

                for (var i = 1; i < Vectors.Count; i++)
                {
                    verts.Add(GetOffsetIntersection(lines[i - 1], lines[i], distance));
                }
            }
            else
            {
                Debug.Log("Buhh???");
            }


            return new Polyline2(verts.ToArray(), Closed);
        }

        public Line2[] GetLines()
        {
            var lineList = new List<Line2>();
            for (var x = 0; x < Vectors.Count - 1; x++)
            {
                lineList.Add(new Line2(Vectors[x], Vectors[x + 1]));
            };

            if (Closed)
            {
                lineList.Add(new Line2(Vectors[Vectors.Count - 1], Vectors[0]));
            }

            return lineList.ToArray();
        }

        Vector2 GetOffsetIntersection(Line2 lineA, Line2 lineB, float distance)
        {
            lineA = lineA.OffsetLine(distance);
            lineB = lineB.OffsetLine(distance);

            return lineA.IntersectionPoint(lineB);
        }

        public void ForceClockwise()
        {
            if (IsClockwise(Vectors))
                return;

            Debug.Log("we had to reverse this");
            Vectors.Reverse();

        }

        public void ForceAntiClockwise()
        {
            if (IsClockwise(Vectors))
                Vectors.Reverse();
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

        public void AddPoint(Vector2 vector)
        {

        }

        public static List<Polyline2> FormPolylines(List<Line2> lines)
        {
            var output = new List<Polyline2>();

            for (int i = 0; i <  lines.Count; i++)
            {
                var line = lines[i];

                for (int u = 0; u < output.Count; u++)
                {
                    var poly = output[u];

                    if (poly.Closed)
                        continue;

                    if (line.start == poly.start)
                    {
                        poly.AddToStart(line.end);
                        goto end;
                    }
                    else if (line.start == poly.end)
                    {
                        poly.AddToEnd(line.end);
                        goto end;
                    }
                    else if (line.end == poly.start)
                    {
                        poly.AddToStart(line.start);
                        goto end;
                    }
                    else if (line.start == poly.end)
                    {
                        poly.AddToEnd(line.end);
                        goto end;
                    }
                        

                }

                output.Add(new Polyline2(new Vector2[] { line.start, line.end }, false));

                end:
                { }
            }

            for (int u = 0; u < output.Count; u++)
            {
                output[u].ForceClockwise();
            }

                return output;
        }


    }
}