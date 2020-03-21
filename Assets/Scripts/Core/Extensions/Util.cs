using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WanderingRoad.Core
{
    public static class Util
    {
        public static void CreateAsset<T>() where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).ToString() + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        public static float InverseLerpUnclamped(float from, float to, float value)
        {
            return (value - from) / (to - from);
        }

        public static HashSet<T> ToHashSet<T>(
            this IEnumerable<T> source,
            IEqualityComparer<T> comparer = null)
        {
            return new HashSet<T>(source, comparer);
        }

        public static Vector3 AsVector(this Color color)
        {
            return new Vector3(color.r, color.b, color.g);
        }

        public static bool EqualsWithinTolerance(this float a, float other, float tolerance = 0.0001f)
        {
            return Mathf.Abs(a - other) < tolerance;
        }

        public static bool EqualsOrLargerThanWithinTolerance(this float a, float other, float tolerance = 0.0001f)
        {
            if (a.EqualsWithinTolerance(other, tolerance))
                return true;

            return a > other;
        }

        /// <summary>
        /// Break a list of items into chunks of a specific size
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            while (source.Any())
            {
                yield return source.Take(chunksize);
                source = source.Skip(chunksize);
            }
        }
    }
}
