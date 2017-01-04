using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Map;

public static class HeightmapPattern  {

    /*

    public static Layer SimpleIsland(int sizeX, int sizeY)
    {
        RNG.DateTimeInit();

        var map = Layer.BlankMap(sizeX, sizeY)
            .CreateCircularFalloff(sizeX * 0.3f)
            .SmoothMap(25)
            .Invert();

        map += Layer.BlankMap(sizeX, sizeY)
            .PerlinFillOctaves(50, 0, 0, RNG.NextFloat(0, 1000), 3, 0.5f, 2f)
            .Remap(-0.6f, 0.3f);

        map.SmoothMap(5).Remap(0,0.5f);

        return map;
    }

    public static Layer MajorMap(int size)
    {
        RNG.DateTimeInit();

        var map = new Layer(size, size);

        map.RandomFillMap(0.51f, 0, 0)
            //.AddToStack(stack)
            .ApplyMask(Layer.BlankMap(map)
                .ApplyMask(Layer.BlankMap(map)
                    .CreateCircularFalloff(size * 0.25f))
                .ApplyMask(Layer.BlankMap(map)
                    .CreateCircularFalloff(size * 0.23f)
                    .Invert())
                 .Invert())
            .Invert()
            .ApplyMask(Layer.BlankMap(map)
                    .CreateCircularFalloff(size * 0.45f))
            //.AddToStack(stack)
            .BoolSmoothOperation(4)
            //.AddToStack(stack)
            .RemoveSmallRegions(400)
            //.Invert()
            //.RemoveSmallRegions(300)
            //.Invert()
            //.AddToStack(stack)
            .AddRoomLogic();
        //.AddToStack(stack);

        var distanceMap = Layer.Clone(map)
            .GetDistanceMap(15)
            .Clamp(0.5f, 1f)
            .Normalise();
            //.AddToStack(stack);

        var blendMap = Layer.Clone(distanceMap)
            //.Clamp(0.5f, 1)
            .Normalise()
            .Invert();

        var perlinMap = Layer.BlankMap(size, size)
            .PerlinFillMap(10, new Domain(0f, 5f), new Coord(0, 0), new Vector2(0.5f, 0.5f), RNG.NextVector2(-1000, 1000), 7, 0.5f, 1.87f);
            //.AddToStack(stack);

        var finalMap = Layer.Blend(distanceMap.Normalise(), perlinMap.Normalise(), blendMap);
        //finalMap.AddToStack(stack);

        var heightMap = CreateHeightMap(map);
            //.AddToStack(stack);
        heightMap.Normalise()
            .LerpHeightMap(map, AnimationCurve.EaseInOut(0,0,1,1))
            .SmoothMap(10)
            .Normalise();
        //.AddToStack(stack);





        finalMap.Normalise().Multiply(0.5f);
            //.AddToStack(stack);

        //heightMap.AddToStack(stack);

        finalMap += heightMap;
        //finalMap.AddToStack(stack);
        finalMap.SmoothMap(1);
        return finalMap;
    }

    public static Layer IslandMap(int size)
    {
        RNG.DateTimeInit();

        var map = new Layer(size, size);

        map.RandomFillMap(0.51f, 0, 0)
            //.AddToStack(stack)
            .ApplyMask(Layer.BlankMap(map)
                    .CreateCircularFalloff(size * 0.45f))
            //.AddToStack(stack)
            .BoolSmoothOperation(4)
            //.AddToStack(stack)
            .RemoveSmallRegions(200)
            //.Invert()
            //.RemoveSmallRegions(300)
            //.Invert()
            //.AddToStack(stack)
            .AddRoomLogic();
            //.AddToStack(stack);

        var difference = map.GetFootprintOutline();

        var distanceMap = Layer.Clone(map)
            .GetDistanceMap(15)
            .Clamp(0.5f, 1f)
            .Normalise();

        var waterFalloff = Layer.Blend(distanceMap, new Layer(size, size, 0f), difference);
        var cliffsFalloff = Layer.Blend(distanceMap.Invert(), new Layer(size, size, 1f), difference.Invert());

        distanceMap.Invert();

        var finalFalloffMap = waterFalloff + cliffsFalloff;

        finalFalloffMap.Invert().Normalise();

        var blendMap = Layer.Clone(distanceMap)
            //.Clamp(0.5f, 1)
            .Normalise()
            .Invert();

        var heightMap = CreateHeightMap(map);
        heightMap.Normalise()
            .LerpHeightMap(map, AnimationCurve.EaseInOut(0, 0, 1, 1))
            .SmoothMap(10)
            .Normalise();

        var blenmer = Layer.Blend(heightMap, new Layer(size, size).FillWith(0), Layer.Clone(waterFalloff).Invert());

        var goof = blenmer + cliffsFalloff.Invert();
        //goof.SmoothMap(1);

        return goof;
    }

    */

