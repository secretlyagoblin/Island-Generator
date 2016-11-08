using UnityEngine;
using System.Collections;

public class PoissonDiskTester : MonoBehaviour {

	// Use this for initialization
	void Start () {

        var propMap = new PoissonDiscSampler(1, 1, 1f/200f);

        foreach (var sample in propMap.Samples())
        {
            //var tex = texture.GetPixelBilinear(Mathf.InverseLerp(0, 200, sample.x), Mathf.InverseLerp(0, 200, sample.y));
            //if (tex.grayscale > 0.5f)
            //{
            Debug.DrawRay(new Vector3(sample.x, 0, sample.y), Vector3.up * 0.01f, Color.green, 100f);
            // }

        }

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
