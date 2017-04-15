using UnityEngine;
using System.Collections;

public class PoissonDiskTester : MonoBehaviour {

    public Gradient TestGradient;

    public GameObject testInstantiate;

	// Use this for initialization
	void Start () {

        

        RNG.DateTimeInit();

        var map = Maps.Map.BlankMap(128,128).PerlinFill(50f,0,0,RNG.Next(10000));


        var mapSize = 5f;
        var minSize = mapSize / 75f;
        var maxSize = mapSize / 25f;

        var propMap = new PoissonDiscSampler(mapSize, mapSize, minSize, maxSize, map);

        foreach (var sample in propMap.Samples())
        {
            //var tex = texture.GetPixelBilinear(Mathf.InverseLerp(0, 200, sample.x), Mathf.InverseLerp(0, 200, sample.y));
            //if (tex.grayscale > 0.5f)
            //{
            //Debug.DrawRay(new Vector3(sample.Position.x, 0, sample.Position.y), Vector3.up * 0.1f, TestGradient.Evaluate(Mathf.InverseLerp(minSize,maxSize,sample.Radius)), 100f);
            // }

            var obj = Instantiate(testInstantiate, new Vector3(sample.Position.x,0,sample.Position.y), Quaternion.identity);
            obj.transform.localScale = Vector3.one * (minSize);


        }

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
