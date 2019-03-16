using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using U3D.Threading.Tasks;

public class StructureV2 : MonoBehaviour {

    public int CellIndex;
    public LevelGenerator.LevelGeneratorSettings LevelGeneratorSettings;

    private LevelGenerator.LevelGenerator _levelGenerator;

    // Use this for initialization
    void Start()
    {
        _levelGenerator = new LevelGenerator.MainLevelGenerator(CellIndex, LevelGeneratorSettings);
        _levelGenerator.Generate();
    }

    private void Update()
    {
        var count = 0;

        while (count < 7)
        {
            count++;

            if (_levelGenerator.DequeueAsyncMesh() == false)
                break;  
        }
    }


}