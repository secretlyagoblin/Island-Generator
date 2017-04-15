using UnityEngine;
using System.Collections;

public class PoissonDiskTester : MonoBehaviour {

    public Gradient Grad;
    public GameObject testInstantiate;

    // Use this for initialization
    void Start()
    {
        var map = Terrain.TerrainData.RegionIsland(128, Rect.zero);
        map.HeightMap.Normalise();
        var mapOverlay = map.HeightMap.GetBumpMap().Normalise();


        var bandCount = 3f;

        for (int bands = 1; bands < bandCount; bands++)
        {
            var propMap = new PoissonDiscSampler(1, 1, bands / 400f);

            var color = Grad.Evaluate(bands / bandCount);

            foreach (var sample in propMap.Samples())
            {
                var val = mapOverlay.BilinearSampleFromNormalisedVector2(sample);
                if (val < bands/ bandCount && val > bands/ bandCount - 1f/ bandCount && map.WalkableMap.BilinearSampleFromNormalisedVector2(sample) > 0.5f)
                {
                    //Debug.DrawRay(new Vector3(sample.x, 0, sample.y), Vector3.up * 0.1f, color, 100f);

                    var obj = Instantiate(testInstantiate, new Vector3(sample.x, map.HeightMap.BilinearSampleFromNormalisedVector2(sample), sample.y), Quaternion.identity);
                    obj.transform.localScale = Vector3.one * (bands / 400f);

                }

            }
        }




    }

        // Update is called once per frame
        void Update () {
	
	}
}
