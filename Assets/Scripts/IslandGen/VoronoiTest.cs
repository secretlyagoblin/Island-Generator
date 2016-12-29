using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiTest : MonoBehaviour {

    public Material DefaultMaterial;

    public AnimationCurve ugh;

    // Use this for initialization
    void Start() {

        RNG.DateTimeInit();
        var stack = new MeshDebugStack(DefaultMaterial);
        Map.SetGlobalStack(stack);
        var seed = RNG.NextFloat(0, 1000);

        //CreateWalkableSpace

        var size = 400;

        var walkableAreaMap = new Map(size, size);

        walkableAreaMap.RandomFillMap(0.5f, 0, 0)
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
            .AddToStack(stack);

       

        var waterFalloff = Map.Blend(walkableAreaFalloffMap, new Map(size, size, 0f), oceanFalloffMap).AddToStack(stack);

        var deepWaterFalloff = waterFalloff.Clone().Invert().BooleanMapFromThreshold(0.35f).AddToGlobalStack().GetDistanceMap(30).Clamp(0.75f, 1f).Normalise().AddToGlobalStack();

        var finalWaterFalloff = Map.Blend(new Map(size, size, 0).Remap(0,0.5f), waterFalloff, deepWaterFalloff).AddToGlobalStack();

        var cliffsFalloff = Map.Blend(new Map(size, size, 0f), walkableAreaFalloffMap, finalWaterFalloff.Clone().Normalise().Clamp(0f,0.1f).Normalise().AddToGlobalStack()).AddToStack(stack);

        // HERE TODAY: need to get identify inner cliffs.

        walkableAreaFalloffMap.Invert();

        var totalFalloffMap = finalWaterFalloff + cliffsFalloff.Invert();

        totalFalloffMap.Invert().Normalise().AddToStack(stack);

        //var blendMap = Map.Clone(walkableAreaFalloffMap)
        //    .Normalise()
        //    .Invert();
        //
        //var perlinMap = Map.BlankMap(size, size)
        //    .PerlinFillMap(10, new Domain(0f, 5f), new Coord(0, 0), new Vector2(0.5f, 0.5f), RNG.NextVector2(-1000, 1000), 7, 0.5f, 1.87f)
        //    .AddToStack(stack);
        //
        //var finalMap = Map.Blend(walkableAreaFalloffMap.Normalise(), perlinMap.Normalise(), blendMap);
        //finalMap.AddToStack(stack);

        var heightMap = CreateHeightMap(walkableAreaMap);

        heightMap.Normalise()
            .LerpHeightMap(walkableAreaMap, AnimationCurve.EaseInOut(0, 0, 1, 1))
            .SmoothMap(10)
            .Normalise();

        heightMap = Map.Blend(new Map(size, size, 0f),heightMap, finalWaterFalloff.Clone().Clamp(0,0.5f).Normalise().AddToGlobalStack()).AddToGlobalStack();

        var heightMapSlope = Map.Clone(heightMap).GetAbsoluteBumpMap().Normalise().Clamp(0.12f,0.2f).Normalise();

        totalFalloffMap.Normalise();

        var finalMap = cliffsFalloff.Clone().Invert().Normalise() + heightMap;

        var realFinalMap = Map.Blend(heightMap.AddToStack(stack), finalMap.AddToStack(stack), heightMapSlope.AddToStack(stack)).AddToStack(stack);

        //var isWaterMap = realFinalMap.Clone().ShiftLowestValueToZero().Clamp(0, 0.1f).AddToStack(stack); 

        stack.CreateDebugStack(0);


		
	}

    Map CreateHeightMap(Map unionMap)
    {
        var subMaps = unionMap.GenerateSubMaps(6, 12);
        var heightmap = Map.CreateHeightMap(subMaps);

        var allRegions = new List<List<Coord>>();

        for (int i = 0; i < subMaps.Length; i++)
        {
            var subMap = subMaps[i];
            allRegions.AddRange(subMap.GetRegions(0));
        }

        var finalSubMaps = Map.BlankMap(unionMap).CreateHeightSortedSubmapsFromDijkstrasAlgorithm(allRegions);
        heightmap = Map.CreateHeightMap(finalSubMaps);

        return heightmap;
    }

    void HellaDoIt()
    {
        RNG.DateTimeInit();

        var stack = new MeshDebugStack(DefaultMaterial);

        Map.SetGlobalStack(stack);

        var seed = RNG.NextFloat(0, 1000);


        //CreateWalkableSpace

        var size = 400;

        var map = new Map(size, size);

        map.RandomFillMap(0.49f, 0, 0)
            .AddToStack(stack)
            /*
            .ApplyMask(Map.BlankMap(map)
                .ApplyMask(Map.BlankMap(map)
                    .CreateCircularFalloff(size * 0.32f))
                .ApplyMask(Map.BlankMap(map)
                    .CreateCircularFalloff(size * 0.25f)
                    .Invert())
                 .Invert())
            .Invert()
            */
            .ApplyMask(Map.BlankMap(map)
                    .CreateCircularFalloff(size * 0.45f))
            .AddToStack(stack)
            .BoolSmoothOperation(4)
            .AddToStack(stack)
            .RemoveSmallRegions(600)
            .Invert()
            .RemoveSmallRegions(300)
            .Invert()
            .AddToStack(stack)
            .AddRoomLogic()
            .Invert()
            //.ThickenOutline(2)
            .Invert()
            .AddToStack(stack);

        var difference = map.GetFootprintOutline().AddToStack(stack);

        var distanceMap = Map.Clone(map)
            .GetDistanceMap(15)
            .Clamp(0.5f, 1f)
            .Normalise()
            .AddToStack(stack);

        var waterFalloff = Map.Blend(distanceMap, new Map(size, size, 0f), difference).AddToStack(stack);
        var cliffsFalloff = Map.Blend(distanceMap.Invert(), new Map(size, size, 1f), difference.Invert()).AddToStack(stack);

        distanceMap.Invert();

        var finalFalloffMap = waterFalloff + cliffsFalloff;

        finalFalloffMap.Invert().Normalise().AddToStack(stack);

        var blendMap = Map.Clone(distanceMap)
            //.Clamp(0.5f, 1)
            .Normalise()
            .Invert();

        var perlinMap = Map.BlankMap(size, size)
            .PerlinFillMap(10, new Domain(0f, 5f), new Coord(0, 0), new Vector2(0.5f, 0.5f), RNG.NextVector2(-1000, 1000), 7, 0.5f, 1.87f)
            .AddToStack(stack);

        var finalMap = Map.Blend(distanceMap.Normalise(), perlinMap.Normalise(), blendMap);
        finalMap.AddToStack(stack);

        var heightMap = CreateHeightMap(map).AddToStack(stack);
        heightMap.Normalise()
            .LerpHeightMap(map, AnimationCurve.EaseInOut(0, 0, 1, 1))
            .SmoothMap(10)
            .Normalise()
            //.Remap(0,0.07f)
            .AddToStack(stack);

        var heightMapSlope = Map.Clone(heightMap).GetAbsoluteBumpMap().AddToStack(stack).Normalise().Clamp(0.12f, 0.2f).AddToStack(stack);

        var blenmer = Map.Blend(heightMap, new Map(size, size).FillWith(0), Map.Clone(waterFalloff).Invert()).AddToStack(stack);

        var goof = blenmer + cliffsFalloff.Invert().Multiply(1.3f);
        goof.AddToStack(stack);

        finalMap.Normalise().AddToStack(stack);

        heightMap.AddToStack(stack);

        finalMap += heightMap;

        //var finalBumpMap = finalMap.Clone().AddToStack(stack).GetAbsoluteBumpMap().Normalise().AddToStack(stack);

        //var mapB = heightMapSlope.Clone().AddToStack(stack).Invert().Lighten(finalBumpMap).AddToStack(stack);

        var realFinalMap = Map.Blend(finalMap.AddToStack(stack), heightMap.AddToStack(stack), heightMapSlope.AddToStack(stack)).AddToStack(stack);

        var isWaterMap = realFinalMap.Clone().ShiftLowestValueToZero().Clamp(0, 0.3f).AddToStack(stack);




        //var roomMap = Map.Clone(map).AddRoomLogic();
        ////stack.RecordMapStateToStack(roomMap);
        //
        //var thickMap = Map.Clone(roomMap).Invert().ThickenOutline(1).Invert();
        ////stack.RecordMapStateToStack(thickMap);
        //
        //var differenceMap = Map.BooleanDifference(roomMap, thickMap);
        ////stack.RecordMapStateToStack(differenceMap);
        //
        //var staticMap = new Map(map);
        //staticMap.RandomFillMap(0.4f);
        //
        //differenceMap = Map.BooleanIntersection(differenceMap, staticMap);
        ////stack.RecordMapStateToStack(differenceMap);
        //
        //var unionMap = Map.BooleanUnion(roomMap, differenceMap);
        ////stack.RecordMapStateToStack(unionMap);
        //
        //unionMap.BoolSmoothOperation(4);
        //unionMap.RemoveSmallRegions(100);





        stack.CreateDebugStack(0);
    }



    // Update is called once per frame
    void Update () {
		
	}
}
