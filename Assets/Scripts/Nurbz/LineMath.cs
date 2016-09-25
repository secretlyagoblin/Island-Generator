using UnityEngine;
using System.Collections;

namespace Nurbz
{
    public static class LineMath
    {
        //Vector 3

        public static Vector3 FindCoPlanarIntersectionPoint(Line3 lineA, Line3 lineB)
        {
            //if (Mathf.Abs(lineA.start.y- lineB.start.y) <0.001f)
            //{
            //    throw new System.Exception("Lines are not coplanar");
            //}

            // Get A,B,C of first line
            var a1 = lineA.end.z - lineA.start.z;
            var b1 = lineA.start.x - lineA.end.x;
            var c1 = a1*lineA.start.x + b1*lineA.start.z;

            var a2 = lineB.end.z - lineB.start.z;
            var b2 = lineB.start.x - lineB.end.x;
            var c2 = a2 * lineB.start.x + b2 * lineB.start.z;

            // Get delta and check if the lines are parallel
            var delta = a1 * b2 - a2 * b1;
            if(delta == 0)
            {
                //throw new System.Exception("Lines are parallel");
                return new Line3(lineA.middle, lineB.middle).middle;
            }

            var x = (b2 * c1 - b1 * c2) / delta;
            var z = (a1 * c2 - a2 * c1) / delta;

            return new Vector3(x, 0, z);
        }

        public static Line3 OffsetLine(Line3 line, float distance)
        {
            var orientation = line.orientation;
            //Debug.Log("Line Orientation: " + orientation);
            
            var perp = new Vector3(orientation.z,0, -orientation.x);
            //Debug.DrawRay(line.start, perp, Color.blue);
            var offset = Vector3.Scale(perp, new Vector3(distance, distance, distance));
            //Debug.DrawLine(line.start, line.start + offset);
            //Debug.DrawLine(line.end, line.end + offset);
            //Debug.DrawLine(line.start + offset, line.end + offset,Color.red);

            return new Line3(line.start + offset, line.end + offset);
        }

        public static Vector3 LineToVector3(Vector3 start, Vector3 end)
        {
            return (start - end).normalized;
        }

        public static Vector3 crossByInverseInPlane(Vector3 vector)
        {
            var inverse = new Vector3(vector.y, 0, -vector.z);
            return Vector3.Cross(vector, inverse);
        }

        public static Vector3 LineToCrossVector3(Vector3 start, Vector3 end)
        {

            var oldVec = LineToVector3(start, end);

            var x1 = oldVec.x;
            var y1 = oldVec.x;
            var z1 = oldVec.x;

            var x2 = 1.5f;
            var y2 = 0.5f;
            var z2 = (-x1 * x2 - y1 * y2) / z1;



            return new Vector3(x2, y2, z2);
        }

        public static Vector3 VectorToCrossVector3(Vector3 vector)
        {

            var oldVec = vector;

            var x1 = oldVec.x;
            var y1 = oldVec.x;
            var z1 = oldVec.x;

            var x2 = 1.5f;
            var y2 = 0.5f;
            var z2 = (-x1 * x2 - y1 * y2) / z1;



            return new Vector3(x2, y2, z2);
        }

        //Vector 2

        public static Vector2 FindCoPlanarIntersectionPoint(Line2 lineA, Line2 lineB)
        {
            //if (Mathf.Abs(lineA.start.y- lineB.start.y) <0.001f)
            //{
            //    throw new System.Exception("Lines are not coplanar");
            //}

            // Get A,B,C of first line
            var a1 = lineA.end.y - lineA.start.y;
            var b1 = lineA.start.x - lineA.end.x;
            var c1 = a1 * lineA.start.x + b1 * lineA.start.y;

            var a2 = lineB.end.y - lineB.start.y;
            var b2 = lineB.start.x - lineB.end.x;
            var c2 = a2 * lineB.start.x + b2 * lineB.start.y;

            // Get delta and check if the lines are parallel
            var delta = a1 * b2 - a2 * b1;
            if (delta == 0)
            {
                //throw new System.Exception("Lines are parallel");
                return new Line2(lineA.middle, lineB.middle).middle;
            }

            var x = (b2 * c1 - b1 * c2) / delta;
            var y = (a1 * c2 - a2 * c1) / delta;

            return new Vector2(x, y);
        }

        public static Line2 OffsetLine(Line2 line, float distance)
        {
            var orientation = line.orientation;
            //Debug.Log("Line Orientation: " + orientation);

            var perp = new Vector2(orientation.y, -orientation.x);
            //Debug.DrawRay(line.start, perp, Color.blue);
            var offset = Vector2.Scale(perp, new Vector2(distance, distance));
            //Debug.DrawLine(line.start, line.start + offset);
            //Debug.DrawLine(line.end, line.end + offset);
            //Debug.DrawLine(line.start + offset, line.end + offset,Color.red);

            return new Line2(line.start + offset, line.end + offset);
        }
    }


}
