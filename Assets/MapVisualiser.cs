using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Maps;

public class MapVisualiser : MonoBehaviour {

    // Use this for initialization
    void Start()
    {

        Map2();





    }

    void Map1()
    {
        RNG.DateTimeInit();
        var seed = RNG.NextFloat(0, 1000);

        var stack = new MeshDebugStack(new Material(Shader.Find("Standard")));
        Map.SetGlobalDisplayStack(stack);

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
            .Display();

        var oceanFalloffMap = walkableAreaMap.GetFootprintOutline().Display();

        var walkableAreaFalloffMap = Map.Clone(walkableAreaMap)
            .GetDistanceMap(15)
            .Clamp(0.5f, 1f)
            .Normalise()
            .Display();

        var waterFalloff = Map.Blend(walkableAreaFalloffMap, new Map(size, size, 0f), oceanFalloffMap);

        var deepWaterFalloff = waterFalloff
            .Clone()
            .Invert()
            .BooleanMapFromThreshold(0.35f)
            .GetDistanceMap(30)
            .Clamp(0.75f, 1f)
            .Normalise()
            .Display();

        var finalWaterFalloff = Map.Blend(new Map(size, size, 0).Remap(0, 0.5f), waterFalloff, deepWaterFalloff);

        var cliffsFalloff = Map.Blend(new Map(size, size, 0f), walkableAreaFalloffMap, finalWaterFalloff.Clone().Normalise().Clamp(0f, 0.1f).Normalise());

        // HERE TODAY: need to get identify inner cliffs.

        walkableAreaFalloffMap.Invert();

        var totalFalloffMap = finalWaterFalloff + cliffsFalloff.Invert();

        totalFalloffMap.Invert().Display();

        var waterEdge = totalFalloffMap.Clone().Clamp(0.5f, 1f).Normalise().BooleanMapFromThreshold(1f).Invert().ThickenOutline(1).Invert().Display();
        var outline = (walkableAreaMap.Clone().ThickenOutline(1) - walkableAreaMap).Display();
        var guf = Map.ApplyMask(outline, Map.BlankMap(outline).FillWith(0), waterEdge).Display();
        var buf = Map.ApplyMask(outline, Map.BlankMap(outline).FillWith(0), waterEdge.Invert()).Display();

        var heightMap = ProcTerrain.TerrainData.CreateHeightMap(walkableAreaMap);

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

    void Map2()
    {

        RNG.DateTimeInit();
        var seed = RNG.NextFloat(0, 1000);

        var stack = new MeshDebugStack(new Material(Shader.Find("Standard")));
        Map.SetGlobalDisplayStack(stack);

        //CreateWalkableSpace

        var size = 256;

        var walkableAreaMap = new Map(size, size);

        var cells = new List<VoronoiCell>();

        for (int i = 0; i < 100; i++)
        {
            cells.Add(new VoronoiCell(RNG.NextVector3(0f, 1f)));
        }

        var voronoiGenerator = new VoronoiGenerator(walkableAreaMap, cells);
        var fallOffMap = voronoiGenerator.GetVoronoiBoolMap(walkableAreaMap.Clone().CreateCircularFalloff(100f).Normalise()).Display();
        var fallOffFineAlright = Map.BooleanIntersection(
            walkableAreaMap
                .Clone()
                .CreateCircularFalloff(100f)
                .Invert(),
            walkableAreaMap
                .Clone()
                .CreateYGradient()
                .Display()
                .BooleanMapFromThreshold(0.7f))
            .Display()
            .Invert();
        var otherFallOffMap = voronoiGenerator.GetVoronoiBoolMap(fallOffFineAlright).Display();

        var heightMap = voronoiGenerator
            .GetHeightMap(walkableAreaMap);

        var smallSmooth = heightMap
            .Clone()
            .ApplyMask(fallOffMap)
            .Invert()
            .Remap(0, 0.5f)
            .SmoothMap(2)
            .Display();
        //var absHeight = smallSmooth.ApplyMask(otherFallOffMap).Display();

        var highSmooth = Map.DecimateMap(smallSmooth, 2).SmoothMap(10).Resize(size,size).SmoothMap().Display();
        var otherSteepness = highSmooth.GetAbsoluteBumpMap().Normalise().Display().BooleanMapFromThreshold(0.5f).Display();

        var exteriorMap = Map.DecimateMap(otherFallOffMap,4).SmoothMap(5).Resize(size,size);
        var exteriorBump = exteriorMap.Clone().GetAbsoluteBumpMap().Normalise().Display().BooleanMapFromThreshold(0.5f).Display();
        var finalBump = Map.BooleanUnion(exteriorBump.Invert(), otherSteepness.Invert()).Invert().Display();

        var finalHeightMap = Map.Blend(smallSmooth.Remap(0, 0.7f), heightMap.Clone().Remap(0.9f, 1f).SmoothMap(2), exteriorMap.Normalise()).Display();



        //walkableAreaMap.GDisplay();

        stack.CreateDebugStack(0);
    }
}


