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

        var levelMap = Map.MapFromGrayscaleTexture(LevelFile);//.Display();
        var chunks = levelMap.CreateSubMapsFromThisLevelMap(16); //16 is magic number, determines smoothness of gradient

        var stack = Map.SetGlobalDisplayStack();

        for (int x = 0; x < chunks.GetLength(0); x++)
        {
            for (int y = 0; y < chunks.GetLength(1); y++)
            {
                chunks[x, y].GetDistanceMap((16 / 2)).Clamp(0f, 0.5f).Remap(Settings.CliffFalloff).Display();
            }
        }

        stack.CreateDebugStack(transform);

        for (int x = 0; x < chunks.GetLength(0); x++)
        {
            for (int y = 0; y < chunks.GetLength(1); y++)
            {
                var chunk = chunks[x, y].Clone().Resize(TerrainStaticValues.HeightmapResolution, TerrainStaticValues.HeightmapResolution);
                TerrainFactory.MakeTerrain(chunk, new Rect(new Vector2(Size.width * y, Size.height * x), Size.size),Settings);
            }
        }






    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
