using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buckets;

public class VoronoiPointBucketManager {

    Bucket<Vector3> _bucket;



    /*
     * Okay so what we need to do
     * Define a rect that is the gamespace
     * Iterate over it in some way so that only the relevant area is covered, no exponential checks
     * 
     * also getting all correct heights etc
     * 
     * Create a function that returns a subset of points for each voronoi chunk
     * 
     * Place those points into a new manager, run voronoi using that
     * 
     * deleting the whole fucking thing afterwards. 
     * 
     * 
     */

    public VoronoiPointBucketManager(Rect rect)
    {
        _bucket = new Bucket<Vector3>(20, rect);
    }

    public void AddRegion(Map.Layer region, int pointCount, Rect rect)
    {
        for (int i = 0; i < pointCount; i++)
        {
            var x = RNG.NextFloat();
            var z = RNG.NextFloat();
            var y = region.BilinearSampleFromNormalisedVector2(new Vector2(x, z));

            x = Mathf.Lerp(rect.position.x, rect.size.x, x);
            z = Mathf.Lerp(rect.position.y, rect.size.y, z);

            _bucket.AddElement(new Vector3(x, y,z),new Vector2(x,z));
        }        
    }

    // Ready for the Voronoi System!
    Bucket<VoronoiCell> GetSubChunk(Rect rect)
    {
        var points = _bucket.GetBucketsIntersectingWithRect(rect);
        var bucket = new Bucket<VoronoiCell>(6, rect);

        for (int i = 0; i < points.Count; i++)
        {
            var p = points[i];

            for (int u = 0; u < p.Elements.Count; u++)
            {
                var point = p.Elements[u];
                bucket.AddElement(new VoronoiCell(new Vector3(point.x,point.y,point.z)), new Vector2(point.x, point.z));
            }
        }
        return bucket;
    }

}
