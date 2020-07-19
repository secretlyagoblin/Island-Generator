using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

namespace WanderingRoad
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

        public static void DrawRect(this Rect bounds, Color color, float time)
        {
            Debug.DrawLine(
                new Vector3(
                    bounds.min.x,
                    0,
                    bounds.min.y),
                new Vector3(
                    bounds.min.x,
                    0,
                    bounds.max.y),
                color,
                time);
            Debug.DrawLine(
    new Vector3(
        bounds.max.x,
        0,
        bounds.min.y),
    new Vector3(
        bounds.max.x,
        0,
        bounds.max.y),
    color,
    time);
            Debug.DrawLine(
new Vector3(
bounds.max.x,
0,
bounds.max.y),
new Vector3(
bounds.min.x,
0,
bounds.max.y),
color,
time);
            Debug.DrawLine(
new Vector3(
bounds.max.x,
0,
bounds.min.y),
new Vector3(
bounds.min.x,
0,
bounds.min.y),
color,
time);
        }

        public static RectInt ToBoundsInt(this Rect bounds)
        {
            return new RectInt(
                Mathf.FloorToInt(bounds.min.x),
                Mathf.FloorToInt(bounds.min.y),
                                Mathf.CeilToInt(bounds.size.x),
                Mathf.CeilToInt(bounds.size.y));
        }

        public static Rect ToBounds(this RectInt bounds)
        {
            return new Rect(bounds.position, bounds.size);
        }

        public static Rect Encapsulate(this Rect rect, Rect other)
        {
            var xMin = rect.xMin < other.xMin ? rect.xMin : other.xMax;
            var yMin = rect.yMin < other.yMin ? rect.yMin : other.xMax;
            var xMax = rect.xMax > other.xMax ? rect.xMax : other.xMax;
            var yMax = rect.yMax > other.yMax ? rect.yMax : other.yMax;

            return new Rect(xMin,xMax,xMax-xMin,yMax-yMin);
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

        public static bool EqualsOrSmallerThanWithinTolerance(this float a, float other, float tolerance = 0.0001f)
        {
            if (a.EqualsWithinTolerance(other, tolerance))
                return true;

            return a < other;
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

        public static void SerialiseFile(this object obj, BinaryFormatter formatter,  string folderPath, string subPath, string name, string extension)
        {
            var path = $"{folderPath}/{subPath}/{name}.{extension}";
            var info = new System.IO.FileInfo(path);

            if (!info.Exists)
                Directory.CreateDirectory(info.Directory.FullName);

            var stream = new FileStream(path, FileMode.Create, FileAccess.Write);

            formatter.Serialize(stream, obj);
            stream.Close();

            Debug.Log($"Serialised to {path}");
        }

        public static T DeserialiseFile<T>(BinaryFormatter formatter, string folderPath, string subPath, string name, string extension)
        {
            var path = $"{folderPath}/{subPath}/{name}.{extension}";

            return DeserialiseFile<T>(formatter, path);

            //Debug.Log($"Serialised to {path}");
        }

        public static T DeserialiseFile<T>(BinaryFormatter formatter, string path)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);

            var item = formatter.Deserialize(stream);
            stream.Close();

            return (T)item;

            //Debug.Log($"Serialised to {path}");
        }
    }
}
