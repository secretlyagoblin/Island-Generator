using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buckets;

public class VoronoiPointBucketManager {

    Bucket<VoronoiCell> _bucket;



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
        _bucket = new Bucket<VoronoiCell>(10, rect);
    }

    public void AddRegion(Terrain.TerrainData region, int pointCount, Rect rect)
    {
        for (int i = 0; i < pointCount; i++)
        {
            var x = RNG.NextFloat(0,0.8f);
            var z = RNG.NextFloat(0, 0.45f);
            var y = region.HeightMap.BilinearSampleFromNormalisedVector2(new Vector2(x, z));
            var inside = region.WalkableMap.BilinearSampleFromNormalisedVector2(new Vector2(x, z));


            x = Mathf.Lerp(rect.position.x, rect.position.x+ rect.size.x, x);
            z = Mathf.Lerp(rect.position.y, rect.position.x +rect.size.y, z);

            var insideBool = inside > 0.5f ? true : false;

            if (insideBool)
            {
                Debug.DrawRay(new Vector3(x, y, z), Vector3.up * 100f, Color.red, 20000f);
            }
            else
            {
                Debug.DrawRay(new Vector3(x, y, z), Vector3.up * 100f, Color.green, 20000f);
            }

            

            var voronoi = new VoronoiCell(new Vector3(x, y, z));
            voronoi.Inside = insideBool;

            _bucket.AddElement(voronoi, new Vector2(x,z));
        }        
    }

    // Ready for the Voronoi System!
    public List<Bucket<VoronoiCell>> GetSubChunk(Rect rect)
    {
        var points = _bucket.GetBucketsIntersectingWithRect(rect);

        //List<VoronoiCell> outputCells = new List<VoronoiCell>();

        //var color = RNG.GetRandomColor();

        //for (int i = 0; i < points.Count; i++)
        //{
        //    var p = points[i];
        //
        //    for (int u = 0; u < p.Elements.Count; u++)
        //    {
        //        var point = p.Elements[u];
        //
        //        //Debug.DrawRay(point, Vector3.up*100f, color,100f);                
        //
        //        var x = Util.InverseLerpUnclamped(rect.position.x, rect.size.x, point.x);
        //        var z = Util.InverseLerpUnclamped(rect.position.y, rect.size.y, point.z);
        //
        //        outputCells.Add(new VoronoiCell(new Vector3(x,point.y,z)));
        //    }
        //}
        return points;
    }

}
