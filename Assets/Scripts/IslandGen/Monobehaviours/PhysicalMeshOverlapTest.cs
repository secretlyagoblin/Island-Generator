using UnityEngine;
using System.Collections;

public class PhysicalMeshOverlapTest : MonoBehaviour {


	public Material BaseMaterial;

	// Use this for initialization
	void Start () {

		RNG.DateTimeInit();

        // Create debug stack to display maps

		var stack = new MeshDebugStack(BaseMaterial);
        Layer.SetGlobalStack(stack);

        //Create physical bounds for maps that overlap

		var rectA = new Rect(Vector2.zero+(Vector2.one*30),new Vector2(10,10));
		var rectB = new Rect((Vector2.one*3) + (Vector2.one * 30), new Vector2(5,5));

        //make maps physical, add a to b, convert back to abstract and 

        Layer.BlankMap(100, 100)
            .FillWith(0f)
            .AddToGlobalStack()
            .ToPhysical(rectA)
            .Add(Layer.BlankMap(7, 7)
                    .PerlinFill(3, 0, 0, RNG.NextFloat(0, 300))
                    .AddToGlobalStack()
                    .ToPhysical(rectB))
            .ToMap()
            .AddToGlobalStack()
            .SmoothMap(7)
            .AddToGlobalStack();


        stack.CreateDebugStack(0f);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
