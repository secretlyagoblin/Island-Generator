using System.Collections;
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
                index2d.x + (isOdd ? 0 : 0.5f),
                index2d.y * Hex.ScaleY);
        }

        public HexIndex Rotate60()
        {
            return new HexIndex(-Index3d.y, -Index3d.z, -Index3d.x);
        }

        public static HexIndex NestMultiply(HexIndex index, int amount)
        {
            var newIndex = index.Index3d * (amount + 1) + (index.Rotate60().Index3d * amount);

            return new HexIndex(newIndex.x, newIndex.y, newIndex.z);
        }

        public HexIndex NestMultiply(int amount)
        {
            return NestMultiply(this, amount);
        }

        public HexIndex[] GenerateRosette(int radius)
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
    }
}