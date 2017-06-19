using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateStandardTerrain : MonoBehaviour {

    public Gradient MajorRocks;
    public Gradient MinorRocks;
    public AnimationCurve Falloff;

    public AnimationCurve LargeSizeMultiplier;
    public AnimationCurve SmallSizeMultiplier;
    public AnimationCurve CliifFalloff;

    private float _width = 220;
    private float _length = 220;
    private float _height = 65;

    private static Vector2 tileAmount = Vector2.one;

    private int _heightmapResoltion = 256 + 1;
    private int _detailResolution = 256;
    private int _detailResolutionPerPatch = 8;
    private int _controlTextureResolution = 256;
    private int _baseTextureReolution = 1024;

    public DetailObjectPool[] DetailObjectPools;

    public SplatCollection SplatCollection;
    public DetailObjectCollection Details;

    public GameObject RockToCreateSteep;
    public GameObject RockToCreateShallow;
    public GameObject Sphere;


    Terrain.TerrainData _map;
    UnityEngine.Terrain _terrain;

    MaterialPropertyBlock _block;

    // Use this for initialization
    void Start()
    {
        _block = new MaterialPropertyBlock();
        CreateAgain();

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

        _map = Terrain.TerrainData.RegionIsland(_heightmapResoltion, new Rect());
        _map.HeightMap.Remap(0, _height);


        TerrainData terrainData = new TerrainData();

        terrainData.size = new Vector3(_width / 8f,
                                        _height,
                                        _length / 8f);

        //terrainData.

        terrainData.baseMapResolution = _baseTextureReolution;
        terrainData.heightmapResolution = _heightmapResoltion;
        terrainData.alphamapResolution = _controlTextureResolution;
        terrainData.SetDetailResolution(_detailResolution, _detailResolutionPerPatch);
        terrainData.SetHeights(0, 0, _map.HeightMap.CreateTerrainMap().Normalise().FloatArray);
        //terrainData.set
        terrainData.splatPrototypes = SplatCollection.GetSplatPrototypes();
        //terrainData.SetAlphamaps(0, 0, SplatCollection.GetAlphaMaps(_map.WalkableMap.Clone().Invert().ThickenOutline(1).Invert().Resize(256, 256)));
        terrainData.detailPrototypes = Details.GetDetailPrototypes();



        //Details.SetDetails(terrainData, _map.WalkableMap);


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

        var mapSize = (float)_width;
        var minSize = mapSize / propCount;
        var maxSize = mapSize / (propCount - 50f);

        var propMap = new PoissonDiscSampler(mapSize, mapSize, minSize);
        var spawnMap = _map.WalkableMap.Clone().ThickenOutline(1);

        var falloffMap = _map.WalkableMap.Clone().GetDistanceMap(15).Clamp(0.5f, 1f).Normalise();


        foreach (var sample in propMap.Samples())
        {
            var normalisedSample = new Vector2(Mathf.InverseLerp(0, mapSize, sample.x), Mathf.InverseLerp(0, mapSize, sample.y));
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
            forward += new Vector3(RNG.NextFloat(-0.4f, 0.4f), 0, RNG.NextFloat(-0.4f, 0.4f));
            var rotation = Quaternion.Euler(0, RNG.NextFloat(-180, 180), 0);
            if (Vector3.Dot(normal, Vector3.up) > 0.05f)
                rotation = Quaternion.LookRotation(forward, Vector3.up);


            var multiplier = LargeSizeMultiplier.Evaluate(steepness);
            var scaledSteepnees = Mathf.Lerp(maxSize, minSize * 0.5f, multiplier);
            var color = MajorRocks.Evaluate(steepness);



            var obj = Instantiate(objToSpawn, new Vector3(sample.x, height, sample.y), rotation, parent.transform);
            obj.transform.localScale = Vector3.one * scaledSteepnees;

            _block.SetColor("_Color", color);
            obj.GetComponentInChildren<MeshRenderer>().SetPropertyBlock(_block);
        }

        propCount = 300f;

        minSize = mapSize / propCount;
        maxSize = mapSize / (propCount - 50f);

        propMap = new PoissonDiscSampler(mapSize, mapSize, minSize);

        var watermap = _map.WaterOutline.Clone().ThickenOutline(1);

        //spawnMap = _map.WalkableMap.Clone();

        foreach (var sample in propMap.Samples())
        {
            var normalisedSample = new Vector2(Mathf.InverseLerp(0, mapSize, sample.x), Mathf.InverseLerp(0, mapSize, sample.y));
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

            var obj = Instantiate(objToSpawn, new Vector3(sample.x, height, sample.y), rotation, parent.transform);
            obj.transform.localScale = Vector3.one * (scale);

            _block.SetColor("_Color", color);
            obj.GetComponentInChildren<MeshRenderer>().SetPropertyBlock(_block);
        }

    }

    private void CreateAgain()
    {
        //GameObject parent = Instantiate(new GameObject("Boostr"));
        //parent.transform.position = new Vector3(0, 0, 0);

        _map = Terrain.TerrainData.DelaunayValleyControlled(_heightmapResoltion, new Rect(), transform, CliifFalloff);
        _map.HeightMap.Remap(0, _height);


        TerrainData terrainData = new TerrainData();

        terrainData.size = new Vector3(_width / 8f,
                                        _height,
                                        _length / 8f);

        var set = Maps.Map.SetGlobalDisplayStack();



        var okayColours = _map.WalkableMap.Clone().GetDistanceMap(8).Resize(_detailResolution, _detailResolution).Invert().Normalise().Clamp(0.4f,1f).Normalise().Display();
        //okayColours = _map.HeightMap.Clone().Normalise().GetAbsoluteBumpMap().Normalise().Clamp(0.1f, 0.9f).Normalise().Display();
        var colour = okayColours.Clone().PerlinFill(15, 0, 0, 123.12123f).Clamp(0.25f,0.75f).Normalise().Display();
        var guff = Maps.Map.Blend(colour, colour.Clone().FillWith(0), okayColours).Display();
        var nuff = Maps.Map.Blend(colour.Invert(), colour.Clone().FillWith(0), okayColours).Display();
        var stuff = okayColours;

        var grass1Falloff = Maps.Map.Blend(_map.WalkableMap.Clone().GetDistanceMap(9).Clamp(0f, 0.5f).Normalise(), 
                _map.WalkableMap.Clone().FillWith(0), 
                _map.WalkableMap.Clone().Invert())
            .Remap(AnimationCurve.EaseInOut(0, 0, 1, 1)).Normalise().Display();


        var grass2Falloff = Maps.Map.Blend(_map.WalkableMap.Clone().GetDistanceMap(10).Clamp(0f, 0.5f).Normalise(),
                _map.WalkableMap.Clone().FillWith(0),
                _map.WalkableMap.Clone().Invert())
            .Normalise().Display();



        //terrainData.

        terrainData.baseMapResolution = _baseTextureReolution;
        terrainData.heightmapResolution = _heightmapResoltion;
        terrainData.alphamapResolution = _controlTextureResolution;
        terrainData.SetDetailResolution(_detailResolution, _detailResolutionPerPatch);
        terrainData.SetHeights(0, 0, _map.HeightMap.CreateTerrainMap().Normalise().FloatArray);

        terrainData.splatPrototypes = SplatCollection.GetSplatPrototypes();
        terrainData.SetAlphamaps(0, 0, SplatCollection.GetAlphaMaps(new Maps.Map[]{ guff, stuff.Invert(), nuff }));

        terrainData.detailPrototypes = Details.GetDetailPrototypes();
        Details.SetDetails(terrainData, new Maps.Map[] { nuff.Clamp(0.25f,0.75f).Normalise(), grass1Falloff.Resize(_detailResolution, _detailResolution) });
       // Details.SetDetails(terrainData, new Maps.Map[] { grass2Falloff.Resize(_detailResolution, _detailResolution).Remap(0,1f), grass1Falloff.Resize(_detailResolution, _detailResolution) });

        //Details.SetDetails(terrainData, new Maps.Map[] { _map.WalkableMap.Clone().Resize(_detailResolution, _detailResolution).Display() });
        //Details.SetDetails(terrainData, new Maps.Map[] { _map.WalkableMap.Clone().Display() });
        //Details.SetDetails(terrainData, new Maps.Map[] { godWhy });

        set.CreateDebugStack(2f);

        //terrainData.name = name;
        GameObject terrainObj = UnityEngine.Terrain.CreateTerrainGameObject(terrainData);
        _terrain = terrainObj.GetComponent<UnityEngine.Terrain>();
        _terrain.detailObjectDistance = 250f;

        //terrain.name = name;
        //terrain.transform.parent = parent.transform;
        terrainObj.transform.position = new Vector3(0, 0, 0);

        var parent = new GameObject();



        var spawnMap = _map.WalkableMap.Clone();//.Invert().ThickenOutline().Invert();

        var okay = UnityTerrainHelpers.PropSample(_map.HeightMap, spawnMap.Clone().Invert().ThickenOutline().Invert(), 200);

        spawnMap = spawnMap.ThickenOutline();

        for (int i = 0; i < okay.Count; i++)
        {            
            var a = okay[i];

            if (a.Size.y == 0)
                continue;

            var height = terrainData.GetInterpolatedHeight(a.SamplePoint.x, a.SamplePoint.y);

            if (height < 0.5f)
                continue;

            if (spawnMap.BilinearSampleFromNormalisedVector2(a.SamplePoint) < 0.5f)
                continue;

            var gob = Instantiate(RockToCreateShallow);

            var scale = Random.Range(1f, 1.3f);
            
            gob.transform.position = (new Vector3(_width * a.Min.x, a.Min.y, _width * a.Min.z));
            gob.transform.localScale = (new Vector3(_width*a.Size.x * scale, a.Size.y, _width * a.Size.z * scale));
            gob.transform.RotateAround(new Vector3(_width * a.Center.x, a.Center.y, _width * a.Center.z), Vector3.up, Random.Range(0, 360f));

            gob.name = "Prop " + a.Scale;

            gob.transform.parent = parent.transform;
        }
    }

    private void CreateMyToys()
    {
        for (int i = 0; i < DetailObjectPools.Length; i++)
        {
            DetailObjectPools[i].SetPhysicalMap(_terrain, _map.WalkableMap);
            DetailObjectPools[i].InitPositions();
        }
    }
}