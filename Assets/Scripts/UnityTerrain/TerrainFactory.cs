using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainFactory {

    private static Maps.Map _levelMap;
    private static ProcTerrainSettings _settings;

    private static float _width = 150;
    private static float _length = 150;
    private static float _height = 150;

    private static Vector2 tileAmount = Vector2.one;

    private static int _heightmapResolution = 256 + 1;
    private static int _detailResolution = 256;
    private static int _detailResolutionPerPatch = 8;
    private static int _controlTextureResolution = 256;
    private static int _baseTextureResolution = 1024;

    static ProcTerrain.TerrainData _map;
    static UnityEngine.Terrain _terrain;

    static MaterialPropertyBlock _block;

    public static Terrain MakeTerrain(Maps.Map levelMap, Rect rect, ProcTerrainSettings settings)
    {
        _levelMap = levelMap;
        _settings = settings;
        SetTerrainValues(rect);
        return GetTerrain(rect.position);
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

    private static Terrain GetTerrain(Vector2 position)
    {
        TerrainData terrainData = new TerrainData();

        terrainData.baseMapResolution = _baseTextureResolution;
        terrainData.heightmapResolution = _heightmapResolution;
        terrainData.size = new Vector3(_width, _height, _length);
        terrainData.alphamapResolution = _controlTextureResolution;
        terrainData.SetDetailResolution(_detailResolution, _detailResolutionPerPatch);
        terrainData.SetHeights(0, 0, _levelMap.FloatArray);

        terrainData.splatPrototypes = _settings.SplatCollection.GetSplatPrototypes();
        //terrainData.SetAlphamaps(0, 0, _settings.SplatCollection.GetAlphaMaps(new Maps.Map[] { guff, stuff.Invert(), nuff }));

        //terrainData.detailPrototypes = _settings.Details.GetDetailPrototypes();
        //_settings.Details.SetDetails(terrainData, new Maps.Map[] { nuff.Clamp(0.25f, 0.75f).Normalise(), grass1Falloff.Resize(_detailResolution, _detailResolution) });
        // Details.SetDetails(terrainData, new Maps.Map[] { grass2Falloff.Resize(_detailResolution, _detailResolution).Remap(0,1f), grass1Falloff.Resize(_detailResolution, _detailResolution) });


        GameObject terrainObj = Terrain.CreateTerrainGameObject(terrainData);
        terrainObj.name = "Chunk " + position.x + " " + position.y;
        _terrain = terrainObj.GetComponent<Terrain>();
        _terrain.detailObjectDistance = 150f;
        _terrain.heightmapPixelError = 150f;
        _terrain.basemapDistance = 200f;

        //terrain.name = name;
        //terrain.transform.parent = parent.transform;
        terrainObj.transform.position = new Vector3(position.x,0,position.y);

        return _terrain;

    }
}

