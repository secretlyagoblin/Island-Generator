﻿using System.Collections.Generic;
using UnityEngine;

namespace WanderingRoad.Procgen.RecursiveHex
{

    public struct HexIndex
    {
        public readonly Vector3Int Index3d;

        public override string ToString()
        {
            return Index3d.ToString();
        }

        public static readonly float ScaleY = Mathf.Sqrt(3f) * 0.5f;
        public static readonly float HalfHex = 0.5f / Mathf.Cos(Mathf.PI / 180f * 30);

        public Vector2Int Index2d { get { return Get2dIndex(Index3d); } }
        public Vector2 Position2d { get { return GetPosition(Index2d); } }

        public Vector3 Position3d
        {
            get
            {
                var twoD = GetPosition(Index2d);
                return new Vector3(twoD.x, 0, twoD.y);
            }
        }

        public Rect Bounds
        {
            get {
                var pos = Position2d;
                var size = new Vector2(1f, HalfHex * 2);
                var halfSize = size * 0.5f;
                pos -= halfSize;

                var rect = new Rect(pos, size);
                //rect.DrawRect(Color.green, 100f);

                return rect;            
            }
        }

        public HexIndex(int x, int y, int z)
        {
            Index3d = new Vector3Int(x, y, z);
        }

        public HexIndex(Vector3Int vector3Int) : this()
        {
            Index3d = vector3Int;
        }

        public HexIndex(Vector3 vector3) : this()
        {
            Index3d = RoundCube(vector3);
        }

        public static Vector3Int Get3dIndex(Vector2Int index2d)
        {
            var x = index2d.x - (index2d.y - (index2d.y & 1)) / 2;
            var z = index2d.y;
            var y = -x - z;
            return new Vector3Int(x, y, z);
        }

        public static Vector2Int Get2dIndex(Vector3Int index3d)
        {
            var col = index3d.x + (index3d.z - (index3d.z & 1)) / 2;
            var row = index3d.z;
            return new Vector2Int(col, row);
        }

        public static Vector2 GetPosition(Vector2Int index2d)
        {
            var x = HalfHex * Mathf.Sqrt(3) * (index2d.x + 0.5f * (index2d.y & 1));
            var y = HalfHex * 3 / 2 * index2d.y;
            return new Vector2(x, y);
        }

        public static Vector3 FloatingHexFromPosition(float x, float y)
        {
            var q = (Mathf.Sqrt(3f) / 3f * x - 1f / 3f * y) / HalfHex;
            var r = (2f / 3f * y) / HalfHex;

            var cube = new Vector3(q, -q - r, r);

            return cube;
        }


        public static HexIndex HexIndexFromPosition(float x, float y)
        {
            var floatingHex = FloatingHexFromPosition(x, y);

            var roundCube = RoundCube(floatingHex);

            return new HexIndex(roundCube);
        }

        public static HexIndex HexIndexFromPosition(Vector2 loc2d)
        {
            return HexIndexFromPosition(loc2d.x, loc2d.y);
        }

        public HexIndex Rotate60()
        {
            return new HexIndex(-Index3d.y, -Index3d.z, -Index3d.x);
        }

        public HexIndex NestMultiply(int amount)
        {
            var newIndex = Index3d * (amount + 1) + (Rotate60().Index3d * amount);
            return new HexIndex(newIndex.x, newIndex.y, newIndex.z);
        }

        public static Matrix4x4 GetInverseMultiplicationMatrix(int amount)
        {
            return Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(60*amount,Vector3.up), new Vector3(amount + 1, 1, amount + 1)).inverse;
        }

        public HexIndex[] GenerateRosetteLinear(int radius)
        {
            //calculate rosette size without any GC :(

            var count = 0;

            for (int q = -radius; q <= radius; q++)
            {
                int r1 = Mathf.Max(-radius, -q - radius);
                int r2 = Mathf.Min(radius, -q + radius);

                for (int r = r1; r <= r2; r++)
                {
                    count++;
                }
            }

            //Do the whole thing again this time making an array            

            var output = new HexIndex[count];

            count = 0;

            for (int q = -radius; q <= radius; q++)
            {
                int r1 = Mathf.Max(-radius, -q - radius);
                int r2 = Mathf.Min(radius, -q + radius);
                for (int r = r1; r <= r2; r++)
                {
                    var vec = new Vector3Int(q, r, -q - r) + this.Index3d;
                    output[count] = new HexIndex(vec.x, vec.y, vec.z);
                    count++;
                }

            }

            return output;
        }

        public HexIndex[] GenerateRosetteCircular(int radius)
        {
            var results = new List<HexIndex>() { this };

            for (int i = 1; i < radius; i++)
            {
                results.AddRange(GenerateRing(i));
            }

            return results.ToArray();
        }

        private static readonly HexIndex[] _directions = new HexIndex[]
        {
            new HexIndex(1,-1,0),
            new HexIndex(0,-1,1),
            new HexIndex(-1,0,1),
            new HexIndex(-1,1,0),
            new HexIndex(0,1,-1),
            new HexIndex(1,0,-1)
        };

