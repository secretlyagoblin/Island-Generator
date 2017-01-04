using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BucketManagerTest : MonoBehaviour {

    RegionBucketManager bucks = new RegionBucketManager();
    public GameObject testObj;

    public int Size;
    public Rect Rect;

	// Use this for initialization
	void Start () {

        //Here I be. Working on the buckets

        bucks.CreateBucketSystem();		
	}
	
	// Update is called once per frame
	void Update () {

        bucks.Update(testObj.transform.position, 3f);

	}
}
