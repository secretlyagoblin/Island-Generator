using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buckets {

    class Bucket<T> {

        public Rect Rect;
        public Vector2 LowerBounds;
        public Vector2 UpperBounds;

        public bool CurrentIteration = false;
        public bool PreviousIteration = false;

        List<BucketData> ElementList;
        public List<T> Elements;

        public List<Bucket<T>> Buckets;

        bool Filled = false;

        int _layer;
        int _maxLayer = 6;

        public Bucket(int maxLayer, Vector2 lowerBounds, Vector2 upperBounds)
        {
            Rect = new Rect(lowerBounds, upperBounds - lowerBounds);
            _layer = 0;
            _maxLayer = maxLayer;

            var Colour = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f));

            LowerBounds = lowerBounds;
            UpperBounds = upperBounds;

            Filled = false;
            ElementList = new List<BucketData>();
            Elements = new List<T>();
            Buckets = new List<Bucket<T>>();
        }

        Bucket(int layer, int maxLayer, Vector2 lowerBounds, Vector2 upperBounds)
        {
            Rect = new Rect(lowerBounds, upperBounds - lowerBounds);
            _layer = layer;
            _maxLayer = maxLayer;

            var Colour = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f));

            LowerBounds = lowerBounds;
            UpperBounds = upperBounds;

            Filled = false;
            ElementList = new List<BucketData>();
            Elements = new List<T>();
            Buckets = new List<Bucket<T>>();
        }

        bool IsIn(Vector2 test)
        {
            return Rect.Contains(test);
        }

        public void AddElement(T element, Vector2 position)
        {
            if (!Filled)
            {
                ElementList.Add(new BucketData(element, position));
                Elements.Add(element);
                if (ElementList.Count > 1 && _layer < _maxLayer)
                {
                    var CellSize = UpperBounds - LowerBounds;
                    CellSize = CellSize * 0.5f;

                    var startA = LowerBounds;
                    var startB = LowerBounds + new Vector2(CellSize.x, 0);
                    var startC = LowerBounds + new Vector2(0, CellSize.y);
                    var startD = LowerBounds + new Vector2(CellSize.x, CellSize.y);

                    Buckets.Add(new Bucket<T>(_layer + 1, _maxLayer, startA, startA + CellSize));
                    Buckets.Add(new Bucket<T>(_layer + 1, _maxLayer, startB, startB + CellSize));
                    Buckets.Add(new Bucket<T>(_layer + 1, _maxLayer, startC, startC + CellSize));
                    Buckets.Add(new Bucket<T>(_layer + 1, _maxLayer, startD, startD + CellSize));

                    for (int i = 0; i < ElementList.Count; i++)
                    {
                        for (int x = 0; x < Buckets.Count; x++)
                        {
                            if (Buckets[x].IsIn(ElementList[i].Position))
                            {
                                Buckets[x].AddElement(ElementList[i].Element, ElementList[i].Position);
                                goto End;

                            }
                        }
                        End:
                        ;
                    }
                    Filled = true;
                    ElementList.Clear();
                    Elements.Clear();
                }
            }
            else
            {
                for (int x = 0; x < Buckets.Count; x++)
                {
                    if (Buckets[x].IsIn(position))
                    {
                        Buckets[x].AddElement(element, position);
                        return;
                    }
                }
            }
            return;
        }

        public List<Bucket<T>> GetBucketsWithinRangeOfPoint(Vector2 testPoint, float testDistance)
        {
            var distance = DistancePointToRectangle(testPoint, Rect);
            var allBuckets = new List<Bucket<T>>();

            if (distance < testDistance)
            {
                if (Filled)
                {
                    for (int i = 0; i < Buckets.Count; i++)
                    {
                        allBuckets.AddRange(Buckets[i].GetBucketsWithinRangeOfPoint(testPoint, testDistance));
                    }
                }
                else
                {
                    CurrentIteration = true;
                    allBuckets.Add(this);
                }
            }

            return allBuckets;
        }

        public List<Bucket<T>> GetPartialTree(Rect testRect, int depth)
        {
            return GetPartialTree(testRect, depth, 0);
        }

        List<Bucket<T>> GetPartialTree(Rect testRect, int depth, int currentDepth)
        {
            var overlap = Rect.Overlaps(testRect);
            var allBuckets = new List<Bucket<T>>();

            if (overlap)
            {
                if (currentDepth != depth)
                {
                    for (int i = 0; i < Buckets.Count; i++)
                    {
                        allBuckets.AddRange(Buckets[i].GetPartialTree(testRect, depth, currentDepth++));
                    }
                }
                else
                {
                    CurrentIteration = true;
                    allBuckets.Add(this);
                }
            }

            return allBuckets;
        }

        static float DistancePointToRectangle(Vector2 point, Rect rect)
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

        struct BucketData {
            public T Element;
            public Vector2 Position;

            public BucketData(T element, Vector2 position)
            {
                Element = element;
                Position = position;
            }
        }
    }
}