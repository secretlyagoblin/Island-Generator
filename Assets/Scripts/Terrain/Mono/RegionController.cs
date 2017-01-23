using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Terrain;

public class RegionController : MonoBehaviour {

    public Material Material;
    public Transform TestTransform;

    
    public int RegionResolution;
    public float RegionSize;

    public int ChunkUpdateDistance;

    public int NumberOfChunksInRow;
    public int ChunkResolution;
    public bool GenerateCollision;
    public int CollisionDecimationFactor;

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

        //Create Heightmap Data

        var heightMap = HeightmapData.RegionIsland(RegionResolution, new Rect(Vector2.zero, Vector2.one * RegionSize));
        //var heightMap = HeightmapData.BlankMap(RegionResolution, new Rect(Vector2.zero, Vector2.one * RegionSize),32f);

        time = Time.realtimeSinceStartup;
        Debug.Log("Generating Heightmap: " + time + " seconds");
        lastTime = time;
        yield return null;

        //Create Region, Instantiate Chunks

        _region = new Region(heightMap);
        _region.CreateChunks(NumberOfChunksInRow, ChunkResolution);
        time = Time.realtimeSinceStartup;
        Debug.Log("Creating Chunks: " + (time-lastTime) + " seconds");
        lastTime = time;
        yield return null;

        //Create Bucket System

        _region.CreateBucketSystem();
        time = Time.realtimeSinceStartup;
        Debug.Log("Creating Bucket System: " + (time - lastTime) + " seconds");
        lastTime = time;
        yield return null;

        //Create and Instantiate Individual Regions

        var obj = new GameObject();
        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;
        obj.name = "HighResolutionMap";

        _region.InstantiateRegionCells(obj.transform, Material);
        time = Time.realtimeSinceStartup;
        Debug.Log("Instantiating Meshes: " + (time - lastTime) + " seconds");
        lastTime = time;
        yield return null;

        //Add Collision To Regions

        if (GenerateCollision)
        {
            _region.InstantiateCollision(CollisionDecimationFactor);
            time = Time.realtimeSinceStartup;
            Debug.Log("Instantiating Collision: " + (time - lastTime) + " seconds");
            lastTime = time;
            yield return null;

            obj = new GameObject();
            obj.transform.parent = transform;
            obj.transform.localPosition = Vector3.zero;
            obj.name = "CollisionMap";

            _region.EnableCollision(obj.transform);
            time = Time.realtimeSinceStartup;
            Debug.Log("Enabling Collision: " + (time - lastTime) + " seconds");
            lastTime = time;
            yield return null;
        }
        //Create and Instantiate Far Landscape Cells

        obj = new GameObject();
        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;
        lastTime = time;
        obj.name = "DummyCells";

        _region.InstantiateDummyCells(obj.transform, Material);
        time = Time.realtimeSinceStartup;
        Debug.Log("Instantiating Far Landscape: " + (time - lastTime) + " seconds");
        lastTime = time;
        yield return null;

        var pool = GetComponent<DetailObjectPool>();
        pool.SetPhysicalMap(heightMap);
        pool.InitPositions();

        time = Time.realtimeSinceStartup;
        Debug.Log("Instantiating Props: " + (time - lastTime) + " seconds");
        lastTime = time;
        yield return null;

        //Clean Up, set loaded as true

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
