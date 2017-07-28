using UnityEngine;
using System.Collections.Generic;

namespace Maps {

    public class PhysicalMap {

        Map _map;

        public Map ToMap()
        {
            return _map;
        }

        protected Rect _rect;

        protected Vector2 _bottomLeft;
        protected Vector2 _topLeft;
        protected Vector2 _topRight;
        protected Vector2 _bottomRight;

        protected NumberRange _xRange;
        protected NumberRange _yRange;

        protected Vector2 _size;

        public PhysicalMap(Map map, Rect rect)
        {

            _map = map;

            _rect = rect;
            _bottomLeft = rect.position;
            _topLeft = new Vector2(_rect.xMax, rect.position.y);
            _bottomRight = new Vector2(rect.position.x, _rect.yMax);
            _topRight = _rect.max;

            _xRange = new NumberRange(rect.position.x, _topRight.x);
            _yRange = new NumberRange(rect.position.y, _topRight.y);
        }

        public bool Overlaps(PhysicalMap other)
        {
            return _rect.Overlaps(other._rect);
        }

        public void DrawRect(Color color)
        {
            Debug.DrawLine(_topLeft, _topRight, color);
            Debug.DrawLine(_topRight, _bottomRight, color);
            Debug.DrawLine(_bottomRight, _bottomLeft, color);
            Debug.DrawLine(_bottomLeft, _topLeft, color);
        }

        public void DrawShape(Collider2D collider, float paintValue)
        {
            for (int x = 0; x < _map.SizeX; x++)
            {
                for (int y = 0; y < _map.SizeY; y++)
                {
                    var pos = ArrayIndexToWorldContext(x, y);

                    if (collider.OverlapPoint(pos))
                        _map[x, y] = paintValue;
                }
            }
        }

        public PhysicalMap DrawLine(Vector2 a, Vector2 b, int thickness, float value)
        {
            _map.DrawLine(CoordFromWorldContext(a), CoordFromWorldContext(b),thickness,value);
            return this;

        }

        Vector2 ArrayIndexToWorldContext(int x, int y)
        {

            var point = _map.GetNormalisedVector3FromIndex(x, y);

            return new Vector2(_xRange.Lerp(point.x), _yRange.Lerp(point.z));
        }

        Vector2 NormalisedVectorFromWorldContext(Vector2 vector)
        {

            vector = new Vector2(_xRange.InverseLerp(vector.x), _yRange.InverseLerp(vector.y));
            return vector;
        }

        Coord CoordFromWorldContext(Vector2 vector)
        {
            int a = Mathf.RoundToInt(_xRange.InverseLerp(vector.x) * _map.SizeX);
            int b = Mathf.RoundToInt(_yRange.InverseLerp(vector.y) * _map.SizeY);
            return new Coord(a, b);
        }

        // Exposed public transformations

        delegate float DataTransformation(float a, float b);

        public PhysicalMap Add(PhysicalMap other)
        {

            if (!Overlaps(other))
                return this;

            var boundsA = new NormalisedRectArray(this, other);

            PerformBilinearFunction(boundsA, other, Add);

            return this;
        }

        public PhysicalMap Subtract(PhysicalMap other)
        {

            if (!Overlaps(other))
                return this;

            var boundsA = new NormalisedRectArray(this, other);

            PerformBilinearFunction(boundsA, other, Subtract);

            return this;
        }

        public PhysicalMap Average(PhysicalMap other)
        {

            if (!Overlaps(other))
                return this;

            var boundsA = new NormalisedRectArray(this, other);

            PerformBilinearFunction(boundsA, other, Average);

            return this;
        }

        public static Rect GetOverlappingRect(PhysicalMap a, PhysicalMap b)
        {

            if (!a.Overlaps(b))
                return new Rect();

            var xRange = NumberRange.GetOverlappingBounds(a._xRange, b._xRange);
            var yRange = NumberRange.GetOverlappingBounds(a._yRange, b._yRange);

            return new Rect(new Vector2(xRange.Min, yRange.Min), new Vector2(xRange.Size, yRange.Size));
        }

        void PerformBilinearFunction(NormalisedRectArray bounds, PhysicalMap mapB, DataTransformation transformationToApply)
        {

            var sizeX = _map.SizeX;
            var xMin = Mathf.RoundToInt(sizeX * bounds.XBounds.Min);
            var xMax = Mathf.RoundToInt(sizeX * bounds.XBounds.Max);

            var sizeY = _map.SizeY;
            var yMin = Mathf.RoundToInt(sizeY * bounds.YBounds.Min);
            var yMax = Mathf.RoundToInt(sizeY * bounds.YBounds.Max);

            //Debug.Log("X Sub-Array Bounds: " + xMin + ", " + xMax);
            //Debug.Log("Y Sub-Array Bounds: " + yMin + ", " + yMax);

            var returnArray = new float[xMax - xMin, yMax - yMin];

            for (int x = xMin; x < xMax; x++)
            {
                for (int y = yMin; y < yMax; y++)
                {

                    var point = ArrayIndexToWorldContext(x, y);
                    var otherPoint = mapB.NormalisedVectorFromWorldContext(point);

                    _map[x, y] = transformationToApply(_map[x, y], mapB._map.BilinearSampleFromNormalisedVector2(otherPoint));
                }
            }
        }

        float Add(float a, float b)
        {
            return a + b;
        }

        float Subtract(float a, float b)
        {
            return a - b;
        }

        float Average(float a, float b)
        {
            return (a + b) * 0.5f;
        }

        protected struct NumberRange {

            public float Min;
            public float Max;

            public float Size { get { return Max - Min; } }

            public NumberRange(float min, float max)
            {
                Min = min;
                Max = max;
            }

            public float InverseLerp(float value)
            {
                return Mathf.InverseLerp(Min, Max, value);
            }

            public float Lerp(float value)
            {
                return Mathf.Lerp(Min, Max, value);
            }


            static public NumberRange GetOverlappingBounds(NumberRange a, NumberRange b)
            {

                NumberRange left;
                NumberRange right;

                if (a.Min < b.Min)
                {
                    left = a;
                    right = b;
                }
                else
                {
                    left = b;
                    right = a;
                }

                var newMin = right.Min;
                var newMax = left.Max < right.Max ? left.Max : right.Max;

                return new NumberRange(newMin, newMax);
            }
        }

        protected struct NormalisedRectArray {

            public NumberRange XBounds;
            public NumberRange YBounds;

            public NormalisedRectArray(PhysicalMap parent, PhysicalMap other)
            {

                var xRange = NumberRange.GetOverlappingBounds(parent._xRange, other._xRange);
                var yRange = NumberRange.GetOverlappingBounds(parent._yRange, other._yRange);

                XBounds = new NumberRange(parent._xRange.InverseLerp(xRange.Min), parent._xRange.InverseLerp(xRange.Max));
                YBounds = new NumberRange(parent._yRange.InverseLerp(yRange.Min), parent._yRange.InverseLerp(yRange.Max));

                //Debug.Log("X Bounds: " + XBounds.Min + ", " + XBounds.Max);
                //Debug.Log("Y Bounds: " + YBounds.Min + ", " + YBounds.Max);
            }




        }
    }
}