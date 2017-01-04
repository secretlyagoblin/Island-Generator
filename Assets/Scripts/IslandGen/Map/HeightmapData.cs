using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Map;

public class HeightmapData {

    public Rect Rect
    {
        get; private set;
    }

    Map.Stack _stack;

    private HeightmapData()
    {

    }

    public float[,] GetFloatArray()
    {
        return _stack.GetMap(MapType.HeightMap).FloatArray;
    }

    public static HeightmapData RegionIsland(int size, Rect rect)
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
            .Remap(0.1f, 1f);

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

        var mapData = new HeightmapData();
        mapData._stack = maps;




        return mapData;
    }

    public static HeightmapData ChunkVoronoi(HeightmapData parentData, Coord coord, int size, Rect rect)
    {
        var data = new HeightmapData();
        data.Rect = rect;
        data._stack = new Map.Stack(rect);

        var parentHeightmap = parentData._stack[MapType.HeightMap].Add(new Layer(size, size, 0).ToPhysical(rect)).ToMap();
        var parentWalkablemap = parentData._stack[MapType.WalkableMap].Add(new Layer(size, size, 0).ToPhysical(rect)).ToMap();

        var voronoi = new VoronoiGenerator(parentHeightmap, coord.TileX, coord.TileY, 0.05f, 23245.2344335454f);

        var diffMap = voronoi.GetVoronoiBoolMap(parentWalkablemap);
        var distanceMap = voronoi.GetDistanceMap().Invert().Remap(0f, 0.05f);

        var heightMap = parentHeightmap + voronoi.GetHeightMap(parentHeightmap) + distanceMap;
        heightMap.Multiply(0.5f);

        data._stack.AddMap(MapType.HeightMap,Layer.Blend(heightMap, parentHeightmap, diffMap));
        data._stack.AddMap(MapType.WalkableMap, diffMap);

        return data;
    }

    public static HeightmapData DummyMap(HeightmapData[,] dummyMaps)
    {
        return new HeightmapData();
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
