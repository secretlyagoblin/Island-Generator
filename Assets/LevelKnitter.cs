using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelKnitter : MonoBehaviour {

    public Texture2D testTexture;

	// Use this for initialization
	void Start () {

        RNG.DateTimeInit();

        var chunkSize = 32;

        var stack = Maps.Map.SetGlobalDisplayStack();

        var map = Maps.Map.MapFromGrayscaleTexture(testTexture).Display();
        var chunks = map.CreateLevelSubMapsFromThisLevelMap(chunkSize);

        for (int x = 0; x < chunks.GetLength(0); x++)
        {
            for (int y = 0; y < chunks.GetLength(1); y++)
            {
                chunks[x, y].GetDistanceMap((int)(chunkSize/2)).Clamp(0.5f,1f).Normalise();
            }
        }

        var final = Maps.Map.CreateMapFromSubMapsWithoutResizing(chunks).Display();

        stack.CreateDebugStack(transform);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
