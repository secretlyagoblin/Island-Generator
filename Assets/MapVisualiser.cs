using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Maps;

public class MapVisualiser : MonoBehaviour {

	// Use this for initialization
	void Start () {

        RNG.DateTimeInit();
        var seed = RNG.NextFloat(0, 1000);

        var stack = new MeshDebugStack(new Material(Shader.Find("Standard")));
        Map.SetGlobalStack(stack);

        //CreateWalkableSpace

        var size = 256;

        var walkableAreaMap = new Map(size, size);


        walkableAreaMap.FillWithBoolNoise(0.5f, 0, 0)
            .ApplyMask(Map.BlankMap(walkableAreaMap)
                    .CreateCircularFalloff(size * 0.45f))
            .BoolSmoothOperation(4)
            .RemoveSmallRegions(600)
            .Invert()
            .RemoveSmallRegions(300)
            .Invert()
            .AddRoomLogic()
            .AddToGlobalStack();

        var oceanFalloffMap = walkableAreaMap.GetFootprintOutline().AddToGlobalStack();

        var walkableAreaFalloffMap = Map.Clone(walkableAreaMap)
            .GetDistanceMap(15)
            .Clamp(0.5f, 1f)
            .Normalise()
            .AddToGlobalStack();

        var waterFalloff = Map.Blend(walkableAreaFalloffMap, new Map(size, size, 0f), oceanFalloffMap);

        var deepWaterFalloff = waterFalloff
            .Clone()
            .Invert()
            .BooleanMapFromThreshold(0.35f)
            .GetDistanceMap(30)
            .Clamp(0.75f, 1f)
            .Normalise()
            .AddToGlobalStack();

        var finalWaterFalloff = Map.Blend(new Map(size, size, 0).Remap(0, 0.5f), waterFalloff, deepWaterFalloff);

        var cliffsFalloff = Map.Blend(new Map(size, size, 0f), walkableAreaFalloffMap, finalWaterFalloff.Clone().Normalise().Clamp(0f, 0.1f).Normalise());

        // HERE TODAY: need to get identify inner cliffs.

        walkableAreaFalloffMap.Invert();

        var totalFalloffMap = finalWaterFalloff + cliffsFalloff.Invert();

        totalFalloffMap.Invert().AddToGlobalStack();

        var waterEdge = totalFalloffMap.Clone().Clamp(0.5f,1f).Normalise().BooleanMapFromThreshold(1f).Invert().ThickenOutline(1).Invert().AddToGlobalStack();
        var outline = (walkableAreaMap.Clone().ThickenOutline(1) - walkableAreaMap).AddToGlobalStack();
        var guf = Map.ApplyMask(outline, Map.BlankMap(outline).FillWith(0), waterEdge).AddToGlobalStack();
        var buf = Map.ApplyMask(outline, Map.BlankMap(outline).FillWith(0), waterEdge.Invert()).AddToGlobalStack();

        var heightMap = Terrain.TerrainData.CreateHeightMap(walkableAreaMap);

        heightMap.Normalise()
            .LerpHeightMap(walkableAreaMap, AnimationCurve.EaseInOut(0, 0, 1, 1))
            .SmoothMap(10)
            .Normalise()
            .Add(Map.BlankMap(heightMap).PerlinFill(5, 0, 0, 12003.123f).Remap(-0.02f, 0.02f))
            .Add(Map.BlankMap(heightMap).PerlinFill(15, 0, 0, 12003.123f).Remap(-0.04f, 0.04f))
            .Remap(0.1f, 1f);

        heightMap = Map.Blend(new Map(size, size, 0f), heightMap, finalWaterFalloff.Clone().Clamp(0, 0.5f).Normalise());

        var heightMapSlope = Map.Clone(heightMap).GetAbsoluteBumpMap().Normalise().Clamp(0.12f, 0.2f).Normalise();

        totalFalloffMap.Normalise();

        var finalMap = cliffsFalloff
            .Clone()
            .Invert()
            .Normalise()
            .Multiply(0.5f)
            + heightMap;

        var realFinalMap = Map.Blend(heightMap, finalMap, heightMapSlope);

        

        //var isWaterMap = realFinalMap.Clone().ShiftLowestValueToZero().Clamp(0, 0.1f).AddToStack(stack); 

        stack.CreateDebugStack(0);

    

}
	
	// Update is called once per frame
	void Update () {
		
	}
}
