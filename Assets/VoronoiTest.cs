using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Maps;

public class VoronoiTest : MonoBehaviour {



    // Use this for initialization
    void Start()
    {

        RNG.DateTimeInit();

        var stack = new MeshDebugStack(new Material(Shader.Find("Standard")));
        Map.SetGlobalStack(stack);

        var mapSize = 256 ;

        var layer = new Map(mapSize, mapSize);
        layer.FillWithBoolNoise(0.48f)
            .AddToGlobalStack()
            .BoolSmoothOperation(12)
            .RemoveSmallRegions(400)
            .Invert()
            //.ThickenOutline()
                        .Invert()
            .AddToGlobalStack()
            .SmoothMap(4)
            .AddToGlobalStack()


            .AddToGlobalStack()
;

        var arg = Map.BlankMap(new Map(5, 5))
            .FillWithNoise()
            .AddToGlobalStack()
            .SmoothMap(1)
            .AddToGlobalStack()
            .Resize(512, 512)
            .AddToGlobalStack();

        var divisions = 8;
        var size = mapSize / divisions;

        for (int x = 0; x < divisions; x++)
        {
            for (int y = 0; y < divisions; y++)
            {
                var submap = layer.ExtractMap(x * size, y * size, size, size)
                    .AddToGlobalStack()
                    .Resize(256,256)
                                .BooleanMapFromThreshold(0.5f)
                    .AddToGlobalStack();
            }
        }






        stack.CreateDebugStack(transform);


    }
	
	// Update is called once per frame
	void Update () {
		
	}

    /*
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
*/
}