    public static Stack CliffHillDiffMap(int size, Rect rect)
    {
        RNG.DateTimeInit();
        var seed = RNG.NextFloat(0, 1000);

        //CreateWalkableSpace

        var walkableAreaMap = new Layer(size, size);

        walkableAreaMap.RandomFillMap(0.5f, 0, 0)
            .ApplyMask(Layer.BlankMap(walkableAreaMap)
                    .CreateCircularFalloff(size * 0.45f))
            .BoolSmoothOperation(4)
            .RemoveSmallRegions(600)
            .Invert()
            .RemoveSmallRegions(300)
            .Invert()
            .AddRoomLogic();

        var oceanFalloffMap = walkableAreaMap.GetFootprintOutline();

        var walkableAreaFalloffMap = Layer.Clone(walkableAreaMap)
            .GetDistanceMap(15)
            .Clamp(0.5f, 1f)
            .Normalise();

        var waterFalloff = Layer.Blend(walkableAreaFalloffMap, new Layer(size, size, 0f), oceanFalloffMap);

        var deepWaterFalloff = waterFalloff
            .Clone()
            .Invert()
            .BooleanMapFromThreshold(0.35f)
            .GetDistanceMap(30)
            .Clamp(0.75f, 1f)
            .Normalise();

        var finalWaterFalloff = Layer.Blend(new Layer(size, size, 0).Remap(0, 0.5f), waterFalloff, deepWaterFalloff);

        var cliffsFalloff = Layer.Blend(new Layer(size, size, 0f), walkableAreaFalloffMap, finalWaterFalloff.Clone().Normalise().Clamp(0f, 0.1f).Normalise());

        // HERE TODAY: need to get identify inner cliffs.

        walkableAreaFalloffMap.Invert();

        var totalFalloffMap = finalWaterFalloff + cliffsFalloff.Invert();

        totalFalloffMap.Invert().Normalise();

        var heightMap = CreateHeightMap(walkableAreaMap);

        heightMap.Normalise()
            .LerpHeightMap(walkableAreaMap, AnimationCurve.EaseInOut(0, 0, 1, 1))
            .SmoothMap(10)
            .Normalise()
            .Remap(0.1f,1f);

        heightMap = Layer.Blend(new Layer(size, size, 0f), heightMap, finalWaterFalloff.Clone().Clamp(0, 0.5f).Normalise());

        var heightMapSlope = Layer.Clone(heightMap).GetAbsoluteBumpMap().Normalise().Clamp(0.12f, 0.2f).Normalise();

        totalFalloffMap.Normalise();

        var finalMap = cliffsFalloff
            .Clone()
            .Invert()
            .Normalise()
            .Multiply(0.5f)
            + heightMap;

        var realFinalMap = Layer.Blend(heightMap, finalMap, heightMapSlope);

        //var isWaterMap = realFinalMap.Clone().ShiftLowestValueToZero().Clamp(0, 0.1f).AddToStack(stack); 

        var maps = new Map.Stack(rect);

        maps.AddMap(MapType.WalkableMap, walkableAreaMap);
        //maps.AddMap(MapType.HeightMap, realFinalMap);
        maps.AddMap(MapType.HeightMap, realFinalMap);



        return maps;
    }

    static Layer CreateHeightMap(Layer unionMap)
    {
        var subMaps = unionMap.GenerateSubMaps(6, 12);
        var heightmap = Layer.CreateHeightMap(subMaps);

        var allRegions = new List<List<Coord>>();

        for (int i = 0; i < subMaps.Length; i++)
        {
            var subMap = subMaps[i];
            allRegions.AddRange(subMap.GetRegions(0));
        }

        var finalSubMaps = Layer.BlankMap(unionMap).CreateHeightSortedSubmapsFromDijkstrasAlgorithm(allRegions);
        heightmap = Layer.CreateHeightMap(finalSubMaps);

        return heightmap;
    }
}



