using UnityEngine;
using System.Collections;

public struct Domain {

    public float min
    { get; private set; }

    public float max
    { get; private set; }

    public Domain(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public float Lerp(float t)
    {
        return Mathf.Lerp(min, max, t);
    }

    public float InverseLerp(float t)
    {
        return Mathf.InverseLerp(min, max, t);
    }

    public float Clamp(float value)
    {
        return Mathf.Clamp(value, min, max);
    }

}
