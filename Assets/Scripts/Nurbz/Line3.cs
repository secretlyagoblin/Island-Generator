using UnityEngine;
using System.Collections;

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

        public Line3(Vector3 a, Vector3 b)
        {
            start = a;
            end = b;
        }

        Vector3 GetMidPoint()
        {
            return new Vector3((start.x + end.x) * 0.5f, (start.y + end.y) * 0.5f, (start.z + end.z) * 0.5f);
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
