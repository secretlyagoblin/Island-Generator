﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecursiveHex
{

    public struct HexIndex
    {
        public readonly Vector3Int Index3d;

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

        public HexIndex(int x, int y, int z)
        {
            Index3d = new Vector3Int(x, y, z);
        }

        public HexIndex(Vector3Int vector3Int) : this()
        {
            Index3d = vector3Int;
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
            var isOdd = index2d.y % 2 != 0;

            return new Vector2(
                index2d.x - (isOdd ? 0 : 0.5f),
                index2d.y * Hex.ScaleY);
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
            new HexIndex(0,-1,+1),
            new HexIndex(+1,-1,0),
            new HexIndex(+1,0,-1),
            new HexIndex(0,+1,-1),
            new HexIndex(-1,+1,0),
            new HexIndex(-1,0,+1),

        };

        public HexIndex[] GenerateRing(int radius)
        {
            var results = new List<HexIndex>();

            var currentPos = this + (_directions[4] * radius);
            var lastPos = currentPos;
            currentPos = currentPos + (_directions[0] * Mathf.FloorToInt(radius * 0.5f));

            var ringStart = currentPos;

            var i = 0;
            var count = 0;

            while (true)
            {
                i = i < 5 ? (i + 1) : 0;

                for (int j = (count == 0 ? Mathf.FloorToInt(radius * 0.5f) : 0); j < radius; j++)
                {

                    //Debug.Log($"Added Cell {currentPos}");
                    results.Add(currentPos);
                    currentPos += _directions[i];
                    Debug.DrawLine(lastPos.Position3d, currentPos.Position3d, Color.green * 0.5f, 100f);
                    lastPos = currentPos;
                    count++;

                    if (ringStart == currentPos)
                        return results.ToArray();
                }
            }
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