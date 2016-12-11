using UnityEngine;
using System.Collections;

public class PhysicalMeshOverlapTest : MonoBehaviour {


	public Material BaseMaterial;

	// Use this for initialization
	void Start () {

		RNG.DateTimeInit();

        // Create debug stack to display maps

		var stack = new MeshDebugStack(BaseMaterial);

        //Create physical bounds for maps that overlap

		var rectA = new Rect(Vector2.zero+(Vector2.one*30),new Vector2(10,10));
		var rectB = new Rect((Vector2.one*3) + (Vector2.one * 30), new Vector2(5,5));

        //make maps physical, add a to b, convert back to abstract and 

		Map.BlankMap(100, 100)
            .FillWith(0f)
            .AddToStack(stack)
            .ToPhysical(rectA)
			.Add(Map.BlankMap(7, 7)
                    .PerlinFill(3, 0, 0, RNG.NextFloat(0, 300))
                    .AddToStack(stack)
                    .ToPhysical(rectB))
			.ToMap()
			.AddToStack(stack)
            .SmoothMap(7)
            .AddToStack(stack);

		stack.CreateDebugStack(0f);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
