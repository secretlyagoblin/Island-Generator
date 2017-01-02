using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassInstantiate : MonoBehaviour {

    public Rect MainRect;
    public int Divisions;

    DataBlock[,] _dataBlock;

    DataBlock[,] _lastBlock = new DataBlock[3, 3];

    public int MassiveMapResolution;
    public int MapResolution;
    float _offsetSize;

    public Material Material;

    public GameObject tesPos;
    Vector3 _lastPos = Vector3.zero;

    bool _firstUpdate = false;


	// Use this for initialization
	void Start () {

        _dataBlock = new DataBlock[Divisions,Divisions];

        var positionSize = MainRect.height / Divisions;
        _offsetSize = positionSize + (positionSize * (1f / MapResolution));
        var offsetVector = new Vector2(_offsetSize, _offsetSize);

        var moddedResolution = MapResolution + 1;

        var totalMap = MapPattern.CliffHillDiffMap(MassiveMapResolution);
        totalMap.SetRect(MainRect);

        var heightmeshGenerator = new HeightmeshGenerator();
        var lens = new MeshLens(new Vector3(_offsetSize, _offsetSize, _offsetSize));
        

        for (int x = 0; x < Divisions; x++)
        {
            for (int y = 0; y < Divisions; y++)
            {
                var dataBlock = new DataBlock();
                var pos = new Vector2(x * positionSize, y * positionSize);
                dataBlock.baseRect = new Rect(pos, offsetVector);

                

                dataBlock.itsTheMap = new Map(moddedResolution, moddedResolution).ToPhysical(dataBlock.baseRect).Add(totalMap[MapType.HeightMap]).ToMap();
                var patch = heightmeshGenerator.GenerateHeightmeshPatch(dataBlock.itsTheMap, lens, dataBlock.baseRect);

                var physBlock = new Map(moddedResolution/8, moddedResolution/8).ToPhysical(dataBlock.baseRect).Add(totalMap[MapType.HeightMap]).ToMap();
                var physPatch = heightmeshGenerator.GenerateHeightmeshPatch(physBlock, lens, dataBlock.baseRect);
                var physMesh = physPatch.CreateMesh();


                var finalMesh = patch.CreateMesh();

                var gobject = new GameObject();

                gobject.SetActive(false);
                
                gobject.transform.position = new Vector3(x * positionSize, 0, y * positionSize);
                gobject.AddComponent<MeshRenderer>().sharedMaterial = Material;
                gobject.AddComponent<MeshFilter>().sharedMesh = finalMesh;
                var col = gobject.AddComponent<MeshCollider>();
                col.sharedMesh = physMesh;
                dataBlock.goblet = gobject;

                _dataBlock[x, y] = dataBlock;
            }
        }

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                _lastBlock[x, y] = _dataBlock[x, y];
            }
        }
		
	}
	
	// Update is called once per frame
	void Update () {

        if (_firstUpdate)
        {
            for (int x = 0; x < Divisions; x++)
            {
                for (int y = 0; y < Divisions; y++)
                {
                    _dataBlock[x, y].goblet.SetActive(false);
                }
            }

            _firstUpdate = false;
        }



        var pos = tesPos.transform.position;

        if(pos.x == _lastPos.x && pos.z == _lastPos.z)
        {
            return;
        }

        var currentX = Mathf.RoundToInt(pos.x / _offsetSize);
        var currentY = Mathf.RoundToInt(pos.z / _offsetSize);

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                _lastBlock[x, y].goblet.SetActive(false);
            }
        }

        var xCount = -1;
        var yCount = -1;

        for (int x = currentX-1; x <= currentX+1; x++)
        {
            xCount++;
            for (int y = currentY - 1; y <= currentY + 1; y++)
            {
                yCount++;
                _dataBlock[x, y].goblet.SetActive(true);

                _lastBlock[xCount, yCount] = _dataBlock[x, y];
            }
            yCount = -1;
        }

        _lastPos = pos;




    }

    public class DataBlock {

        public Rect baseRect;
        public Map itsTheMap;
        public GameObject goblet;
    }
}
