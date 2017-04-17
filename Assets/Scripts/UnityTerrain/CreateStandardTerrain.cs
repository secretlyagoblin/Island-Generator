using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateStandardTerrain : MonoBehaviour {

    public Gradient MajorRocks;
    public Gradient MinorRocks;
    public AnimationCurve Falloff;

    public AnimationCurve LargeSizeMultiplier;
    public AnimationCurve SmallSizeMultiplier;

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
        terrainData.SetAlphamaps(0, 0, SplatCollection.GetAlphaMaps(_map.WalkableMap.Clone().Invert().ThickenOutline(1).Invert().Resize(256, 256)));
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

        var propCount = 180f;

        var mapSize = (float)width;
        var minSize = mapSize / propCount;
        var maxSize = mapSize / (propCount - 50f);

        var propMap = new PoissonDiscSampler(mapSize, mapSize, minSize, maxSize, steepnessMap);
        var spawnMap = _map.WalkableMap.Clone().ThickenOutline(1);

        var falloffMap = _map.WalkableMap.Clone().GetDistanceMap(15).Clamp(0.5f,1f).Normalise();


        foreach (var sample in propMap.Samples())
        {
            var normalisedSample = new Vector2(Mathf.InverseLerp(0, mapSize, sample.Position.x), Mathf.InverseLerp(0, mapSize, sample.Position.y));
            var height = terrainData.GetInterpolatedHeight(normalisedSample.x, normalisedSample.y);

            if (height < 0.5f)
                continue;

            if (spawnMap.BilinearSampleFromNormalisedVector2(normalisedSample) < 0.5f)
                continue;

            var dist = falloffMap.BilinearSampleFromNormalisedVector2(normalisedSample);

            if (dist < 0.15f)
                continue;

            var steepness = steepnessMap.BilinearSampleFromNormalisedVector2(normalisedSample);// + RNG.NextFloat(-0.1f,0.1f);

            var objToSpawn = steepness > 0.5f ? RockToCreateShallow : RockToCreateSteep;

            

            var normal = terrainData.GetInterpolatedNormal(normalisedSample.x, normalisedSample.y);
            var forward = new Vector3(normal.x, 0, normal.z);
            forward += new Vector3(RNG.NextFloat(-0.4f,0.4f),0,RNG.NextFloat(-0.4f,0.4f));
            var rotation = Quaternion.Euler(0, RNG.NextFloat(-180, 180), 0);
            if (Vector3.Dot(normal, Vector3.up) > 0.05f)
                rotation = Quaternion.LookRotation(forward, Vector3.up);


            var multiplier = LargeSizeMultiplier.Evaluate(steepness);
            var scaledSteepnees = Mathf.Lerp(maxSize, minSize*0.5f, multiplier);
            var color = MajorRocks.Evaluate(steepness);



            var obj = Instantiate(objToSpawn, new Vector3(sample.Position.x, height, sample.Position.y), rotation, parent.transform);
            obj.transform.localScale = Vector3.one * scaledSteepnees;

            _block.SetColor("_Color", color);
            obj.GetComponentInChildren<MeshRenderer>().SetPropertyBlock(_block);
        }

        propCount = 300f;

        minSize = mapSize / propCount;
        maxSize = mapSize / (propCount - 50f);

        propMap = new PoissonDiscSampler(mapSize, mapSize, minSize, maxSize, steepnessMap);

        var watermap = _map.WaterOutline.Clone().ThickenOutline(1);

        //spawnMap = _map.WalkableMap.Clone();

        foreach (var sample in propMap.Samples())
        {
            var normalisedSample = new Vector2(Mathf.InverseLerp(0, mapSize, sample.Position.x), Mathf.InverseLerp(0, mapSize, sample.Position.y));
            var height = terrainData.GetInterpolatedHeight(normalisedSample.x, normalisedSample.y);

            if (height < 0.5f)
                continue;

            if (spawnMap.BilinearSampleFromNormalisedVector2(normalisedSample) < 0.5f)
                continue;

            var dist = falloffMap.BilinearSampleFromNormalisedVector2(normalisedSample);

            if (dist > 0.15f)
                continue;

            var steepness = steepnessMap.BilinearSampleFromNormalisedVector2(normalisedSample);// + RNG.NextFloat(-0.1f,0.1f);

            var objToSpawn = steepness > 0.5f ? RockToCreateShallow : RockToCreateSteep;

            var multiplier = SmallSizeMultiplier.Evaluate(steepness);

            var color = MinorRocks.Evaluate(steepness);


            var scaledSteepnees = Mathf.Lerp(maxSize, minSize, multiplier);
            color = MinorRocks.Evaluate(multiplier);

            var scale = scaledSteepnees;

            var waterSample = watermap.BilinearSampleFromNormalisedVector2(normalisedSample);

            var normal = terrainData.GetInterpolatedNormal(normalisedSample.x, normalisedSample.y);
            var forward = new Vector3(normal.x, 0, normal.z);
            forward += new Vector3(RNG.NextFloat(-0.4f, 0.4f), 0, RNG.NextFloat(-0.4f, 0.4f));
            var rotation = Quaternion.Euler(0, RNG.NextFloat(-180, 180), 0);
            if (Vector3.Dot(normal, Vector3.up) > 0.05f)
                rotation = Quaternion.LookRotation(forward, Vector3.up);



            if (waterSample > 0.5f)
            {
                objToSpawn = RNG.NextFloat() > 0.3f ? RockToCreateShallow : RockToCreateSteep;                
                //color = Color.red;

                if (waterSample > 0.99f)
                {
                    //color = Color.green;
                    objToSpawn = RockToCreateShallow;
                    //scale = scale * (RNG.NextFloat(0.7f, 1.1f));
                }

            }

            if (_map.LandOutline.BilinearSampleFromNormalisedVector2(normalisedSample) > 0.5f)
            {
                objToSpawn = RNG.NextFloat() > 0.5f ? RockToCreateShallow : RockToCreateSteep;
                //color = Color.blue;
            }

            var obj = Instantiate(objToSpawn, new Vector3(sample.Position.x, height, sample.Position.y), rotation, parent.transform);
            obj.transform.localScale = Vector3.one * (scale);

            _block.SetColor("_Color", color);
            obj.GetComponentInChildren<MeshRenderer>().SetPropertyBlock(_block);
        }

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