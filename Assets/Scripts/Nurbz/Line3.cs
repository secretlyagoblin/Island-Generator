using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Nurbz
{
    public struct Line3
    {
        public Vector3 start { get; private set; }
        public Vector3 end { get; private set; }
        public Vector3 middle {
            get { return GetMidPoint(); }
        }
        public Vector3 orientation
        {
            get { return GetOrientation(); }
        }

        public static Line3 zero = new Line3(Vector3.zero, Vector3.zero);

        public Line3(Vector3 a, Vector3 b)
        {
            start = a;
            end = b;
        }

        public bool IsEqualTo(Line3 other)
        {
            if (start == other.start && end == other.end)
                return true;
            return false;
        }

        public static List<Line3> GetUniqueLines(List<Line3> linesToSmallify)
        {
            return linesToSmallify.GroupBy(g => g.middle).Select(g => g.First()).ToList();
        } 

        Vector3 GetMidPoint()
        {
            return Vector3.Lerp(start, end, 0.5f);
            //return new Vector3((start.x + end.x) * 0.5f, (start.y + end.y) * 0.5f, (start.z + end.z) * 0.5f);
        }

        Vector3 GetOrientation()
        {
            return LineMath.LineToVector3(start, end);
        }

        public float FindDegree()
        {
            var vector = LineMath.LineToVector3(start, end);
            float value = (float)((Mathf.Atan2(vector.x, vector.z) / Mathf.PI) * 180f);
            if (value < 0)
                value += 360f;
            return value;
        }

        public Vector3 IntersectionPoint(Line3 otherLine)
        {
            return LineMath.FindCoPlanarIntersectionPoint(this, otherLine);
        }

        public void DrawDebugView(float duration, Color color)
        {
            Debug.DrawLine(start, end, color, duration);
        }

        public Line3 OffsetLine(float distance)
        {
            return LineMath.OffsetLine(this, distance);
        }
    }
}
