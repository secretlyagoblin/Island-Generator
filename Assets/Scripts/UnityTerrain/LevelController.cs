using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Maps;

public class LevelController : MonoBehaviour {

    public Texture2D LevelFile;
    public ProcTerrainSettings Settings;
    public Rect Size;

    TerrainChunk[,] _terrainChunks;

    // Use this for initialization
    void Start () {

        RNG.DateTimeInit();

        var levelMap = Map.MapFromGrayscaleTexture(LevelFile);//.Display();
        var chunks = levelMap.CreateLevelSubMapsFromThisLevelMap(32); //16 is magic number, determines smoothness of gradient

        var sizeX = chunks.GetLength(0);
        var sizeY = chunks.GetLength(0);

        var stack = Map.SetGlobalDisplayStack();

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                chunks[x, y].GetDistanceMap((32 / 2)).Clamp(0f, 0.5f).Remap(Settings.CliffFalloff).Display();
            }
        }

        var totalMap = Map.CreateMapFromSubMapsWithoutResizing(chunks).Display();
        totalMap.Add(totalMap
                    .Clone()
                        .PerlinFill(6f, 0, 0, 1000.23432f)
                        .Remap(0f, 0.1f)
                    )
                    .SmoothMap(1)
                    .Display()
                    .Normalise();

        chunks = totalMap.GenerateNonUniformSubmapsOverlappingWherePossible(sizeX);


        _terrainChunks = new TerrainChunk[sizeX, sizeY];

        stack.CreateDebugStack(transform);

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                var chunk = chunks[x, y]
                    .Clone()
                    .Resize(TerrainStaticValues.HeightmapResolution, TerrainStaticValues.HeightmapResolution);

                _terrainChunks[x,y] = TerrainFactory.MakeTerrainChunk(chunk, new Rect(new Vector2(Size.width * x, Size.height * y), Size.size),Settings);
            }
        }

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                Terrain left = null;
                if (x - 1 >= 0)
                    left = _terrainChunks[x - 1, y].Terrain;

                Terrain right = null;
                if (x + 1 < sizeX)
                    right = _terrainChunks[x + 1, y].Terrain;

                Terrain bottom = null;
                if (y - 1 >= 0)
                    bottom = _terrainChunks[x , y-1].Terrain;

                Terrain top = null;
                if (y + 1 < sizeY)
                    top = _terrainChunks[x , y+1].Terrain;

                _terrainChunks[x, y].Terrain.SetNeighbors(left, top, right, bottom);
            }
        }






    }
	
	// Update is called once per frame
	void Update () {

        //for (int x = 0; x < _terrainChunks.GetLength(0); x++)
        //{
        //    for (int y = 0; y < _terrainChunks.GetLength(1); y++)
        //    {
        //        for (int i = 0; i < _terrainChunks[x, y].Props.Length; i++)
        //        {
        //            Graphics.DrawMeshInstanced(Settings.DetailMesh, 0, Settings.DetailMaterial, _terrainChunks[x, y].Props[i]);
        //        }
        //        ;//_terrainChunks[x,y].Props
        //    }
        //}
		
	}
}
