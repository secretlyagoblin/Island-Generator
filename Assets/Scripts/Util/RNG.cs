using UnityEngine;
using System.Collections.Generic;

public static class RNG {

    static System.Random _pseudoRandom;
    static bool _initialised = false;

    public static void DateTimeInit(){
        if (_initialised)
        {
        }
        else
        {
            Init(System.DateTime.Now.ToString());
        }
    }

    public static void Init(string seed)
    {
        if (_initialised)
        { }
        else
        {
            _pseudoRandom = new System.Random(seed.GetHashCode());
            _initialised = true;
        }
    }

    public static void Init()
    {
        if (_initialised)
        { }
        else
        {
            _pseudoRandom = new System.Random();
            _initialised = true;
        }
    }

    public static void ForceInit(string seed)
    {
        _pseudoRandom = new System.Random(seed.GetHashCode());
        _initialised = true;
    }

    public static bool CoinToss()
    {
        return _pseudoRandom.NextDouble() < 0.5;
    }

    public static T CoinToss<T>(T a, T b)
    {
        return (_pseudoRandom.NextDouble() < 0.5)?a:b;
    }

    public static bool SmallerThan(float value)
    {
        return _pseudoRandom.NextDouble() < value;
    }

    public static bool SmallerThan(double value)
    {
        return _pseudoRandom.NextDouble() < value;
    }

    public static int Next()
    {
        return _pseudoRandom.Next();
    }

    public static T NextFromList<T>(List<T> list)
    {
        return list[Next(0, list.Count)];
    }

    public static int Next(int maxValue)
    {
        return _pseudoRandom.Next(maxValue);
    }

    public static int Next(int minValue, int maxValue)
    {
        return _pseudoRandom.Next(minValue, maxValue);
    }

    public static int Next(int minValue, int maxValue, AnimationCurve curve)
    {
        var t = curve.Evaluate(NextFloat());
        return Mathf.FloorToInt(Mathf.Lerp(minValue,maxValue,t));
    }

    public static float Next(float minValue, float maxValue, AnimationCurve curve)
    {
        var t = curve.Evaluate(NextFloat());
        return Mathf.Lerp(minValue, maxValue, t);
    }

    public static float NextFullRangeFloat()
    {
        double mantissa = (_pseudoRandom.NextDouble() * 2.0) - 1.0;
        double exponent = System.Math.Pow(2.0, _pseudoRandom.Next(-126, 128));
        return (float)(mantissa * exponent);
    }

    public static float NextFloat()
    {
        return (float)(_pseudoRandom.NextDouble());

    }

    public static Vector2 NextVector2(float minValue, float maxValue)
    {
        return new Vector2(NextFloat(minValue, maxValue), NextFloat(minValue, maxValue));
    }


    public static Vector3 NextVector3(float minValue, float maxValue)
    {
        return new Vector3(NextFloat(minValue, maxValue), NextFloat(minValue, maxValue), NextFloat(minValue, maxValue));
    }


    public static float NextFloat(float maxValue)
    {
        return (float)(_pseudoRandom.NextDouble()) * maxValue;

    }

    public static float NextFloat(float minValue, float maxValue)
    {


        return minValue+ ((float)(_pseudoRandom.NextDouble()) * (maxValue-minValue));

    }

    public static float NextFloat(float minValue, float maxValue, AnimationCurve curve)
    {
        var t = curve.Evaluate(NextFloat());
        return minValue + (t * (maxValue - minValue));
    }

    public static double NextDouble()
    {
        return _pseudoRandom.NextDouble();

    }

    public static T GetRandomItem<T>(T[] array)
    {
        var item = Next(array.Length);
        return array[item];
    }

    public static T GetRandomItem<T>(List<T> list)
    {
        var item = Next(list.Count);
        return list[item];
    }

    public static Color NextColor()
    {
        return new Color(NextFloat(), NextFloat(), NextFloat());

    }

    public static List<T> Shuffle<T>(List<T> list)
    {
        var intList = new T[list.Count];
        list.CopyTo(intList);
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = RNG.Next(n + 1);
            T value = intList[k];
            intList[k] = intList[n];
            intList[n] = value;
        }
        return new List<T>(intList);
    }

}
