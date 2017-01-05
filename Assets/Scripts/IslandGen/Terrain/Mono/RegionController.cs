using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Terrain;

public class RegionController : MonoBehaviour {

    public Material Material;
    public Transform TestTransform;
    public int ChunkUpdateDistance;

    Region _region;

    bool _loaded = false;

    

	// Use this for initialization
	void Start () {
        StartCoroutine(Load());
    }

    IEnumerator Load()
    {
        var lastTime = 0f;
        var time = 0f;

        var heightMap = HeightmapData.RegionIsland(400, new Rect(Vector2.zero, Vector2.one * 1000f));
        time = Time.realtimeSinceStartup;
        Debug.Log("Generating Heightmap: " + time + " seconds");
        lastTime = time;
        yield return null;

        _region = new Region(heightMap);
        _region.CreateChunks(16, 100);
        time = Time.realtimeSinceStartup;
        Debug.Log("Creating Chunks: " + (time-lastTime) + " seconds");
        lastTime = time;

        yield return null;

        _region.CreateBucketSystem();
        time = Time.realtimeSinceStartup;
        Debug.Log("Creating Bucket System: " + (time - lastTime) + " seconds");
        lastTime = time;
        yield return null;

        var obj = new GameObject();
        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;
        obj.name = "CollisionCells";

        _region.InstantiateRegionCells(obj.transform, Material);
        time = Time.realtimeSinceStartup;
        Debug.Log("Instantiating Collision: " + (time - lastTime) + " seconds");
        lastTime = time;
        yield return null;

        obj = new GameObject();
        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;
        lastTime = time;
        obj.name = "DummyCells";

        _region.InstantiateDummyCells(obj.transform, Material);
        time = Time.realtimeSinceStartup;
        Debug.Log("Instantiating Far Landscape: " + (time - lastTime) + " seconds");
        lastTime = time;

        Debug.Log("Total Load Time : " + Time.realtimeSinceStartup        + " seconds");

        _loaded = true;
    }

    void Update()
    {
        if (_loaded)
        {
            var pos = TestTransform.position;
            _region.Update(transform.InverseTransformPoint(pos), ChunkUpdateDistance);
        }
    }

    



}
