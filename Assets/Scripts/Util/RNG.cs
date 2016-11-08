using UnityEngine;
using System.Collections;

public static class RNG {

    static System.Random _pseudoRandom;
    static bool _initialised = false;

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

    public static int Next()
    {
        return _pseudoRandom.Next();
    }

    public static int Next(int maxValue)
    {
        return _pseudoRandom.Next(maxValue);
    }

    public static int Next(int minValue, int maxValue)
    {
        return _pseudoRandom.Next(minValue, maxValue);
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

    public static float NextFloat(float maxValue)
    {
        return (float)(_pseudoRandom.NextDouble()) * maxValue;

    }

    public static float NextFloat(float minValue, float maxValue)
    {
        return minValue+ ((float)(_pseudoRandom.NextDouble()) * (maxValue-minValue));

    }

    public static double NextDouble()
    {
        return _pseudoRandom.NextDouble();

    }

}
