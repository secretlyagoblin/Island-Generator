using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecursiveHex
{
    public static class RandomSeedProperties
    {
        public static float X = 234.43546f;
        public static float Y = 124.3465f;
        public static float Scale = 8.43645f;

        public static void Disable()
        {
            X = 0;
            Y = 0;
            Scale = 0;

            Debug.LogError("Random Seed is disabled, no voronoi grid");
        }

        public static RandomOffset GetOffset(int x, int y)
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
    }

    public struct RandomOffset
    {
        public float Angle;
        public float Distance;
    }
}
