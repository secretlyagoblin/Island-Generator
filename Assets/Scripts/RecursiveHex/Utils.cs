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
    }
}