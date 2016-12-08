using UnityEngine;
using System.Collections;

public class PhysicalMeshOverlapTest : MonoBehaviour {


	public Material BaseMaterial;

	// Use this for initialization
	void Start () {

		RNG.DateTimeInit();


		var stack = new MeshDebugStack(BaseMaterial);
		
		var mapA = Map.BlankMap(100,100).PerlinFillMap(10,0,0,30).AddToStack(stack);
		var mapB = Map.BlankMap(100,100).PerlinFillMap(20,0,0,25).AddToStack(stack);

		var rectA = new Rect(Vector2.zero,new Vector2(10,10));
		var rectB = new Rect(new Vector2(0,7),new Vector2(10,10));

		mapA.ToPhysical(rectA)
			.PhysicalAverage(mapB.ToPhysical(rectB))
			.ToMap()
			.AddToStack(stack);

		stack.CreateDebugStack(0f);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
