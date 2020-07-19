using UnityEngine;
using System.Collections.Generic;
using System;

namespace WanderingRoad.Random
{

    public static class RNG
    {

        static System.Random _pseudoRandom;
        static bool _initialised = false;

        private static int _seed;

        private static uint _derivedXYSeed;

        public static int CurrentSeed()
        {
            return _seed;
        }

        public static void DateTimeInit()
        {
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
                ForceInit(seed);
            }
        }

        //public static void Init()
        //{
        //    if (_initialised)
        //    { }
        //    else
        //    {
        //        _pseudoRandom = new System.Random();
        //        _derivedXYSeed = (uint)_pseudoRandom.Next(0, int.MaxValue);
        //        _initialised = true;
        //    }
        //}

        public static void ForceInit(string seed)
        {
            Debug.Log($"Current Seed: \"{seed}\"");
            _seed = seed.GetHashCode();
            _pseudoRandom = new System.Random(_seed);
            _derivedXYSeed = (uint)_pseudoRandom.Next(0, int.MaxValue);
            _initialised = true;
        }

        public static bool CoinToss()
        {
            return _pseudoRandom.NextDouble() < 0.5;
        }

        public static T CoinToss<T>(T a, T b)
        {
            return (_pseudoRandom.NextDouble() < 0.5) ? a : b;
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
            return Mathf.FloorToInt(Mathf.Lerp(minValue, maxValue, t));
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


            return minValue + ((float)(_pseudoRandom.NextDouble()) * (maxValue - minValue));

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

        public static Color NextColorBright()
        {
            return Color.HSVToRGB(
                NextFloat(0, 1),
                0.9f,
                0.9f);
        }

        public static Color NextColorDark()
        {
            return Color.HSVToRGB(
                NextFloat(0, 1),
                0.2f,
                0.2f);
        }

        public static Color SimilarColor(Color color, float bounds = 0.1f)
        {
            return new Color(color.r + RNG.NextFloat(-bounds, bounds), color.b + RNG.NextFloat(-bounds, bounds), color.g + RNG.NextFloat(-bounds, bounds));

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

        //https://softwareengineering.stackexchange.com/questions/161336/how-to-generate-random-numbers-without-making-new-random-objects

        private static uint BitRotate(uint x)
        {
            const int bits = 16;
            return (x << bits) | (x >> (32 - bits));
        }

        public static uint GetXYNoiseInt(int x, int y, int offset = 0)
        {
            UInt32 num = _derivedXYSeed + (uint)offset;
            for (uint i = 0; i < 16; i++)
            {
                num = num * 541 + (uint)x;
                num = BitRotate(num);
                num = num * 809 + (uint)y;
                num = BitRotate(num);
                num = num * 673 + (uint)i;
                num = BitRotate(num);
            }

            return num % 4;
        }

        //private static uint _divisor = (uint.MaxValue / 100000);

        //public static float GetXYNoise(int x, int y, int offset = 0)
        //{
        //    double wha = GetXYNoiseInt(x, y, offset) / _divisor;
        //    return (float)(wha / _divisor);
        //}



    }
}
