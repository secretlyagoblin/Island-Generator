using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Nurbz {
    class MeshContour {

        public Plane Plane { get; private set; }

        public MeshContour(Plane plane)
        {
            Plane = plane;
        }

        void GetSegmentPlaneIntersection(Vector3 p1, Vector3 p2, List<Vector3> points)
        {
            float d1 = Plane.GetDistanceToPoint(p1),
                  d2 = Plane.GetDistanceToPoint(p2);

            bool bP1OnPlane = (Mathf.Abs(d1) < 0.0000001f),
                  bP2OnPlane = (Mathf.Abs(d2) < 0.0000001f);

            if (bP1OnPlane)
                points.Add(p1);

            if (bP2OnPlane)
                points.Add(p2);

            if (bP1OnPlane && bP2OnPlane)
                return;

            if (d1 * d2 > 0)  // points on the same side of plane
                return;

            float t = d1 / (d1 - d2); // 'time' of intersection point on the segment
            points.Add(p1 + t * (p2 - p1));
        }

        Line3 TrianglePlaneIntersection(Vector3 triA, Vector3 triB, Vector3 triC)
        {
            var points = new List<Vector3>();

            GetSegmentPlaneIntersection(triA, triB, points);
            GetSegmentPlaneIntersection(triB, triC, points);
            GetSegmentPlaneIntersection(triC, triA, points);

            points = points.Distinct().ToList();

            if (points.Count() >= 2)
            {
                Debug.DrawLine(points[0], points[1], Color.red, 100f);

                return new Line3(points[0], points[1]);
            }

            return Line3.zero;



        }

        public void ContourMesh(Mesh mesh)
        {
            var tris = mesh.triangles;
            var points = mesh.vertices;

            var dict = new Dictionary<int, List<Line3>>();

            for (int i = 0; i < tris.Length-3; i+=3)
            {
                var p0 = points[tris[i]];
                var p1 = points[tris[i + 1]];
                var p2 = points[tris[i + 2]];

                var sorting = new Vector3[] { p0, p1, p2 };

                var list = sorting.OrderBy(x => x.y).ToList();

                var min = Mathf.CeilToInt(list[0].y);
                var max = Mathf.FloorToInt(list[2].y);

                if (max < min)
                    continue;

                for (int u = min; u <= max; u++)
                {
                    Plane = new Plane(Vector3.up, Vector3.up*u);

                    var line = TrianglePlaneIntersection(p0, p1, p2);

                    if (line.IsEqualTo(Line3.zero))
                        return;

                    if (dict.ContainsKey(u))
                    {
                        dict[u].Add(line);
                    }
                    else
                    {
                        dict.Add(u, new List<Line3> { line });
                    }
                }                
            }

            var keys = dict.Keys.ToArray();

            var polys = new List<Polyline3>();

            for (int i = 0; i < keys.Length; i++)
            {
                var lines = dict[keys[i]];

                polys.AddRange(Polyline3.FormPolylines(lines));
            }

            for (int i = 0; i < polys.Count; i++)
            {

                var outPoints = polys[i].DivideDistance(2.3f);
                polys[i].Debugdraw(Color.blue, 100f);

                var Colour = new Color(RNG.NextFloat(0, 1), RNG.NextFloat(0, 1), RNG.NextFloat(0, 1));

                for (int u = 0; u < outPoints.Count; u++)
                {
                    var p = outPoints[u];
                    Debug.DrawRay(p, Vector3.up, Colour, 100f);
                }
            }
        }



    }
}
