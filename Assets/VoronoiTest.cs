using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Map;

public class VoronoiTest : MonoBehaviour {

    

	// Use this for initialization
	void Start () {

        RNG.DateTimeInit();

        var stack = new MeshDebugStack(new Material(Shader.Find("Standard")));
        Layer.SetGlobalStack(stack);

        var layer = new Layer(200, 200);
        layer.FillWithNoise()
            .AddToGlobalStack()
            .BoolSmoothOperation(7)
            .AddToGlobalStack();

        var heightMap = new Layer(200, 200);
        heightMap.PerlinFill(41.242323f, 0, 0, 1224321.343442f)
            .AddToGlobalStack();

        Rect rect = new Rect(Vector2.zero, Vector2.one);

        var cells = new List<VoronoiCell>();

        for (int i = 0; i < 1000; i++)
        {
            var x = RNG.NextFloat();
            var y = RNG.NextFloat();

            var inside = layer.BilinearSampleFromNormalisedVector2(new Vector2(x, y));
            var height = heightMap.BilinearSampleFromNormalisedVector2(new Vector2(x, y));


            var cell = new VoronoiCell(new Vector3(x,height, y));
            cell.Inside = inside > 0.5f ? true : false;
            cells.Add(cell);
        }

        var voronoiGenerator = new VoronoiGenerator(layer, cells);

       var be = voronoiGenerator.GetHeightMap(layer).AddToGlobalStack();
       voronoiGenerator.GetVoronoiBoolMap(layer).AddToGlobalStack();
       voronoiGenerator.GetDistanceMap().AddToGlobalStack();
       var ae = voronoiGenerator.GetFalloffMap(5).AddToGlobalStack().Multiply(0.2f);

        (be + ae).AddToGlobalStack();



        stack.CreateDebugStack(transform);


    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
