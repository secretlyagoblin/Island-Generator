using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateStandardTerrain : MonoBehaviour {

    public Texture2D LevelFile;

    public Gradient MajorRocks;
    public Gradient MinorRocks;
    public AnimationCurve Falloff;

    public AnimationCurve LargeSizeMultiplier;
    public AnimationCurve SmallSizeMultiplier;
    public AnimationCurve CliffFalloff;

    private float _width = 150;
    private float _length = 150;
    private float _height = 65;

    private static Vector2 tileAmount = Vector2.one;

    private int _heightmapResolution = 256 + 1;
    private int _detailResolution = 256;
    private int _detailResolutionPerPatch = 8;
    private int _controlTextureResolution = 256;
    private int _baseTextureResolution = 1024;

    public DetailObjectPool[] DetailObjectPools;

    public SplatCollection SplatCollection;
    public DetailObjectCollection Details;

    public GameObject RockToCreateSteep;
    public GameObject RockToCreateShallow;
    public GameObject Tree;
    public GameObject Sphere;


    ProcTerrain.TerrainData _map;
    UnityEngine.Terrain _terrain;

    MaterialPropertyBlock _block;

    // Use this for initialization
    void Start()
    {
        _block = new MaterialPropertyBlock();
        SetTerrainValues(256/2);
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

    private void SetTerrainValues(int size)
    {
        var multiplierX = LevelFile.width / 3;
        var multiplierY = LevelFile.height / 3;

        size = size * multiplierX;

        _heightmapResolution = size + 1;
        _detailResolution = size;
        _detailResolutionPerPatch = 8;
        _controlTextureResolution = size;
        _baseTextureResolution = 1024;
        _width = _width * multiplierX;
        _length = _length * multiplierY;
    }


    private void CreateAgain()
    {
        //GameObject parent = Instantiate(new GameObject("Boostr"));
        //parent.transform.position = new Vector3(0, 0, 0);


        var set = Maps.Map.SetGlobalDisplayStack();
        _map = ProcTerrain.TerrainData.DelaunayValleyControlled(_heightmapResolution, new Rect(), transform,LevelFile, CliffFalloff);
        _map.HeightMap.Remap(0, _height);


        TerrainData terrainData = new TerrainData();


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

        terrainData.baseMapResolution = _baseTextureResolution;
        terrainData.heightmapResolution = _heightmapResolution;
        terrainData.size = new Vector3(_width, _height, _length);
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

        set.CreateDebugStack(-12f);

        //terrainData.name = name;
        GameObject terrainObj = UnityEngine.Terrain.CreateTerrainGameObject(terrainData);
        _terrain = terrainObj.GetComponent<UnityEngine.Terrain>();
        _terrain.detailObjectDistance = 150f;
        _terrain.heightmapPixelError = 150f;
        _terrain.basemapDistance = 200f;

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

            GameObject gob = null;

            if (RNG.NextFloat() < 0.7f)
                gob = Instantiate(RockToCreateShallow);
            else
                gob = Instantiate(Tree);



            var scale = Random.Range(1f, 1.3f);
            
            gob.transform.position = (new Vector3(_width * a.Min.x, a.Min.y, _width * a.Min.z));
            gob.transform.localScale = (new Vector3(_width*a.Size.x * scale, a.Size.y, _width * a.Size.z * scale));
            gob.transform.RotateAround(new Vector3(_width * a.Center.x, a.Center.y, _width * a.Center.z), Vector3.up, Random.Range(0, 360f));

            gob.name = "Prop " + a.Scale;

            gob.transform.parent = parent.transform;
        }
    }

    private void CreateAgainFinal(Maps.Map map)
    {
        //GameObject parent = Instantiate(new GameObject("Boostr"));
        //parent.transform.position = new Vector3(0, 0, 0);


        var set = Maps.Map.SetGlobalDisplayStack();
        _map = ProcTerrain.TerrainData.DelaunayValleyControlledAssumingLevelSet(map, new Rect(), CliffFalloff);
        _map.HeightMap.Remap(0, _height);

        TerrainData terrainData = new TerrainData();


        var colourBase = _map.WalkableMap.Clone().GetDistanceMap(8).Resize(_detailResolution, _detailResolution).Invert().Normalise().Clamp(0.4f, 1f).Normalise().Display();
        var colourPerlin = colourBase.Clone().PerlinFill(15, 0, 0, 123.12123f).Clamp(0.25f, 0.75f).Normalise().Display();
        var perlinA = Maps.Map.Blend(colourPerlin, colourPerlin.Clone().FillWith(0), colourBase).Display();
        var perlinB = Maps.Map.Blend(colourPerlin.Invert(), colourPerlin.Clone().FillWith(0), colourBase).Display();

        var grass1Falloff = Maps.Map.Blend(_map.WalkableMap.Clone().GetDistanceMap(9).Clamp(0f, 0.5f).Normalise(),
                _map.WalkableMap.Clone().FillWith(0),
                _map.WalkableMap.Clone().Invert())
            .Remap(AnimationCurve.EaseInOut(0, 0, 1, 1)).Normalise().Display();

        var grass2Falloff = Maps.Map.Blend(_map.WalkableMap.Clone().GetDistanceMap(10).Clamp(0f, 0.5f).Normalise(),
                _map.WalkableMap.Clone().FillWith(0),
                _map.WalkableMap.Clone().Invert())
            .Normalise().Display();



        //terrainData.

        terrainData.baseMapResolution = _baseTextureResolution;
        terrainData.heightmapResolution = _heightmapResolution;
        terrainData.size = new Vector3(_width, _height, _length);
        terrainData.alphamapResolution = _controlTextureResolution;
        terrainData.SetDetailResolution(_detailResolution, _detailResolutionPerPatch);
        terrainData.SetHeights(0, 0, _map.HeightMap.CreateTerrainMap().Normalise().FloatArray);

        terrainData.splatPrototypes = SplatCollection.GetSplatPrototypes();
        terrainData.SetAlphamaps(0, 0, SplatCollection.GetAlphaMaps(new Maps.Map[] { perlinA, colourBase.Invert(), perlinB }));

        terrainData.detailPrototypes = Details.GetDetailPrototypes();
        Details.SetDetails(terrainData, new Maps.Map[] { perlinB.Clamp(0.25f, 0.75f).Normalise(), grass1Falloff.Resize(_detailResolution, _detailResolution) });
        // Details.SetDetails(terrainData, new Maps.Map[] { grass2Falloff.Resize(_detailResolution, _detailResolution).Remap(0,1f), grass1Falloff.Resize(_detailResolution, _detailResolution) });

        //Details.SetDetails(terrainData, new Maps.Map[] { _map.WalkableMap.Clone().Resize(_detailResolution, _detailResolution).Display() });
        //Details.SetDetails(terrainData, new Maps.Map[] { _map.WalkableMap.Clone().Display() });
        //Details.SetDetails(terrainData, new Maps.Map[] { godWhy });

        set.CreateDebugStack(-12f);

        //terrainData.name = name;
        GameObject terrainObj = UnityEngine.Terrain.CreateTerrainGameObject(terrainData);
        _terrain = terrainObj.GetComponent<UnityEngine.Terrain>();
        _terrain.detailObjectDistance = 150f;
        _terrain.heightmapPixelError = 150f;
        _terrain.basemapDistance = 200f;

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

            GameObject gob = null;

            if (RNG.NextFloat() < 0.7f)
                gob = Instantiate(RockToCreateShallow);
            else
                gob = Instantiate(Tree);



            var scale = Random.Range(1f, 1.3f);

            gob.transform.position = (new Vector3(_width * a.Min.x, a.Min.y, _width * a.Min.z));
            gob.transform.localScale = (new Vector3(_width * a.Size.x * scale, a.Size.y, _width * a.Size.z * scale));
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