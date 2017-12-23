using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldGen;

public class WorldGenerator : MonoBehaviour {

    public RegionNetworkSettings RegionSettings;
    public WorldMeshSettings WorldMeshSettings;
    public RegionMeshSettings RegionMeshSettings;

    RegionNetwork _regionNetwork;
    WorldMesh _worldMesh;
    RegionMeshNetwork _regionMeshNetwork;

    public int IterationCount = 1;
    int _iterationCounter = 0;
    int _failureCount = 0;

	// Use this for initialization

	void Start () {

        RNG.DateTimeInit();

        StartCoroutine(StepThrough(WorldMeshSettings.DebugDisplayTime));

        //_regionNetwork.DebugDraw(Color.white, 100f, true);

    }

    private IEnumerator StepThrough(float time)
    {
        var secs = new WaitForSeconds(time);

        for (int i = 0; i < IterationCount; i++)
        {
            Generate();
            yield return secs;
        }

        Debug.Log(_iterationCounter + " iterations run, " + _failureCount + " errors");
        Debug.Log(((float)_failureCount / (float)IterationCount * 100f) + "% fail rate");
    }

    private void Generate()
    {
        _iterationCounter++;

        _regionNetwork = new RegionNetwork(transform, RegionSettings);
        _regionNetwork.Simulate(RegionSettings.SimulationLength, RegionSettings.SimulationStep);

        _worldMesh = new WorldMesh(transform, _regionNetwork.Finalise(), WorldMeshSettings);

        if (!_worldMesh.Generate())
        {
            _failureCount++;
            Generate();
            return;
        }

        //nothing after here does anything, but it should!
        //for this to work, generate has to set up data for next stage
        // after that, debug display should have enough info it run over graph independintly of generate.

        _worldMesh.DisplayDebugGraph();
        _regionMeshNetwork = new RegionMeshNetwork(_worldMesh.Finalise());
    }
}
