using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecursiveHex
{
    public static class Utils
    {
        public static HexGroup[] ForEachHexGroup(this HexGroup[] array, Action<HexGroup> action)
        {
            for (int i = 0; i < array.Length; i++)
            {
                action(array[i]);
            }

            return array;
        }

        public static T[] ForEachHexGroup<T>(this HexGroup[] array, Func<HexGroup, T> action)
        {
            var output = new T[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                output[i] = action(array[i]);
            }

            return output;
        }

        /// <summary>
        /// The major offsetting function.
        /// </summary>
        /// <param name="localOffset"></param>
        /// <returns></returns>
        public static Vector2Int GetNestedHexIndexFromOffset(this Hex hex, Vector2Int localOffset)
        {

            var inUseIndex = hex.Index; //we'll be modifying this, so we make a local copy
            var evenY = inUseIndex.y % 2 == 0;
            var offsetX = inUseIndex.x * 3;
            var offsetY = inUseIndex.y * 3;

            offsetX += evenY ? 0 : 1;

            if (!evenY)
            {
                var offsetEvenY = localOffset.y % 2 == 0;

                offsetX += offsetEvenY ? 0 : 1;
            }

            var x = offsetX + localOffset.x;
            var y = offsetY + localOffset.y;

            return new Vector2Int(x, y);
        }

        private const float MAGIC_INNER_ROTATION = 0.33347317225183f;

        private static readonly float _scale = 1f / (Mathf.Sqrt(7)); //(1f / 3f);

        public static Vector2 GetNestedHexLocalCoordinateFromOffset(this Hex hex, Vector3Int offset)
        {
            var coord = hex.Index + offset;
            var coord2d = Hex.Get2dIndex(coord);

            var isOdd = coord2d.y % 2 != 0;
            var xOffset = isOdd ? 0.5f : 0f;
            var finalX = coord2d.x + xOffset;

            //https://stackoverflow.com/questions/13695317/rotate-a-point-around-another-point

            float cosTheta = Mathf.Cos(MAGIC_INNER_ROTATION);
            float sinTheta = Mathf.Sin(MAGIC_INNER_ROTATION);
            return new Vector2(           

                    (cosTheta * (finalX) - sinTheta * (coord2d.y * Hex.ScaleY)) * _scale,
                    (sinTheta * (finalX) + cosTheta * (coord2d.y * Hex.ScaleY))* _scale
            );
        }

        /// <summary>
        /// Gets a consistent vector within 0.5 when given a hex or a hex with an offset
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Vector2 GetNoiseOffset(this Hex hex)
        {
            return hex.GetNoiseOffset(Vector2Int.zero);
        }

        private const float NOISE_OFFSET_SCALE = 0.37f; //Any higher caused 1 or more barycenter errors

        /// <summary>
        /// Gets a consistent vector within 0.5 when given a hex or a hex with an offset
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Vector2 GetNoiseOffset(this Hex hex, Vector2Int offset)
        {
            var result = RandomSeedProperties.GetOffset(hex.Index.x + offset.x, hex.Index.y + offset.y);

            return new Vector2(
                Mathf.Sin(result.Angle) * result.Distance * NOISE_OFFSET_SCALE,
                Mathf.Cos(result.Angle) * result.Distance * NOISE_OFFSET_SCALE);
        }

        public static float Blerp(float a, float b, float c, Vector3 weight)
        {
            return a * weight.x + b * weight.y + c * weight.z;
        }

        public static Color Blerp(Color a, Color b, Color c, Vector3 weight)
        {
                var r = a.r * weight.x + b.r * weight.y + c.r * weight.z;
                var g = a.g * weight.x + b.g * weight.y + c.g * weight.z;
                var bee = a.b * weight.x + b.b * weight.y + c.b * weight.z;
        
                return new Color(r, g, bee);
        }

        public static T Blerp<T>(T a, T b, T c, Vector3 weight)
        {
            if (weight.x >= weight.y && weight.x >= weight.z)
            {
                return a;
            }
            else if (weight.y >= weight.z && weight.y >= weight.x)
            {
                return b;
            }
            else
            {
                return c;
            }
        }

        public static Vector3 GetNoiseOffset(this Vector3 vector3)
        {
            var result = RandomSeedProperties.GetOffset(vector3.x, vector3.z);

            return new Vector3(
                Mathf.Sin(result.Angle) * result.Distance * NOISE_OFFSET_SCALE,
                0,
                Mathf.Cos(result.Angle) * result.Distance * NOISE_OFFSET_SCALE);
        }

        public static Vector3 AddNoiseOffset(this Vector3 vector3)
        {
            return vector3 + vector3.GetNoiseOffset();

        }

        //public static Color Blerp(Color a, Color b, Color c, Vector3 weight)
        //{
        //    if (weight.x >= weight.y && weight.x >= weight.z)
        //    {
        //        return a;
        //    }
        //    else if (weight.y >= weight.z && weight.y >= weight.x)
        //    {
        //        return b;
        //    }
        //    else
        //    {
        //        return c;
        //    }
        //}
    }
}