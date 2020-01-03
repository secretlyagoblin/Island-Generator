﻿using System;
using System.Collections;
using System.Collections.Generic;
using MeshMasher;
using UnityEngine;

namespace RecursiveHex
{
    public struct Hex
    {
        public HexIndex Index;
        public HexPayload Payload;
        public string DebugData;

        public bool IsBorder;

        private bool _notNull;

        public static readonly float ScaleY = Mathf.Sqrt(3f) * 0.5f;
        public static readonly float HalfHex = 0.5f/Mathf.Cos(Mathf.PI/180f*30);

        public static readonly Vector2[] StaticFlatHexPoints = new Vector2[]
        {
            GetStaticFlatCornerXY(0),
            GetStaticFlatCornerXY(1),
            GetStaticFlatCornerXY(2),
            GetStaticFlatCornerXY(3),
            GetStaticFlatCornerXY(4),
            GetStaticFlatCornerXY(5)
        };

        /// <summary>
        /// Create a new hex just from the XY. Will need to be expanded later.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Hex(HexIndex index, HexPayload payload, bool isBorder, string debugData = "")
        {
            Index = index;
            Payload = payload;
            DebugData = debugData;
            IsBorder = isBorder;

            _notNull = true;
        }

        public Hex(Hex hex, bool isBorder)
        {
            Index = hex.Index;
            Payload = hex.Payload;
            DebugData = hex.DebugData;
            IsBorder = isBorder;

            _notNull = true;
        }

        public static Hex InvalidHex
        {
            get {
                return new Hex()
                {
                    IsBorder = true,
                    _notNull = false
                };
            }
        }

        public static bool IsInvalid(Hex hex)
        {
            return !hex._notNull;
        }

        /// <summary>
        /// Get the nth corner of the hexagon on the XY Plane
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Vector2 GetPointyCornerXY(int i)
        {
            var angle_deg = 60f * i - 30f;
            var angle_rad = Mathf.PI / 180f * angle_deg;
            return new Vector2(Index.Index2d.x + Hex.HalfHex * Mathf.Cos(angle_rad),
                         (Index.Index2d.y * Hex.ScaleY) + Hex.HalfHex * Mathf.Sin(angle_rad));
        }

        /// <summary>
        /// Get the nth corner of the hexagon on the XY Plane
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static Vector2 GetStaticPointyCornerXY(int i)
        {
            var angle_deg = 60f * i - 30f;
            var angle_rad = Mathf.PI / 180f * angle_deg;
            return new Vector2(Mathf.Cos(angle_rad),
                         Mathf.Sin(angle_rad));
        }

        /// <summary>
        /// Get the nth corner of the hexagon on the XY Plane
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static Vector2 GetStaticFlatCornerXY(int i)
        {
            var angle_deg = 60f * i;
            var angle_rad = Mathf.PI / 180f * angle_deg;
            return new Vector2(Mathf.Cos(angle_rad),
                         Mathf.Sin(angle_rad));
        }

        private const float MAGIC_INNER_ROTATION = 19.106605350869f;

        /// <summary>
        /// Get the nth corner of the hexagon on the XY Plane
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static Vector2 GetStaticInnerCornerXY(int i)
        {
            var angle_deg = 60f * i - 30f - MAGIC_INNER_ROTATION;
            var angle_rad = Mathf.PI / 180f * angle_deg;
            return new Vector2(Mathf.Cos(angle_rad),
                         Mathf.Sin(angle_rad));
        }

        /// <summary>
        /// Get the nth corner of the hexagon  on the XZ plane
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Vector3 GetPointyCornerXZ(int i)
        {
            var angle_deg = 60f * i - 30f;
            var angle_rad = Mathf.PI / 180f * angle_deg;
            return new Vector3(Index.Index2d.x + Hex.HalfHex * Mathf.Cos(-angle_rad),0,
                         (Index.Index2d.y * Hex.ScaleY) + Hex.HalfHex * Mathf.Sin(-angle_rad));
        }
    }

    public struct HexIndex
    {
        public readonly Vector3Int Index;
        public readonly Vector2Int Index2d;
        public readonly Vector2 Position;

        public HexIndex(int x, int y, int z)
        {
            Index = new Vector3Int(x, y, z);
            Index2d = Get2dIndex(Index);
            Position = GetPosition(Index2d);
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
                index2d.x + (isOdd?0:0.5f),
                index2d.y * Hex.ScaleY);
        }

        public HexIndex Rotate60()
        {
            return new HexIndex(-Index.y, -Index.z, -Index.x);
        }

        public static HexIndex NestMultiply (HexIndex index, int amount)
        {
            var newIndex = index.Index * (amount + 1) + (index.Rotate60().Index * amount);

            return new HexIndex(newIndex.x,newIndex.y,newIndex.z);
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

            count = 0;

            var output = new HexIndex[count];

                    for (int q = -radius; q <= radius; q++)
            {
                int r1 = Mathf.Max(-radius, -q - radius);
                int r2 = Mathf.Min(radius, -q + radius);
                for (int r = r1; r <= r2; r++)
                {
                    var vec = new Vector3Int(q, r, -q - r) + this.Index;
                    output[count] = new HexIndex(vec.x,vec.y,vec.z);
                    count++;
                }

            }

            return output;
        }
    }



}