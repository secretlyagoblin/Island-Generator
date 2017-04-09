using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain/Detail Object Collection")]
public class DetailObjectCollection : ScriptableObject {

    public DetailObject[] DetailObjects;

    public DetailPrototype[] GetDetailPrototypes()
    {
        var count = DetailObjects.Length;

        var output = new DetailPrototype[count];

        for (int i = 0; i < count; i++)
        {
            output[i] = DetailObjects[i].GetDetail();
        }

        return output;
    }

    public void SetDetails(TerrainData terrainData, Maps.Map baseMap)
    {
        var sizeX = baseMap.SizeX;
        var sizeY = baseMap.SizeY;
        var count = DetailObjects.Length;



        //var map = new float[sizeX, sizeY, count];

        for (int z = 0; z < count; z++)
        {
            var map = new int[sizeX, sizeY];

            // For each point on the alphamap...
            for (var x = 0; x < sizeX; x++)
            {
                for (var y = 0; y < sizeY; y++)
                {
                    // Get the normalized terrain coordinate that
                    // corresponds to the the point.

                    //map[y, x] = 0;
                    //
                    //if (baseMap[x, y] == z)
                    //{
                    //    if (Mathf.PerlinNoise(x * 0.12342f, y * 0.12342f) < 0.5f)
                    //        map[y, x] = RNG.Next(30, 110);
                    //}



                    map[y, x] = baseMap[x, y] == z ? (int)(RNG.Next(200,300)* Mathf.Lerp(-0.2f,1.4f,(Mathf.PerlinNoise(x * 0.12342f, y * 0.12342f)))) : 0;
                }
            }
            terrainData.SetDetailLayer(0, 0, z, map);
        }
    }
}
