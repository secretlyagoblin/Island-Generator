using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldGen;

public class WorldGenerator : MonoBehaviour {

    public RegionNetworkSettings RegionSettings;

    public float SimulationStep = 0.1f;
    public float SimulationLength = 10f;

    RegionNetwork _regionNetwork;
    WorldMesh _worldMesh;

	// Use this for initialization
	void Start () {

        RNG.DateTimeInit();

        _regionNetwork = new RegionNetwork(transform, RegionSettings);  
        _regionNetwork.Simulate(SimulationLength, SimulationStep);       

        _worldMesh = new WorldMesh(_regionNetwork.Finalise(), transform);
        _worldMesh.Generate();

        //_regionNetwork.DebugDraw(Color.white, 100f, true);

    }
}
