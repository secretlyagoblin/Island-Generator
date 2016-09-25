using UnityEngine;
using System.Collections.Generic;

namespace Nurbz {

    public class Polyline2 {

        public Vector2[] Vectors
        { get; private set; }
        public bool Closed
        { get; private set; }

        public Polyline2 (Vector2[] vectors, bool closed)
        {
            Vectors = vectors;
            Closed = closed;
        }

        public void DebugDraw(Color color, float time)
        {
            for (var i = 0; i < Vectors.Length - 1; i++)
            {
                Debug.DrawLine(Vectors[i], Vectors[i + 1], color, time);
            }

            if (Closed)
            {
                Debug.DrawLine(Vectors[Vectors.Length - 1], Vectors[0], color, time);
            }
        }

        public void DebugDraw(Gradient gradient, float time)
        {
            var stepSize = 1f / Vectors.Length;
            var stepTotal = 0f;

            for (var i = 0; i < Vectors.Length - 1; i++)
            {
                Debug.DrawLine(Vectors[i], Vectors[i + 1], gradient.Evaluate(stepTotal), time);
                stepTotal += stepSize;
            }

            if (Closed)
            {
                Debug.DrawLine(Vectors[Vectors.Length - 1], Vectors[0], gradient.Evaluate(1f), time);
            }
        }

        public Polyline2 OffsetInPlane(float distance)
        {
            var lines = GetLines();
            var verts = new List<Vector2>();

            if (Closed)
            {

                verts.Add(GetOffsetIntersection(lines[lines.Length - 1], lines[0], distance));

                for (var i = 1; i < Vectors.Length; i++)
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
            for (var x = 0; x < Vectors.Length - 1; x++)
            {
                lineList.Add(new Line2(Vectors[x], Vectors[x + 1]));
            };

            if (Closed)
            {
                lineList.Add(new Line2(Vectors[Vectors.Length - 1], Vectors[0]));
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
            System.Array.Reverse(Vectors);

        }

        public void ForceAntiClockwise()
        {
            if (IsClockwise(Vectors))
                System.Array.Reverse(Vectors);
        }

        bool IsClockwise(Vector2[] vectors)
        {
            var total = 0f;
            for (int i = 0; i < vectors.Length - 1; i++)
            {
                var a = vectors[i];
                var b = vectors[i + 1];
                total += ((b.x - a.x) * (a.y + b.y));
            }

            var c = vectors[vectors.Length-1];
            var d = vectors[0];
            total += ((d.x - c.x) * (d.y + c.y));

            Debug.Log(total);

            return (total >= 0f);
        }
    }
}