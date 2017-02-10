using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buckets;

public class VoronoiPointBucketManager {

    Bucket<Vector3> _bucket;

    public VoronoiPointBucketManager(int divisions, Vector2 lowerBounds, Vector2 upperBounds)
    {
        _bucket = new Bucket<Vector3>(divisions, lowerBounds, upperBounds);

    }


}
