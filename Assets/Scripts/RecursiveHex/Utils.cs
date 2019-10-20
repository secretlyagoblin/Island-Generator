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
        /// Mapping a XY grid of hexes by a level requires a rhombus - this is the X translation of the bottom of this rhombus
        /// </summary>
        private static Vector2 _rhombusX = new Vector2(2.5f, 1);

        /// <summary>
        /// Mapping a XY grid of hexes by a level requires a rhombus - this is the Y translation of the bottom of this rhombus
        /// </summary>
        private static Vector2 _rhombusY = new Vector2(0.5f, 3);

        /// <summary>
        /// Rhombus offsetting obvious results in a rhombus. 
        /// We have to skew in the other direction to get a nice result - this is the magic number I chose for that.
        /// </summary>
        private const float MAGIC_GRID_SKEW_RATIO = 0.59f;


        /// <summary>
        /// The major offsetting function. Set up for a 2x2 grid only, with some magic numbers for keeping the grid sqaure.
        /// </summary>
        /// <param name="localOffset"></param>
        /// <returns></returns>
        public static Vector2Int GetNestedHexIndexFromOffset(this Hex hex, Vector2Int localOffset)
        {
            var inUseIndex = hex.Index; //we'll be modifying this, so we make a local copy

            var xOffset = inUseIndex.y * MAGIC_GRID_SKEW_RATIO;

            inUseIndex.x -= Mathf.FloorToInt(xOffset);

            var shiftedIndex = (inUseIndex.x * _rhombusX) +
                (inUseIndex.y * _rhombusY);

            var evenX = inUseIndex.x % 2 == 0;
            var evenY = inUseIndex.y % 2 == 0;

            var offsetX = Mathf.FloorToInt(shiftedIndex.x);
            var offsetY = Mathf.FloorToInt(shiftedIndex.y);

            //Different grids require different edge cases - here's them.
            if (evenX && evenY)
            {

            }
            else if (evenX && !evenY)
            {
                if (localOffset.y % 2 != 0)
                {
                    offsetX++;
                }
            }
            else if (!evenX && evenY)
            {
                if (localOffset.y % 2 != 0)
                {
                    offsetX++;
                }
            }
            else if (!evenX && !evenY)
            {

            }

            //final offset
            var x = offsetX + localOffset.x;
            var y = offsetY + localOffset.y;

            return new Vector2Int(x, y);
        }

        private const float MAGIC_INNER_ROTATION = 0.33347317225183f;

        //private const float MAGIC_INNER_ROTATION = 700f;
        //private const float MAGIC_INNER_ROTATION = 0f;


        private static float _scale = 1f / (Mathf.Sqrt(7));

        public static Vector2 GetNestedHexLocalCoordinateFromOffset(this Hex hex, Vector2Int offset)
        {
            var index = hex.Index + offset;
            var isOdd =  index.y % 2 != 0;
            var xOffset = isOdd ? 0.5f : 0f;
            var finalX = index.x + xOffset;

            //https://stackoverflow.com/questions/13695317/rotate-a-point-around-another-point

            float cosTheta = Mathf.Cos(MAGIC_INNER_ROTATION);
            float sinTheta = Mathf.Sin(MAGIC_INNER_ROTATION);
            return new Vector2(           

                    (cosTheta * (finalX) - sinTheta * (index.y * Hex.ScaleY)) * _scale,
                    (sinTheta * (finalX) + cosTheta * (index.y * Hex.ScaleY))* _scale
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
                Mathf.Sin(result.Angle) * result.Distance * 0.5f,
                Mathf.Cos(result.Angle) * result.Distance * 0.5f);
        }

        public static Vector2 GetLocalCoordiateFromOffset(this Hex hex, Vector2Int offset)
        {
            throw new NotImplementedException();
        }

        public static Vector2 GetPointWithNoiseFromOffset(this Hex hex, Vector2Int offset)
        {
            return hex.GetLocalCoordiateFromOffset(offset) + hex.GetNoiseOffset(offset);
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
    }
}