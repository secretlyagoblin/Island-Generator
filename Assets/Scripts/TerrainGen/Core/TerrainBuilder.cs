using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainBuilder
{
    // Start is called before the first frame update
    public static void BuildTerrain(TerrainChunkCollection chunks)
    {
        var chunk = chunks._chunks[0];
        var terrainData = new TerrainData();
        terrainData.size = chunk.ScaledBounds.size+(Vector3Int.up*1000);
        terrainData.heightmapResolution = chunks.Size * chunk.Multiplier;
        terrainData.SetHeights(0, 0, chunk.GetHeightmap());

        var terrain = new Terrain
        {
            terrainData = terrainData
        };
    }
}