        public HexIndex[] GenerateRing(int radius)
        {
            var resultsCount = radius == 0 ? 1 : (radius ) * 6;

            var results = new HexIndex[resultsCount];

            var currentPos = this + (_directions[4] * radius);
            //var lastPos = currentPos;
            currentPos += _directions[0] * Mathf.FloorToInt(radius * 0.5f);

            var ringStart = currentPos;

            var i = 0;
            var count = 0;

            while (true)
            {        
                for (int j = (count == 0 ? Mathf.FloorToInt(radius * 0.5f) : 0); j < radius; j++)
                {

                    //Debug.Log($"Added Cell {currentPos}");
                    results[count] = (currentPos);
                    currentPos += _directions[i];
                    //Debug.DrawLine(lastPos.Position3d, currentPos.Position3d, Color.green * 0.5f, 100f);
                    //lastPos = currentPos;
                    count++;

                    if (ringStart == currentPos)
                        return results;
                }

                i = i < 5 ? (i + 1) : 0;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is HexIndex index &&
                   Index3d.Equals(index.Index3d) &&
                   Index2d.Equals(index.Index2d) &&
                   Position2d.Equals(index.Position2d) &&
                   Position3d.Equals(index.Position3d);
        }

        public override int GetHashCode()
        {
            var hashCode = -1712539406;
            hashCode = hashCode * -1521134295 + Index3d.GetHashCode();
            hashCode = hashCode * -1521134295 + Index2d.GetHashCode();
            hashCode = hashCode * -1521134295 + Position2d.GetHashCode();
            hashCode = hashCode * -1521134295 + Position3d.GetHashCode();
            return hashCode;
        }


        static int CubeDistance(HexIndex a, HexIndex b)
        {
            return Mathf.Max(Mathf.Abs(a.Index3d.x - b.Index3d.x), Mathf.Abs(a.Index3d.y - b.Index3d.y), Mathf.Abs(a.Index3d.z - b.Index3d.z));
        }

        static Vector3Int RoundCube(Vector3 cube)
        {
            var rx = Mathf.RoundToInt(cube.x);
            var ry = Mathf.RoundToInt(cube.y);
            var rz = Mathf.RoundToInt(cube.z);

            var x_diff = Mathf.Abs(rx - cube.x);
            var y_diff = Mathf.Abs(ry - cube.y);
            var z_diff = Mathf.Abs(rz - cube.z);

            if (x_diff > y_diff && x_diff > z_diff)
            {

                rx = -ry - rz;
            }
            else if (y_diff > z_diff)
            {
                ry = -rx - rz;
            }
            else
            {
                rz = -rx - ry;
            }

            return new Vector3Int(rx, ry, rz);
        }

        public static HexIndex[] DrawOrientedLineWithHole(HexIndex a, HexIndex b)
        {
            HexIndex[] line;

            if (a.Position2d.y > b.Position2d.y)
            {
                line =  DrawLine(a, b);
            }
            else
            {
                line = DrawLine(b, a);
            }

            if (line.Length <= 1) return line;

            var midPoint = Mathf.FloorToInt(line.Length * 0.5f)-1;
            var count = 0;
            var outline = new HexIndex[line.Length - 1];

            for (int i = 0; i < line.Length; i++)
            {
                try
                {
                    outline[count] = line[i];

                    if (i != midPoint) count++;
                }
                catch
                {
                    throw new System.Exception("Dugg");
                }
            }

            return new HexIndex[0];       
        }

        public static HexIndex[] DrawOrientedLine(HexIndex a, HexIndex b)
        {
            if (a.Position2d.y > b.Position2d.y)
            {
                return DrawLine(a, b);
            }

            return DrawLine(b, a);
        }

        public int DistanceTo(HexIndex other)
        {
            return CubeDistance(this, other);
        }

        public static HexIndex[] DrawLine(HexIndex a, HexIndex b)
        {
            if(a == b)
            {
                return new HexIndex[] { a };
            }

            var N = CubeDistance(a, b);
            var results = new HexIndex[N+1];
            for (int i = 0; i < N+1; i++)
            {
                results[i] = 
                    new HexIndex(
                        RoundCube(
                            Vector3.Lerp(a.Index3d, b.Index3d, 1.0f / N * i)));
            }

            return results;
        }

        public static HexIndex operator +(HexIndex a, HexIndex b)
        {
            return new HexIndex(a.Index3d.x + b.Index3d.x, a.Index3d.y + b.Index3d.y, a.Index3d.z + b.Index3d.z);
        }

        public static HexIndex operator -(HexIndex a, HexIndex b)
        {
            return new HexIndex(a.Index3d.x - b.Index3d.x, a.Index3d.y - b.Index3d.y, a.Index3d.z - b.Index3d.z);
        }

        public static HexIndex operator *(HexIndex a, int b)
        {
            return new HexIndex(a.Index3d.x * b, a.Index3d.y * b, a.Index3d.z * b);
        }

        public static HexIndex operator *(HexIndex a, HexIndex b)
        {
            return new HexIndex(a.Index3d.x * b.Index3d.x, a.Index3d.y * b.Index3d.y, a.Index3d.z * b.Index3d.z);
        }

        public static bool operator ==(HexIndex a, HexIndex b)
        {
            return a.Index3d.x == b.Index3d.x && a.Index3d.y == b.Index3d.y && a.Index3d.z == b.Index3d.z;
        }

        public static bool operator !=(HexIndex a, HexIndex b)
        {
            return !(a == b);
        }


    }
}