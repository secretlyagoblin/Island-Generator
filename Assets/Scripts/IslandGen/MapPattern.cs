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
            .PerlinFillMap(50, 0, 0, RNG.NextFloat(0, 1000), 3, 0.5f, 2f)
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
