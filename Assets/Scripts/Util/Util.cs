using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public static class Util {

    public static class AngleCalculator {
        public static float GetAngle(Vector2 testVector, Vector2 north)
        {
            var u = north;
            var v = testVector;

            var dp = (u.x * v.x) + (u.y * v.y); //dot product
            var mag = Mathf.Sqrt(Mathf.Pow(u.x, 2) + Mathf.Pow(u.y, 2)) *
                Mathf.Sqrt(Mathf.Pow(v.x, 2) + Mathf.Pow(v.y, 2));
            var angle = Mathf.Rad2Deg * (Mathf.Acos(dp / mag));

            if (((u.x * -v.y) + (u.y * v.x)) < 0) //check if clockwise or anticlockwise
            {
                angle = 180f + (180f - angle);
            }

            return angle;
        }

        public static float GetAngle(Vector3 testVector)
        {
            return GetAngle(testVector, Vector3.forward);
        }

        /*

    public static Orientation GetOrientation(float angle)
    {
        Orientation orientation;

        if (angle < 45)
        {
            orientation = Orientation.North;
        }
        else if (angle < 135)
        {
            orientation = Orientation.East;
        }
        else if (angle < 225)
        {
            orientation = Orientation.South;
        }
        else if (angle < 315)
        {
            orientation = Orientation.West;
        }
        else
        {
            orientation = Orientation.North;
        }
        return orientation;
    }

    */

        public static float FullToHalfCircle(float angle)
        {
            if (angle < 0)
                angle = -angle;
            if (angle > 180)
                angle = 180 - (angle - 180);
            return angle;
        }

        public static float FullToQuarterCircle(float angle)
        {
            if (angle < 0)
                angle = -angle;
            if (angle > 90)
                angle = 90 - (angle - 90);
            return angle;
        }



        

    }

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
    
}