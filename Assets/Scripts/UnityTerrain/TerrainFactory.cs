using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainFactory {

    /// <summary>
    /// All things to do with creating the terrainChunk will happen in this, make it as messy as you want. 
    /// The terrainchunk itself should just hold as little data as possible and be delt with by a central manager.
    /// 
    /// TerrainFactory makes terrainchunk, chunkmanager manages terrainchunk
    /// </summary>

    private static Maps.Map _levelMap;
    private static ProcTerrainSettings _settings;

    private static float _width = 150;
    private static float _length = 150;
    private static float _height = 150;

    private static Vector2 tileAmount = Vector2.one;

    private static int _heightmapResolution;
    private static int _detailResolution;
    private static int _detailResolutionPerPatch;
    private static int _controlTextureResolution;
    private static int _baseTextureResolution;

    static ProcTerrain.TerrainData _map;
    static UnityEngine.Terrain _terrain;

    static MaterialPropertyBlock _block;

    public static TerrainChunk MakeTerrainChunk(Maps.Map levelMap, Coord coord, Rect rect, ProcTerrainSettings settings)
    {


        _levelMap = levelMap;
        _settings = settings;
        SetTerrainValues(rect);
        return new TerrainChunk(GetTerrain(rect.position,coord), GetProps(rect));
    }



    private static void SetTerrainValues(Rect rect)
    {
        _heightmapResolution = TerrainStaticValues.HeightmapResolution;
        _detailResolution = TerrainStaticValues.DetailMapResolution;
        _detailResolutionPerPatch = TerrainStaticValues.DetailResolutionPerPatch;
        _controlTextureResolution = TerrainStaticValues.ControlTextureResolution;
        _baseTextureResolution = 1024;
        _width = rect.width;
        _length = rect.height;
    }

    private static Terrain GetTerrain(Vector2 position, Coord tile)
    {
        if (_levelMap.IsBlank())
            return null;

        // Creating Map
        _map = ProcTerrain.TerrainData.DelaunayValleyControlledAssumingLevelSet(_levelMap, new Rect(), _settings.CliffFalloff);
        var hMap = Maps.Map.Blend(_map.HeightMap.Normalise(), _levelMap, ProcTerrain.TerrainData.Falloffmap(TerrainStaticValues.HeightmapResolution));
        //_map.HeightMap.Remap(0, _height);

        //Creating TerrainData
        TerrainData terrainData = new TerrainData();

        terrainData.baseMapResolution = _baseTextureResolution;
        terrainData.heightmapResolution = _heightmapResolution;
        terrainData.size = new Vector3(_width, _height, _length);
        terrainData.alphamapResolution = _controlTextureResolution;
        terrainData.SetDetailResolution(_detailResolution, _detailResolutionPerPatch);
        terrainData.SetHeights(0, 0, hMap.CreateTerrainMap().FloatArray);
        //terrainData.SetHeights(0, 0, _levelMap.CreateTerrainMap().FloatArray);

        //Creating Colours and stuff
        var colourBase = _map.WalkableMap.Clone().GetDistanceMap(8).Resize(_detailResolution, _detailResolution).Invert().Normalise().Clamp(0.4f, 1f).Normalise().Display();
        var colourPerlin = colourBase.Clone().PerlinFill(15, tile.x, tile.y, 123.12123f).Clamp(0.25f, 0.75f).Normalise().Display();
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

        //Setting up prototypes
        terrainData.splatPrototypes = _settings.SplatCollection.GetSplatPrototypes();
        terrainData.SetAlphamaps(0, 0, _settings.SplatCollection.GetAlphaMaps(new Maps.Map[] { perlinA, colourBase.Invert(), perlinB }));

        terrainData.detailPrototypes = _settings.Details.GetDetailPrototypes();
        _settings.Details.SetDetails(terrainData, new Maps.Map[] { perlinB.Clamp(0.25f, 0.75f).Normalise(), grass1Falloff.Resize(_detailResolution, _detailResolution) });


        GameObject terrainObj = Terrain.CreateTerrainGameObject(terrainData);
        terrainObj.name = "Chunk " + position.x + " " + position.y;
        _terrain = terrainObj.GetComponent<Terrain>();
        _terrain.detailObjectDistance = 150f;
        _terrain.heightmapPixelError = 50f;
        _terrain.basemapDistance = 700f;

        terrainObj.transform.position = new Vector3(position.x,0,position.y);

        return _terrain;

    }

    private static Matrix4x4[][] GetProps(Rect rect)
    {

        var output = new List<Matrix4x4[]>();
        var counter = new List<Matrix4x4>();

        if (_levelMap.IsBlank())
            return output.ToArray();

        var spawnMap = _map.WalkableMap.Clone();//.Invert().ThickenOutline().Invert();
        var objectLocations = UnityTerrainHelpers.PropSample(_map.HeightMap.Clone().Normalise(), spawnMap.Clone().Invert().ThickenOutline().Invert(), 64);
        spawnMap = spawnMap.ThickenOutline();


        for (int i = 0; i < objectLocations.Count; i++)
        {
            var a = objectLocations[i];

            if (a.Size.y == 0)
                continue;

            var height = _terrain.terrainData.GetInterpolatedHeight(a.SamplePoint.x + rect.position.x, a.SamplePoint.y + rect.position.y);

            if (height < 0.5f)
                continue;

            if (spawnMap.BilinearSampleFromNormalisedVector2(a.SamplePoint) < 0.5f)
                continue;

            //GameObject gob = null;

            //if (RNG.NextFloat() < 0.7f)
            //    gob = Instantiate(RockToCreateShallow);
            //else
            //    gob = Instantiate(Tree);



            var scale = Random.Range(1f, 1.3f);



            var position = (new Vector3(_width * a.Min.x + rect.position.x, a.Min.y * _height, _width * a.Min.z + rect.position.y));
            var localScale = (new Vector3(_width * a.Size.x * scale, a.Size.y*_height, _width * a.Size.z * scale));
            var rotation = Quaternion.LookRotation(new Vector3(RNG.NextFloat(-1f, 1f), 0, RNG.NextFloat(-1f, 1f)), Vector3.up);

            //var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //cube.transform.position = position;
            //cube.transform.localScale = localScale;
            //cube.transform.rotation = rotation;

            var matrix = Matrix4x4.TRS(position, rotation, localScale);



            counter.Add(matrix);
            if(counter.Count == 1023)
            {
                output.Add(counter.ToArray());
                counter.Clear();
            }

        }

        return output.ToArray();
    }
}

