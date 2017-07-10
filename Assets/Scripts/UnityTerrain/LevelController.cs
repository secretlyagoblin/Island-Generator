using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Maps;

public class LevelController : MonoBehaviour {

    public Texture2D LevelFile;
    public ProcTerrainSettings Settings;
    public Rect Size;

    // Use this for initialization
    void Start () {

        RNG.DateTimeInit();

        var levelMap = Map.MapFromGrayscaleTexture(LevelFile);//.Display();
        var chunks = levelMap.CreateLevelSubMapsFromThisLevelMap(16); //16 is magic number, determines smoothness of gradient

        var sizeX = chunks.GetLength(0);
        var sizeY = chunks.GetLength(0);

        var stack = Map.SetGlobalDisplayStack();

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                chunks[x, y].GetDistanceMap((16 / 2)).Clamp(0f, 0.5f).Remap(Settings.CliffFalloff).Display();
            }
        }

        var totalMap = Map.CreateMapFromSubMapsWithoutResizing(chunks);
        totalMap.Add(totalMap
                    .Clone()
                        .PerlinFill(3f, 0, 0, 1000.23432f)
                        .Remap(0f, 0.1f)
                    );

        chunks = totalMap.GenerateNonUniformSubmapsOverlappingWherePossible(sizeX);


        var terrains = new Terrain[sizeX, sizeY];

        stack.CreateDebugStack(transform);

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                var chunk = chunks[x, y]
                    .Clone()
                    .Resize(TerrainStaticValues.HeightmapResolution, TerrainStaticValues.HeightmapResolution);

                terrains[x,y] = TerrainFactory.MakeTerrain(chunk, new Rect(new Vector2(Size.width * y, Size.height * x), Size.size),Settings);
            }
        }

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                Terrain bottom = null;
                if (x - 1 >= 0)
                    bottom = terrains[x - 1, y];

                Terrain top = null;
                if (x + 1 < sizeX)
                    top = terrains[x + 1, y];

                Terrain left = null;
                if (y - 1 >= 0)
                    left = terrains[x , y-1];

                Terrain right = null;
                if (y + 1 < sizeY)
                    right = terrains[x , y+1];

                terrains[x, y].SetNeighbors(left, top, right, bottom);
            }
        }






    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
