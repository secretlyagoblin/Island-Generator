using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BucketManagerTest : MonoBehaviour {

    TerrainBucketManager bucks = new TerrainBucketManager();
    public GameObject testObj;

    public int Size;
    public Rect Rect;

	// Use this for initialization
	void Start () {

        var totalData = new FinalChunk[Size, Size];

        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                totalData[x, y] = new FinalChunk();
            }
        }

        bucks.CreateBucketSystem(Rect, totalData);		
	}
	
	// Update is called once per frame
	void Update () {

        bucks.Update(testObj.transform.position, 3f);

	}
}
