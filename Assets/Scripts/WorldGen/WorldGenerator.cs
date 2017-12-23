using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldGen;

public class WorldGenerator : MonoBehaviour {

    public RegionNetworkSettings RegionSettings;
    public WorldMeshSettings WorldMeshSettings;

    public float SimulationStep = 0.1f;
    public float SimulationLength = 10f;

    RegionNetwork _regionNetwork;
    WorldMesh _worldMesh;

    public int IterationCount = 1;
    int _iterationCounter = 0;
    int _failureCount = 0;

	// Use this for initialization

	void Start () {

        RNG.DateTimeInit();

        StartCoroutine(StepThrough(IterationCount, WorldMeshSettings.DebugDisplayTime));

        //_regionNetwork.DebugDraw(Color.white, 100f, true);

    }

    private IEnumerator StepThrough(int iterations, float time)
    {
        var secs = new WaitForSeconds(time);

        for (int i = 0; i < iterations; i++)
        {
            Generate();
            yield return secs;
        }

        Debug.Log(IterationCount + " iterations run, " + _failureCount + " errors");
        Debug.Log(((float)_failureCount / (float)IterationCount * 100f) + "% fail rate");
    }

    private void Generate()
    {

        //Debug.Log("...running iteration " + (_iterationCounter) + "...");

        _regionNetwork = new RegionNetwork(transform, RegionSettings);
        _regionNetwork.Simulate(SimulationLength, SimulationStep);

        _worldMesh = new WorldMesh(transform, _regionNetwork.Finalise(), WorldMeshSettings);


        if (!_worldMesh.Generate())
        {
            _failureCount++;
            Generate();
        }
    }
}
