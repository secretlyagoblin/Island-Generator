using UnityEngine;

namespace WanderingRoad.Procgen.RecursiveHex
{
    static class InterpolationHelpers
    {

        public static float Blerp(float a, float b, float c, Vector3 weight)
        {
            return a * weight.x + b * weight.y + c * weight.z;
        }

        public static Color Blerp(Color a, Color b, Color c, Vector3 weight)
        {
                var r = a.r * weight.x + b.r * weight.y + c.r * weight.z;
                var g = a.g * weight.x + b.g * weight.y + c.g * weight.z;
                var bee = a.b * weight.x + b.b * weight.y + c.b * weight.z;
        
                return new Color(r, g, bee);
        }

        public static T Blerp<T>(T a, T b, T c, Vector3 weight)
        {
            if (weight.x >= weight.y && weight.x >= weight.z)
            {
                return a;
            }
            else if (weight.y >= weight.z && weight.y >= weight.x)
            {
                return b;
            }
            else
            {
                return c;
            }
        }
    }
}