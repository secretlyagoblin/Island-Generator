using UnityEngine;
using System.Collections;

public class UnityTerrainGenerator {

	// Use this for initialization
	static GameObject CreateTerrain (Map map, Vector3 size) {

        GameObject TerrainObj = new GameObject("TerrainObj");

        TerrainData _TerrainData = new TerrainData();

        _TerrainData.size = size;
        _TerrainData.heightmapResolution = 512;
        _TerrainData.baseMapResolution = 1024;
        _TerrainData.SetDetailResolution(1024, 16);
        _TerrainData.SetHeights(0, 0, map.FloatArray);

        int _heightmapWidth = _TerrainData.heightmapWidth;
        int _heightmapHeight = _TerrainData.heightmapHeight;

        TerrainCollider _TerrainCollider = TerrainObj.AddComponent<TerrainCollider>();
        Terrain _Terrain2 = TerrainObj.AddComponent<Terrain>();

        _TerrainCollider.terrainData = _TerrainData;
        _Terrain2.terrainData = _TerrainData;

        return TerrainObj;


    }
}
