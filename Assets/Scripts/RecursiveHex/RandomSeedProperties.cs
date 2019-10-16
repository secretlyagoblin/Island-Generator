using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecursiveHex
{
    public struct RandomSeedProperties
    {
        public float X;
        public float Y;
        public float Scale;

        public RandomOffset GetOffset(int x, int y)
        {
            var perlin = Mathf.PerlinNoise((x + X) * Scale, (y + Y) * Scale);
            var radian = perlin * Mathf.PI * 4;
            var perlinLength = Mathf.PerlinNoise((x+1000 + X) * Scale, (y+1000 + Y) * Scale);

            return new RandomOffset()
            {
                Angle = radian,
                Distance = perlinLength
            };
        }

        public static RandomSeedProperties Default()
        {
            return new RandomSeedProperties()
            {
                X = 234.43546f,
                Y = 124.3465f,
                Scale = 8.43645f
            };
        }
    }

    public struct RandomOffset
    {
        public float Angle;
        public float Distance;
    }
}
