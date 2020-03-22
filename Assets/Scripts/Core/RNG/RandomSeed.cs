using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WanderingRoad.Core.Random
{
    public static class RandomXY
    {
        public static float X = 234.43546f;
        public static float Y = 124.3465f;
        public static float Scale = 8.43645f;

        //private const float NOISE_OFFSET_SCALE = 0.37f; //We need to rethink this..


        private static bool _isDisabled = false;

        public static void SetRandomSeed(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static void Disable()
        {
            _isDisabled = true;
            Debug.LogWarning("Random Seed is disabled, no voronoi grid");
        }

        public static RandomOffset GetOffset(int x, int y)
        {
            return GetOffset((float)x, (float)y);
        }

        public static RandomOffset GetOffset(float x, float y)
        {
            if (_isDisabled)
            {
                return new RandomOffset()
                {
                    Angle = 0,
                    Distance = 0
                };
            }

            var perlin = Mathf.PerlinNoise((x + X) * Scale, (y + Y) * Scale);
            var radian = perlin * Mathf.PI * 4;
            var perlinLength = Mathf.PerlinNoise((x+1000 + X) * Scale, (y+1000 + Y) * Scale);

            return new RandomOffset()
            {
                Angle = radian,
                Distance = Mathf.Clamp(perlinLength,0,1)
            };
        }

        /// <summary>
        /// Gets a consistent vector within 0.5 when given a hex or a hex with an offset
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Vector2 AddNoiseOffset(this Vector2 vec, int multiplier = 1)
        {
            var index2d = vec;
            var result = RandomXY.GetOffset(index2d.x, index2d.y);

            return new Vector2(
                Mathf.Sin(result.Angle) * result.Distance * multiplier,
                Mathf.Cos(result.Angle) * result.Distance * multiplier);
        }

        public static Vector3 AddNoiseOffset(this Vector3 vec, float multiplier = 1)
        {
            var vector3 = vec;

            var result = RandomXY.GetOffset(vector3.x, vector3.z);

            var offset = new Vector3(
                Mathf.Sin(result.Angle) * result.Distance * multiplier,
                0,
                Mathf.Cos(result.Angle) * result.Distance * multiplier);

            return vec + offset;
        }


    }

    public struct RandomOffset
    {
        public float Angle;
        public float Distance;
    }
}
