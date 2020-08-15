using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainBuilder
{
    // Start is called before the first frame update
    public static Terrain BuildTerrain(TerrainChunk chunk)
    {
        var terrainData = new TerrainData();

        terrainData.baseMapResolution = 1024;
        terrainData.heightmapResolution = 1025;
        terrainData.size = new Vector3(chunk.ScaledBounds.size.x, chunk.MaxValue - chunk.MinValue, chunk.ScaledBounds.size.y);
        terrainData.alphamapResolution = 256;
        terrainData.SetHeights(0, 0, chunk.GetResizedHeightmap(1025));       

        GameObject terrainObj = UnityEngine.Terrain.CreateTerrainGameObject(terrainData);
        var terrain = terrainObj.GetComponent<UnityEngine.Terrain>();
        terrain.drawInstanced = true;
        //terrain.detailObjectDistance = 150f;
        terrain.heightmapPixelError = 6f;
        //terrain.basemapDistance = 200f;

        return terrain;



        //terrainData.heightmapResolution = chunks.Size * chunk.Multiplier;


    }
}
