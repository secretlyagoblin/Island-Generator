using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateStandardTerrain : MonoBehaviour {

    public Gradient Steepness;
    public AnimationCurve Falloff;

    private float width = 1500;
    private float lenght = 1500;
    private float height = 300;

    private static Vector2 tileAmount = Vector2.one;

    private int heightmapResoltion = 256 +1;
    private int detailResolution = 256;
    private int detailResolutionPerPatch = 8;
    private int controlTextureResolution = 256;
    private int baseTextureReolution = 1024;

    public DetailObjectPool[] DetailObjectPools;

    public SplatCollection SplatCollection;
    public DetailObjectCollection Details;

    public GameObject RockToCreateSteep;
    public GameObject RockToCreateShallow;
    public GameObject Sphere;


    Terrain.TerrainData _map;

    MaterialPropertyBlock _block;

    // Use this for initialization
    void Start()
    {
        _block = new MaterialPropertyBlock();
        CreateTerrain();

    }

    // Update is called once per frame

    bool duh = false;

    void Update()
    {
        if (!duh)
        {
            duh = true;
            CreateMyToys();
        }
    }

    private void CreateTerrain()
    {
        //GameObject parent = Instantiate(new GameObject("Boostr"));
        //parent.transform.position = new Vector3(0, 0, 0);

        _map = Terrain.TerrainData.RegionIsland(heightmapResoltion, new Rect());
        _map.HeightMap.Remap(0, height);


        TerrainData terrainData = new TerrainData();

        terrainData.size = new Vector3(width / 8f,
                                        height,
                                        lenght / 8f);

        //terrainData.

        terrainData.baseMapResolution = baseTextureReolution;
        terrainData.heightmapResolution = heightmapResoltion;
        terrainData.alphamapResolution = controlTextureResolution;
        terrainData.SetDetailResolution(detailResolution, detailResolutionPerPatch);
        terrainData.SetHeights(0, 0, _map.HeightMap.CreateTerrainMap().Normalise().FloatArray);
        //terrainData.set
        terrainData.splatPrototypes = SplatCollection.GetSplatPrototypes();
        terrainData.SetAlphamaps(0, 0, SplatCollection.GetAlphaMaps(_map.WalkableMap.Clone().Resize(256, 256)));
        terrainData.detailPrototypes = Details.GetDetailPrototypes();



        Details.SetDetails(terrainData, _map.WalkableMap);


        //terrainData.name = name;
        GameObject terrainObj = UnityEngine.Terrain.CreateTerrainGameObject(terrainData);
        var terrain = terrainObj.GetComponent<UnityEngine.Terrain>();
        terrain.detailObjectDistance = 100f;

        //terrain.name = name;
        //terrain.transform.parent = parent.transform;
        terrainObj.transform.position = new Vector3(0, 0, 0);

        var parent = new GameObject();




        var steepnessMap = Maps.Map.BlankMap(128, 128).GetSteepnessMapFromTerrain(terrainData).Normalise().Remap(Falloff).Invert().Normalise();

        //for (int x = 0; x < steepnessMap.SizeX; x++)
        //{
        //    for (int y = 0; y < steepnessMap.SizeY; y++)
        //    {
        //        var steps = 12f;
        //
        //        var steep = Mathf.RoundToInt(steps * steepnessMap[x, y]) / steps;
        //        var col = Steepness.Evaluate(steep);
        //        
        //        var height = 30f + (steep * 5);
        //        var point = new Vector3(x, height, y);
        //
        //        if (steep > 0.01f)
        //        {
        //
        //            Debug.DrawLine(point - (Vector3.left * 0.5f), point + (Vector3.left * 0.5f), col, 100f);
        //            Debug.DrawLine(point - (Vector3.forward * 0.5f), point + (Vector3.forward * 0.5f), col, 100f);
        //        }
        //
        //        //Debug.DrawRay(, Vector3.up, , 100f);
        //    }
        //}

        var propCount = 300f;

        var mapSize = (float)width;
        var minSize = mapSize / propCount;
        var maxSize = mapSize / (propCount + 100f);

        var propMap = new PoissonDiscSampler(mapSize, mapSize, minSize, maxSize, steepnessMap);
        var spawnMap = _map.WalkableMap.Clone().ThickenOutline(1);


        foreach (var sample in propMap.Samples())
        {

            //var tex = texture.GetPixelBilinear(Mathf.InverseLerp(0, 200, sample.x), Mathf.InverseLerp(0, 200, sample.y));
            //if (tex.grayscale > 0.5f)
            //{
            //Debug.DrawRay(new Vector3(sample.Position.x, 30, sample.Position.y), Vector3.up * 0.1f, Steepness.Evaluate(Mathf.InverseLerp(minSize,maxSize,sample.Radius)), 100f);
            // }

            //var height = Steepness.Evaluate(Mathf.InverseLerp(minSize, maxSize, sample.Radius);


            var normalisedSample = new Vector2(Mathf.InverseLerp(0, mapSize, sample.Position.x), Mathf.InverseLerp(0, mapSize, sample.Position.y));
            var height = terrainData.GetInterpolatedHeight(normalisedSample.x, normalisedSample.y);

            if (height < 0.5f)
                continue;

            if (RNG.NextFloat() < 0.35f)
                continue;



            if (spawnMap.BilinearSampleFromNormalisedVector2(normalisedSample) < 0.5f)
                continue;

            var steepness = steepnessMap.BilinearSampleFromNormalisedVector2(normalisedSample);// + RNG.NextFloat(-0.1f,0.1f);

            var objToSpawn = steepness > 0.5f ? RockToCreateShallow : RockToCreateSteep;



            var obj = Instantiate(objToSpawn, new Vector3(sample.Position.x, height, sample.Position.y), Quaternion.Euler(0, RNG.NextFloat(-180f, 180f),0), parent.transform);
            obj.transform.localScale = Vector3.one * (sample.Radius);



            _block.SetColor("_Color", Steepness.Evaluate(steepness));
            obj.GetComponentInChildren<MeshRenderer>().SetPropertyBlock(_block);


        }


        /*

        var propCount = 130f;

        
        for (int x = 1; x < propCount; x++)
        {
            for (int y = 1; y < propCount; y++)
            {
                var locX = (x ) / propCount;
                var locY = (y) / propCount;

                var innerHeight = terrainData.GetInterpolatedHeight(locX, locY);

                if (_heightMap.WalkableMap.BilinearSampleFromNormalisedVector2(new Vector2(locX, locY)) > 0.5f && innerHeight > 0.3f)
                {
                    

                    var normal = terrainData.GetInterpolatedNormal(x / propCount, y / propCount);
                    var forward = new Vector3(normal.x, 0, normal.z);
                    var steepness = terrainData.GetSteepness(x / propCount, y / propCount);

                    var RockToCreate = steepness < 62 | RNG.Next(0,50)<1 ? RockToCreateShallow : RockToCreateSteep;
                    //steepness = Util.InverseLerpUnclamped(0f, 150f, steepness);
                    //innerHeight -= (steepness*40);

                    forward.Normalize();

                    var rotation = Quaternion.identity;

                    if(Vector3.Dot(normal,Vector3.up) > 0.05f)
                    {
                        rotation = Quaternion.LookRotation(Vector3.Lerp(normal, forward, 0.5f), Vector3.up);

                        //rotation = Quaternion.LookRotation(forward, Vector3.up);

                        //rotation = Quaternion.Euler(0, RNG.NextFloat(-180f, 180f), 0);

                        var obj = Instantiate(RockToCreate, new Vector3(locX * width, innerHeight, locY * lenght), rotation, parent.transform);

                        //obj.transform.RotateAroundLocal(obj.transform.up, RNG.NextFloat(-180f, 180f));


                        var scaleX = Random.Range(1.3f, 2.2f);

                        if (RNG.Next(0, 20) < 1)
                        {
                            scaleX = Random.Range(2.8f, 4.1f);
                        } else if (RNG.Next(0, 20) < 3)
                        {
                            scaleX = Random.Range(1.8f, 3.5f);
                        }




                        obj.transform.localScale = new Vector3(scaleX, scaleX * 3, scaleX);

                        obj.transform.Translate(new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f) - (1f * scaleX * 2), Random.Range(-0.2f, 0.2f)));
                        //obj.transform.Translate(Vector3.down * 15f);

                    }




                }
                



            }
            
            
        }
        */





    }

    private void CreateMyToys()
    {
        for (int i = 0; i < DetailObjectPools.Length; i++)
        {
            DetailObjectPools[i].SetPhysicalMap(_map);
            DetailObjectPools[i].InitPositions();
        }
    }
}