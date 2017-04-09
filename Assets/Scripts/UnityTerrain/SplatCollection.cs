using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain/Splat Collection")]
public class SplatCollection : ScriptableObject {

    public List<TerrainSplat> TerrainSplats;

    public SplatPrototype[] GetSplatPrototypes()
    {
        var count = TerrainSplats.Count;

        var output = new SplatPrototype[count];

        for (int i = 0; i < TerrainSplats.Count; i++)
        {
            output[i] = TerrainSplats[i].GetSplat();
        }

        return output;
    }
    
    public float[,,] GetAlphaMaps(Maps.Map baseMap)
    {
        var sizeX = baseMap.SizeX;
        var sizeY = baseMap.SizeY;
        var splatCount = TerrainSplats.Count;

        var map = new float[sizeX, sizeY, splatCount];

        // For each point on the alphamap...
        for (var x = 0; x < sizeX; x++)
        {
            for (var y = 0; y < sizeY; y++)
            {
                // Get the normalized terrain coordinate that
                // corresponds to the the point.
                var normX = x * 1.0 / (sizeX - 1);
                var normY = y * 1.0 / (sizeY - 1);

                for (int z = 0; z < splatCount; z++)
                {

                    map[y, x, z] = baseMap[x, y] == z ? 1 : 0;

                }
            }
        }
        return map;        
    }

}
