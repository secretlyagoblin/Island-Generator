using UnityEngine;
using System.Collections;

public class PhysicalMeshOverlapTest : MonoBehaviour {


	public Material BaseMaterial;

	// Use this for initialization
	void Start () {

		RNG.DateTimeInit();


		var stack = new MeshDebugStack(BaseMaterial);
		
		var mapA = Map.BlankMap(100,100).FillWilth(0f).AddToStack(stack);
		var mapB = Map.BlankMap(7,7).PerlinFillMap(3,0,0, RNG.NextFloat(0, 300)).AddToStack(stack);

		var rectA = new Rect(Vector2.zero+(Vector2.one*30),new Vector2(10,10));
		var rectB = new Rect((Vector2.one*3) + (Vector2.one * 30), new Vector2(5,5));

		mapA.ToPhysical(rectA)
			.Add(mapB.ToPhysical(rectB))
			.ToMap()
			.AddToStack(stack);

		stack.CreateDebugStack(0f);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
