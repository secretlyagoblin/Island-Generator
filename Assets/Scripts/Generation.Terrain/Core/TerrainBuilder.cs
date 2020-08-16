using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainBuilder
{
    // Start is called before the first frame update

    public static TerrainData BuildTerrainData(float[,] values, Vector2 size, TerrainManifest manifest)
    {
        var terrainData = new TerrainData();

        terrainData.baseMapResolution = 1024;
        terrainData.heightmapResolution = 1025;
        terrainData.size = new Vector3(size.x, manifest.MaxHeight - manifest.MinHeight, size.y);
        terrainData.alphamapResolution = 256;
        terrainData.SetHeights(0, 0, values);

        return terrainData;
    }

    public static Terrain BuildTerrain(TerrainData data, Vector3 position)
    {

        GameObject terrainObj = UnityEngine.Terrain.CreateTerrainGameObject(data);
        terrainObj.transform.position = position;
        var terrain = terrainObj.GetComponent<UnityEngine.Terrain>();
        terrain.drawInstanced = true;
        //terrain.detailObjectDistance = 150f;
        terrain.heightmapPixelError = 6f;
        //terrain.basemapDistance = 200f;

        return terrain;



        //terrainData.heightmapResolution = chunks.Size * chunk.Multiplier;


    }
}
