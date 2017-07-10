using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using U3D.Threading.Tasks;
using ProcTerrain;

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

    public DetailObjectPool[] DetailObjectPools;

    Region _region;
    bool _loaded = false;
    ProcTerrain.TerrainData _heightMap;

    float _timeSinceStartup = 0;
    float _previousTimeSinceStartup = 0;

    // Use this for initialization
    void Start () {

        RNG.DateTimeInit();
        PaletteManager.GetPalette();

        StartCoroutine(LoadPart1());
    }

    IEnumerator LoadPart1()
    {
        _previousTimeSinceStartup = 0f;
        _timeSinceStartup = 0f;

        //Create Heightmap Data

        var regionRect = new Rect(Vector2.zero, Vector2.one * RegionSize);

        _heightMap = ProcTerrain.TerrainData.RegionIsland(RegionResolution, regionRect);
        //var heightMap = HeightmapData.BlankMap(RegionResolution, new Rect(Vector2.zero, Vector2.one * RegionSize),32f);

        _timeSinceStartup = Time.realtimeSinceStartup;
        Debug.Log("Generating Heightmap: " + _timeSinceStartup + " seconds");
        _previousTimeSinceStartup = _timeSinceStartup;
        yield return null;

        var voronoiPointBucketManager = new VoronoiPointBucketManager(regionRect);
        voronoiPointBucketManager.AddRegion(_heightMap, 24000, regionRect);

        _timeSinceStartup = Time.realtimeSinceStartup;
        Debug.Log("Generating Voronoi Cells: " + (_timeSinceStartup - _previousTimeSinceStartup) + " seconds");
        _previousTimeSinceStartup = _timeSinceStartup;
        yield return null;



        //Create Region, Instantiate Chunks

        _region = new Region(_heightMap, voronoiPointBucketManager);
        _region.CreateMultithreadedChunks(NumberOfChunksInRow, ChunkResolution,ReturnedTask);
        //_region.CreateChunks(NumberOfChunksInRow, ChunkResolution);
        _timeSinceStartup = Time.realtimeSinceStartup;
        Debug.Log("Starting to Create Chunks: " + (_timeSinceStartup - _previousTimeSinceStartup) + " seconds");
        _previousTimeSinceStartup = _timeSinceStartup;
        yield return null;

        //StartCoroutine(LoadPart2());

        Debug.Log("CANCEL LOAD");
        yield break;

        //Create Bucket System

       
    }

    IEnumerator LoadPart2()
    {
        _region.CreateBucketSystem();
        _timeSinceStartup = Time.realtimeSinceStartup;
        Debug.Log("Creating Bucket System: " + (_timeSinceStartup - _previousTimeSinceStartup) + " seconds");
        _previousTimeSinceStartup = _timeSinceStartup;
        yield return null;

        //Create and Instantiate Individual Regions

        var obj = new GameObject();
        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;
        obj.name = "HighResolutionMap";

        _region.InstantiateRegionCells(obj.transform, Material);
        _timeSinceStartup = Time.realtimeSinceStartup;
        Debug.Log("Instantiating Meshes: " + (_timeSinceStartup - _previousTimeSinceStartup) + " seconds");
        _previousTimeSinceStartup = _timeSinceStartup;
        yield return null;

        //Add Collision To Regions

        if (GenerateCollision)
        {
            _region.InstantiateCollision(CollisionDecimationFactor);
            _timeSinceStartup = Time.realtimeSinceStartup;
            Debug.Log("Instantiating Collision: " + (_timeSinceStartup - _previousTimeSinceStartup) + " seconds");
            _previousTimeSinceStartup = _timeSinceStartup;
            yield return null;

            obj = new GameObject();
            obj.transform.parent = transform;
            obj.transform.localPosition = Vector3.zero;
            obj.name = "CollisionMap";

            _region.EnableCollision(obj.transform);
            _timeSinceStartup = Time.realtimeSinceStartup;
            Debug.Log("Enabling Collision: " + (_timeSinceStartup - _previousTimeSinceStartup) + " seconds");
            _previousTimeSinceStartup = _timeSinceStartup;
            yield return null;
        }
        //Create and Instantiate Far Landscape Cells

        obj = new GameObject();
        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;
        _previousTimeSinceStartup = _timeSinceStartup;
        obj.name = "DummyCells";

        _region.InstantiateDummyCells(obj.transform, Material);
        _timeSinceStartup = Time.realtimeSinceStartup;
        Debug.Log("Instantiating Far Landscape: " + (_timeSinceStartup - _previousTimeSinceStartup) + " seconds");
        _previousTimeSinceStartup = _timeSinceStartup;
        yield return null;

        for (int i = 0; i < DetailObjectPools.Length; i++)
        {
            //DetailObjectPools[i].SetPhysicalMap(_);
            DetailObjectPools[i].InitPositions();
        }

        _timeSinceStartup = Time.realtimeSinceStartup;
        Debug.Log("Instantiating Props: " + (_timeSinceStartup - _previousTimeSinceStartup) + " seconds");
        _previousTimeSinceStartup = _timeSinceStartup;
        yield return null;

        //Clean Up, set loaded as true

        Debug.Log("Total Load Time : " + Time.realtimeSinceStartup + " seconds");

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

    void ReturnedTask(Task t)
    {
        _timeSinceStartup = Time.realtimeSinceStartup;
        Debug.Log("Chunk Creation Finished: " + (_timeSinceStartup - _previousTimeSinceStartup) + " seconds");
        _previousTimeSinceStartup = _timeSinceStartup;

        StartCoroutine(LoadPart2());
    }
}
