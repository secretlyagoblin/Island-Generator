using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RcokMaker : MonoBehaviour {

    public GameObject BaseObject;
    public Vector3 Offset;

	// Use this for initialization
	void Start () {

        var count = 80;

        for (int x = 0; x < count; x++)
        {
            for (int y = 0; y < count; y++)
            {
                //if (Random.Range(0, 2) <1)
                 //   continue;

                var obj = Instantiate(BaseObject, new Vector3(x*Offset.x, Random.Range(0.8f, 1.2f)*Offset.y, y*Offset.z), Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up), transform);
                obj.transform.localScale = new Vector3(Random.Range(0.8f, 2f), Random.Range(2f, 4f), Random.Range(0.8f, 2f));
                obj.transform.Translate(Vector3.up*60f * Mathf.PerlinNoise(x * 0.12342f, y * 0.12342f));

                obj.transform.Translate(new Vector3(Random.Range(-1f, -1f),0, Random.Range(-1f, -1f)));


                //obj.
            }
        }
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
