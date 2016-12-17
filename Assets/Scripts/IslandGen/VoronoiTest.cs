using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiTest : MonoBehaviour {

    public Material DefaultMaterial;

	// Use this for initialization
	void Start () {

        RNG.DateTimeInit();

        var stack = new MeshDebugStack(DefaultMaterial);

        Map.SetGlobalStack(stack);

        var seed = RNG.NextFloat(0, 1000);

        var map = Map.BlankMap(100, 100);
        var voronoi = new VoronoiGenerator(map,0, 0, 0.05f, seed);
        map += voronoi.GetFalloffMap(4);
        map.AddToGlobalStack();

        map = Map.BlankMap(100, 100);
        voronoi = new VoronoiGenerator(map, 1, 0, 0.05f, seed);
        map += voronoi.GetFalloffMap(4);
        map.AddToGlobalStack();

        stack.CreateDebugStack(0);


		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
