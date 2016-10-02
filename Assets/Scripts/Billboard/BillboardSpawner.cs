using UnityEngine;
using System.Collections;

public class BillboardSpawner : MonoBehaviour {

    public GameObject Billboard;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        var gameobject = Instantiate(Billboard);
        gameobject.transform.position = new Vector3(Random.Range(-50f, 50f), Random.Range(-50f, 50f), Random.Range(-50f, 50f));
	
	}
}
