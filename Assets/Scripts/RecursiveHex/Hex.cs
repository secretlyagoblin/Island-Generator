using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecursiveHex
{
    public struct Hex
    {
        public float Height;
        public Vector2Int Index;
        public int Code;

        public static readonly float ScaleY = 0.866025f;
        public static readonly float HalfHex = 0.55f;

        /// <summary>
        /// Create a new hex just from the XY. Will need to be expanded later.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Hex(int x, int y)
        {
            Index = new Vector2Int(x, y);
            Height = 0;
            Code = 0;
        }

        /// <summary>
        /// Get the nth corner of the hexagon on the XY Plane
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Vector2 GetCornerXY(int i)
        {
            var angle_deg = 60f * i - 30f;
            var angle_rad = Mathf.PI / 180f * angle_deg;
            return new Vector2(Index.x + Hex.HalfHex * Mathf.Cos(angle_rad),
                         (Index.y * Hex.ScaleY) + Hex.HalfHex * Mathf.Sin(angle_rad));
        }

        /// <summary>
        /// Get the nth corner of the hexagon  on the XZ plane
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Vector3 GetCornerXZ(int i)
        {
            var angle_deg = 60f * i - 30f;
            var angle_rad = Mathf.PI / 180f * angle_deg;
            return new Vector3(Index.x + Hex.HalfHex * Mathf.Cos(-angle_rad), Height,
                         (Index.y * Hex.ScaleY) + Hex.HalfHex * Mathf.Sin(-angle_rad));
        }
    }
}