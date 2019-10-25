using System;
using System.Collections;
using System.Collections.Generic;
using MeshMasher;
using UnityEngine;

namespace RecursiveHex
{
    public struct Hex
    {
        public readonly Vector2Int Index;
        public HexPayload Payload;
        public string DebugData;

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
        public Hex(Vector2Int index, HexPayload payload, string debugData = "")
        {
            Index = index;
            Payload = payload;
            DebugData = debugData;
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
            return new Vector2(Index.x + Hex.HalfHex * Mathf.Cos(angle_rad),
                         (Index.y * Hex.ScaleY) + Hex.HalfHex * Mathf.Sin(angle_rad));
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
            return new Vector3(Index.x + Hex.HalfHex * Mathf.Cos(-angle_rad),0,
                         (Index.y * Hex.ScaleY) + Hex.HalfHex * Mathf.Sin(-angle_rad));
        }
    }

    public struct HexPayload
    {
        public float Height;
        public Color Color;

        public static HexPayload Blerp(Hex a, Hex b, Hex c, Vector3 weights)
        {
            return new HexPayload()
            {
                Height = Utils.Blerp(a.Payload.Height, b.Payload.Height, c.Payload.Height, weights),
                Color = Utils.Blerp(a.Payload.Color, b.Payload.Color, c.Payload.Color, weights),
            };
        }

        public void PopulatePayloadObject(PayloadData data)
        {
            data.KeyValuePairs =
                new Dictionary<string, object>()
                {
                    {"Height",Height },
                     {"Color",Color }
                };
        }
    }
}