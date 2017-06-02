using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Nurbz {
    public class Polyline3 {
        public List<Vector3> Vectors { get; private set; }
        public bool Closed { get; private set; }

        public Vector3 start
        {
            get
            {
                return Vectors[0];
            }
        }

        public Vector3 end
        {
            get
            {
                return Vectors[Vectors.Count - 1];
            }
        }

        public Polyline3(Vector3[] vectors)
        {
            Vectors = vectors.ToList();
            Closed = true;
        }



        public Polyline3(List<Vector3> vectors)
        {
            Vectors = vectors;
            Closed = true;
        }

        public Polyline3(Vector3[] vectors, bool closed)
        {
            Vectors = vectors.ToList();
            Closed = closed;
        }

        public List<Vector2> Vector3ToVector2()
        {
            List<Vector2> vector2d = new List<Vector2>();
            foreach (var vec in Vectors)
            {
                vector2d.Add(new Vector2(vec.x, vec.z));
            }
            return vector2d;
        }

        public Matrix4x4[] GetLoftFrames()
        {
            var matrixList = new List<Matrix4x4>();

            var firstMatrix = new Matrix4x4();
            var orientation = LineMath.LineToCrossVector3(Vectors[0], Vectors[1]);
            firstMatrix.SetTRS(Vectors[0], Quaternion.LookRotation(orientation), Vector3.one);
            //Quaternion.

            matrixList.Add(firstMatrix);

            var length = Vectors.Count;

            for (var i = 1; i < length - 1; i++)
            {
                var workingMatrix = new Matrix4x4();
                var workingPosition = Vectors[i];
                var workingOrientation = LineMath.LineToVector3(Vectors[i - 1], Vectors[i]) + LineMath.LineToVector3(Vectors[i], Vectors[i + 1]);
                workingOrientation.Scale(new Vector3(0.5f, 0.5f, 0.5f));
                workingOrientation = LineMath.VectorToCrossVector3(workingOrientation);
                var workingQuaternion = Quaternion.LookRotation(workingOrientation);

                workingMatrix.SetTRS(workingPosition, workingQuaternion, Vector3.one);
                matrixList.Add(workingMatrix);
            }

            var lastMatrix = new Matrix4x4();
            lastMatrix.SetTRS(Vectors[length - 1], Quaternion.LookRotation(LineMath.LineToCrossVector3(Vectors[length - 2], Vectors[length - 1])), Vector3.one);

            matrixList.Add(lastMatrix);


            return matrixList.ToArray();
        }

        public void Debugdraw()
        {
            for (var i = 0; i < Vectors.Count - 1; i++)
            {
                Debug.DrawLine(Vectors[i], Vectors[i + 1], Color.white, 100f);

            }

        }

        public void Debugdraw(Color color, float time)
        {
            for (var i = 0; i < Vectors.Count - 1; i++)
            {
                Debug.DrawLine(Vectors[i], Vectors[i + 1], color, time);

            }

        }

        public Line3[] GetLines()
        {
            var lineList = new List<Line3>();
            for (var x = 0; x < Vectors.Count - 1; x++)
            {
                lineList.Add(new Line3(Vectors[x], Vectors[x + 1]));
            };

            if (Closed)
            {
                lineList.Add(new Line3(Vectors[Vectors.Count - 1], Vectors[0]));
            }

            return lineList.ToArray();
        }

        public Polyline3 OffsetInPlane(float distance)
        {
            var lines = GetLines();
            var verts = new List<Vector3>();

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


            return new Polyline3(verts.ToArray(), Closed);
        }

        Vector3 GetOffsetIntersection(Line3 lineA, Line3 lineB, float distance)
        {
            lineA = lineA.OffsetLine(distance);
            lineB = lineB.OffsetLine(distance);

            return lineA.IntersectionPoint(lineB);
        }

        public void Flip()
        {
            Vectors.Reverse();
        }

        public Vector3 GetNormal()
        {

            var a = Vectors[0];
            var b = Vectors[1];
            var c = Vectors[2];


            Vector3 side1 = b - a;
            Vector3 side2 = c - a;
            return Vector3.Cross(side1, side2).normalized;
        }

        public void AddToEnd(Vector3 point)
        {
            Vectors.Add(point);
            SetClosed();
        }

        public void AddToStart(Vector3 point)
        {
            Vectors.Insert(0, point);
            SetClosed();

        }

        void SetClosed()
        {
            if (start == end)
                Closed = true;
            else
                Closed = false;
        }

        public List<Vector3> DivideDistance(float distance)
        {
            var dist = 0f;
            var points = new List<Vector3>();

            for (int i = 0; i < Vectors.Count-1; i++)
            {
                var a = Vectors[i];
                var b = Vectors[i+1];

                dist += Vector3.Distance(a, b);

                while (dist > distance)
                {
                    var t = Mathf.InverseLerp(0, distance, dist);
                    var p = Vector3.Lerp(a, b, t);

                    points.Add(p);
                    dist -= distance;
                }
            }

            return points;
        }

        public static List<Polyline3> FormPolylines(List<Line3> lines)
        {

            lines = Line3.GetUniqueLines(lines);

            var output = new List<Polyline3>();

            for (int i = 0; i < lines.Count; i++)
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

                output.Add(new Polyline3(new Vector3[] { line.start, line.end }, false));

                end:
                { }
            }

            for (int u = 0; u < output.Count; u++)
            {
                //output[u].ForceClockwise();
            }

            return output;
        }
    }
}
