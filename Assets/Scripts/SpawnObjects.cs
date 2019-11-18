using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObjects : MonoBehaviour {

    public GameObject Object;
    public int Iterations;

	// Use this for initialization
	void Start () {

        var noiseScale = 0.1f;

        MaterialPropertyBlock block = new MaterialPropertyBlock();

        RNG.DateTimeInit();

        for (int i = 0; i < Iterations; i++)
        {
            var pos = new Vector3(RNG.NextFloat(0, 1000),0,RNG.NextFloat(0, 1000));
            var rot = new Vector3(0, RNG.NextFloat(0, 100),0);

            var colorNoise = Mathf.PerlinNoise(pos.x * 0.04f, pos.z * 0.04f) *0.5f;

            block.SetColor("_Color",new Color(RNG.NextFloat(0, 0.5f) + colorNoise, RNG.NextFloat(0, colorNoise), RNG.NextFloat(0, 1)));


            var scale = (RNG.NextFloat(0.6f, 1.4f));
            scale = scale *((Mathf.PerlinNoise(pos.x * noiseScale, pos.z * noiseScale))+0.5f);
            //scale = scale * 0.5f;

            //var obj = Instantiate(Object, pos, Quaternion.AngleAxis(RNG.NextFloat(0, 360),Vector3.up));
            var obj = Instantiate(Object, pos, Quaternion.Euler(rot));

            obj.transform.localScale = new Vector3(scale, scale, scale);

            obj.GetComponentInChildren<MeshRenderer>().SetPropertyBlock(block);
        }
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
