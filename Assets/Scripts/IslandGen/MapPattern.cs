using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapPattern  {

    public static Map SimpleIsland(int sizeX, int sizeY)
    {
        RNG.DateTimeInit();

        var map = Map.BlankMap(sizeX, sizeY)
            .CreateCircularFalloff(sizeX * 0.3f)
            .SmoothMap(25)
            .Invert();

        map += Map.BlankMap(sizeX, sizeY)
            .PerlinFillOctaves(50, 0, 0, RNG.NextFloat(0, 1000), 3, 0.5f, 2f)
            .Remap(-0.6f, 0.3f);

        map.SmoothMap(5).Remap(0,0.5f);

        return map;
    }

    public static Map MajorMap(int size)
    {
        RNG.DateTimeInit();

        var map = new Map(size, size);

        map.RandomFillMap(0.51f, 0, 0)
            //.AddToStack(stack)
            .ApplyMask(Map.BlankMap(map)
                .ApplyMask(Map.BlankMap(map)
                    .CreateCircularFalloff(size * 0.25f))
                .ApplyMask(Map.BlankMap(map)
                    .CreateCircularFalloff(size * 0.23f)
                    .Invert())
                 .Invert())
            .Invert()
            .ApplyMask(Map.BlankMap(map)
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

        var distanceMap = Map.Clone(map)
            .GetDistanceMap(15)
            .Clamp(0.5f, 1f)
            .Normalise();
            //.AddToStack(stack);

        var blendMap = Map.Clone(distanceMap)
            //.Clamp(0.5f, 1)
            .Normalise()
            .Invert();

        var perlinMap = Map.BlankMap(size, size)
            .PerlinFillMap(10, new Domain(0f, 5f), new Coord(0, 0), new Vector2(0.5f, 0.5f), RNG.NextVector2(-1000, 1000), 7, 0.5f, 1.87f);
            //.AddToStack(stack);

        var finalMap = Map.Blend(distanceMap.Normalise(), perlinMap.Normalise(), blendMap);
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

    public static Map IslandMap(int size)
    {
        RNG.DateTimeInit();

        var map = new Map(size, size);

        map.RandomFillMap(0.51f, 0, 0)
            //.AddToStack(stack)
            /*
            .ApplyMask(Map.BlankMap(map)
                .ApplyMask(Map.BlankMap(map)
                    .CreateCircularFalloff(size * 0.25f))
                .ApplyMask(Map.BlankMap(map)
                    .CreateCircularFalloff(size * 0.23f)
                    .Invert())
                 .Invert())
            .Invert()
            */
            .ApplyMask(Map.BlankMap(map)
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

        var distanceMap = Map.Clone(map)
            .GetDistanceMap(15)
            .Clamp(0.5f, 1f)
            .Normalise();

        var waterFalloff = Map.Blend(distanceMap, new Map(size, size, 0f), difference);
        var cliffsFalloff = Map.Blend(distanceMap.Invert(), new Map(size, size, 1f), difference.Invert());

        distanceMap.Invert();

        var finalFalloffMap = waterFalloff + cliffsFalloff;

        finalFalloffMap.Invert().Normalise();

        var blendMap = Map.Clone(distanceMap)
            //.Clamp(0.5f, 1)
            .Normalise()
            .Invert();

        var heightMap = CreateHeightMap(map);
        heightMap.Normalise()
            .LerpHeightMap(map, AnimationCurve.EaseInOut(0, 0, 1, 1))
            .SmoothMap(10)
            .Normalise();

        var blenmer = Map.Blend(heightMap, new Map(size, size).FillWith(0), Map.Clone(waterFalloff).Invert());

        var goof = blenmer + cliffsFalloff.Invert();
        //goof.SmoothMap(1);

        return goof;
    }

    public static MapCollection CliffHillDiffMap(int size)
    {
        RNG.DateTimeInit();
        var seed = RNG.NextFloat(0, 1000);

        //CreateWalkableSpace

        var walkableAreaMap = new Map(size, size);

        walkableAreaMap.RandomFillMap(0.5f, 0, 0)
            .ApplyMask(Map.BlankMap(walkableAreaMap)
                    .CreateCircularFalloff(size * 0.45f))
            .BoolSmoothOperation(4)
            .RemoveSmallRegions(600)
            .Invert()
            .RemoveSmallRegions(300)
            .Invert()
            .AddRoomLogic();

        var oceanFalloffMap = walkableAreaMap.GetFootprintOutline();

        var walkableAreaFalloffMap = Map.Clone(walkableAreaMap)
            .GetDistanceMap(15)
            .Clamp(0.5f, 1f)
            .Normalise();



        var waterFalloff = Map.Blend(walkableAreaFalloffMap, new Map(size, size, 0f), oceanFalloffMap);

        var deepWaterFalloff = waterFalloff
            .Clone()
            .Invert()
            .BooleanMapFromThreshold(0.35f)
            .GetDistanceMap(30)
            .Clamp(0.75f, 1f)
            .Normalise();

        var finalWaterFalloff = Map.Blend(new Map(size, size, 0).Remap(0, 0.5f), waterFalloff, deepWaterFalloff);

        var cliffsFalloff = Map.Blend(new Map(size, size, 0f), walkableAreaFalloffMap, finalWaterFalloff.Clone().Normalise().Clamp(0f, 0.1f).Normalise());

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

        var maps = new MapCollection();

        maps.AddMap(MapType.WalkableMap, walkableAreaMap);
        //maps.AddMap(MapType.HeightMap, realFinalMap);
        maps.AddMap(MapType.HeightMap, realFinalMap);

        return maps;
    }

    static Map CreateHeightMap(Map unionMap)
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


}

