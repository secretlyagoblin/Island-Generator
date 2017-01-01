using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassInstantiate : MonoBehaviour {

    public Rect MainRect;
    public int Divisions;

    DataBlock[,] _dataBlock;

    public int MapResolution;


	// Use this for initialization
	void Start () {

        _dataBlock = new DataBlock[Divisions,Divisions];

        var positionSize = MainRect.height / Divisions;
        var OffsetSize = positionSize + (positionSize * (1 / MapResolution));
        var offsetVector = new Vector2(OffsetSize, OffsetSize);

        var moddedResolution = MapResolution + 1;

        var totalMap = MapPattern.CliffHillDiffMap(moddedResolution);
        totalMap.SetRect(MainRect);
        

        for (int x = 0; x < Divisions; x++)
        {
            for (int y = 0; y < Divisions; y++)
            {
                var dataBlock = new DataBlock();
                var pos = new Vector2(x * positionSize, y * positionSize);
                dataBlock.baseRect = new Rect(pos, offsetVector);
                dataBlock.itsTheMap = new Map(moddedResolution, moddedResolution).ToPhysical(dataBlock.baseRect).Add(totalMap[MapType.HeightMap]).ToMap();
            }
        }
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public class DataBlock {

        public Rect baseRect;
        public Map itsTheMap;
    }
}
