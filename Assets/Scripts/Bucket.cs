using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Bucket {
    public Rect Rect;
    public Vector2 LowerBounds;
    public Vector2 UpperBounds;

    public bool CurrentIteration = false;
    public bool PreviousIteration = false;

    public List<Vector3> Points;

    public List<Bucket> Buckets;

    bool Filled = false;


    public int Layer;
    public int MaxLayer = 12;

    public Bucket(int layer, Vector2 lowerBounds, Vector2 upperBounds)
    {
        Rect = new Rect(lowerBounds, upperBounds - lowerBounds);
        Layer = layer;

        var Colour = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f));

        LowerBounds = lowerBounds;
        UpperBounds = upperBounds;

        Filled = false;
        Points = new List<Vector3>();
        Buckets = new List<Bucket>();
    }

    public bool IsIn(Vector3 test)
    {
        return Rect.Contains(test);
    }

    public void AddPoint(Vector3 vector3)
    {
        if (!Filled)
        {
            Points.Add(vector3);
            if (Points.Count > 1 && Layer < MaxLayer)
            {
                var CellSize = UpperBounds - LowerBounds;
                CellSize = CellSize * 0.5f;

                var startA = LowerBounds;
                var startB = LowerBounds + new Vector2(CellSize.x, 0);
                var startC = LowerBounds + new Vector2(0, CellSize.y);
                var startD = LowerBounds + new Vector2(CellSize.x, CellSize.y);

                Buckets.Add(new Bucket(Layer + 1, startA, startA + CellSize));
                Buckets.Add(new Bucket(Layer + 1, startB, startB + CellSize));
                Buckets.Add(new Bucket(Layer + 1, startC, startC + CellSize));
                Buckets.Add(new Bucket(Layer + 1, startD, startD + CellSize));

                for (int i = 0; i < Points.Count; i++)
                {
                    for (int x = 0; x < Buckets.Count; x++)
                    {
                        if (Buckets[x].IsIn(Points[i]))
                        {
                            Buckets[x].AddPoint(Points[i]);
                            goto End;

                        }
                    }
                End:
                    ;
                }
                Filled = true;
                Points.Clear();
            }
        }
        else
        {
            for (int x = 0; x < Buckets.Count; x++)
            {
                if (Buckets[x].IsIn(vector3))
                {
                    Buckets[x].AddPoint(vector3);
                    return;
                }
            }
        }
        return;
    }

    public Bucket[] GetBuckets(Vector2 testPoint, float testDistance)
    {
        var distance = Bucket.DistancePointToRectangle(testPoint, Rect);
        var allBuckets = new List<Bucket>();

        if (distance < testDistance)
        {
            if (Filled)
            {
                for (int i = 0; i < Buckets.Count; i++)
                {
                    allBuckets.AddRange(Buckets[i].GetBuckets(testPoint, testDistance));
                }
            }
            else
            {
                CurrentIteration = true;
                allBuckets.Add(this);
            }
        }

        return allBuckets.ToArray();
    }

    public static float DistancePointToRectangle(Vector2 point, Rect rect)
    {
        //  Calculate a distance between a point and a rectangle.
        //  The area around/in the rectangle is defined in terms of
        //  several regions:
        //
        //  O--x
        //  |
        //  y
        //
        //
        //        I   |    II    |  III
        //      ======+==========+======   --yMin
        //       VIII |  IX (in) |  IV
        //      ======+==========+======   --yMax
        //       VII  |    VI    |   V
        //
        //
        //  Note that the +y direction is down because of Unity's GUI coordinates.

        if (point.x < rect.xMin)
        { // Region I, VIII, or VII
            if (point.y < rect.yMin)
            { // I
                Vector2 diff = point - new Vector2(rect.xMin, rect.yMin);
                return diff.magnitude;
            }
            else if (point.y > rect.yMax)
            { // VII
                Vector2 diff = point - new Vector2(rect.xMin, rect.yMax);
                return diff.magnitude;
            }
            else
            { // VIII
                return rect.xMin - point.x;
            }
        }
        else if (point.x > rect.xMax)
        { // Region III, IV, or V
            if (point.y < rect.yMin)
            { // III
                Vector2 diff = point - new Vector2(rect.xMax, rect.yMin);
                return diff.magnitude;
            }
            else if (point.y > rect.yMax)
            { // V
                Vector2 diff = point - new Vector2(rect.xMax, rect.yMax);
                return diff.magnitude;
            }
            else
            { // IV
                return point.x - rect.xMax;
            }
        }
        else
        { // Region II, IX, or VI
            if (point.y < rect.yMin)
            { // II
                return rect.yMin - point.y;
            }
            else if (point.y > rect.yMax)
            { // VI
                return point.y - rect.yMax;
            }
            else
            { // IX
                return 0f;
            }
        }
    }
}