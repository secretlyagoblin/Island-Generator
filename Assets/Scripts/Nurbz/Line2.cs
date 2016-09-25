using UnityEngine;
using System.Collections.Generic;

namespace Nurbz {

    public struct Line2 {

        public Vector2 start
        { get; private set; }
        public Vector2 end
        { get; private set; }
        public Vector2 middle
        {
            get { return GetMidPoint(); }
        }
        public Vector2 orientation
        {
            get { return GetOrientation(); }
        }

        public Line2(Vector2 a, Vector2 b)
        {
            start = a;
            end = b;
        }

        Vector2 GetMidPoint()
        {
            return new Vector2((start.x + end.x) * 0.5f, (start.y + end.y) * 0.5f);
        }

        Vector2 GetOrientation()
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

        public Vector2 IntersectionPoint(Line2 otherLine)
        {
            return LineMath.FindCoPlanarIntersectionPoint(this, otherLine);
        }

        public void DrawDebugView(float duration, Color color)
        {
            Debug.DrawLine(start, end, color, duration);
        }

        public Line2 OffsetLine(float distance)
        {
            return LineMath.OffsetLine(this, distance);
        }
    }

}
