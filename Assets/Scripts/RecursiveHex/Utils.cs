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

        public static Vector2Int GetNestedHexIndexFromOffset(this Hex hex, Vector2Int offset)
        {
            throw new NotImplementedException();
        }

        public static Vector2 GetNestedHexLocalCoordinateFromOffset(this Hex hex, Vector2Int offset)
        {
            throw new NotImplementedException();
        }

        public static Vector2 GetNoiseOffset(this Hex hex)
        {
            return hex.GetNoiseOffset(Vector2Int.zero);
        }

        public static Vector2 GetNoiseOffset(this Hex hex, Vector2Int offset)
        {
            throw new NotImplementedException();
        }

        public static Vector2 GetLocalCoordiateFromOffset(this Hex hex, Vector2Int offset)
        {
            throw new NotImplementedException();
        }

        public static Vector2 GetPointWithNoiseFromOffset(this Hex hex, Vector2Int offset)
        {
            return hex.GetLocalCoordiateFromOffset(offset) + hex.GetNoiseOffset(offset);
        }
    }
}