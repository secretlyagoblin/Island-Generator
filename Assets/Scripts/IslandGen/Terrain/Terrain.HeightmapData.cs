using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Map;

namespace Terrain {

    public class HeightmapData {

        public Rect Rect
        {
            get; private set;
        }

        Map.Stack _stack;

        private HeightmapData()
        {

        }

        public float[,] GetFloatArray(MapType type)
        {
            return _stack.GetMap(type).FloatArray;
        }

        public static HeightmapData BlankMap(int size, Rect rect, float value)
        {
            var data = new HeightmapData();
            data.Rect = rect;
            data._stack = new Map.Stack(rect);

            var parentHeightmap = new Layer(size, size, value);
            var parentWalkablemap = new Layer(size, size, value*2);

            data._stack.AddMap(MapType.HeightMap, parentHeightmap);
            data._stack.AddMap(MapType.WalkableMap, parentWalkablemap);

            return data;
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
            maps.AddMap(MapType.HeightMap, realFinalMap.Multiply(100f));
            maps.SetRect(rect);

            var mapData = new HeightmapData();
            mapData.Rect = rect;
            
            mapData._stack = maps;




            return mapData;
        }

        public static HeightmapData ChunkVoronoi(HeightmapData parentData, Coord coord, int size, Rect rect)
        {
            rect = GrowRectByOne(rect, size);
            size = size + 1;

            var data = new HeightmapData();
            data.Rect = rect;
            data._stack = new Map.Stack(rect);

            var parentHeightmap = new Layer(size, size, 0).ToPhysical(rect).Add(parentData._stack[MapType.HeightMap]).ToMap();
            var parentWalkablemap = new Layer(size, size, 0).ToPhysical(rect).Add(parentData._stack[MapType.WalkableMap]).ToMap();

            //var voronoi = new VoronoiGenerator(parentHeightmap, coord.TileX, coord.TileY, 0.05f, 23245.2344335454f);

            //var diffMap = voronoi.GetVoronoiBoolMap(parentWalkablemap);
            //var distanceMap = voronoi.GetDistanceMap().Invert().Remap(0f, 0.05f);

            //var heightMap = parentHeightmap + voronoi.GetHeightMap(parentHeightmap) + distanceMap;
            //heightMap.Multiply(0.5f);

            //data._stack.AddMap(MapType.HeightMap, Layer.Blend(heightMap, parentHeightmap, diffMap));
            //data._stack.AddMap(MapType.WalkableMap, diffMap);

            data._stack.AddMap(MapType.HeightMap, parentHeightmap);
            data._stack.AddMap(MapType.WalkableMap, parentWalkablemap);

            return data;

            /*            

            var _stack = new Map.Stack(rect);
            _stack.AddMap(MapType.HeightMap, new Layer(size, size, 0f).PerlinFill(7,coord.TileX,coord.TileY,456.1234f).Multiply(100f));
            _stack.AddMap(MapType.WalkableMap, new Layer(size, size, 0f));

            var data = new HeightmapData();
            data.Rect = rect;
            data._stack = _stack;

            */

            //return data;
        }

        public static HeightmapData CreateCollisionData(HeightmapData cellData, int descimationFactor, Rect rect)
        {

            var map = Layer.DecimateMap(cellData._stack.GetMap(Map.MapType.HeightMap),4);

            var data = new HeightmapData();
            data.Rect = rect;
            data._stack = new Map.Stack(rect);
            data._stack.AddMap(MapType.HeightMap, map);

            return data;
        }

        public static HeightmapData DummyMap(HeightmapData[,] dummyMaps, Rect rect)
        {
            rect = GrowRectByOne(rect, dummyMaps[0,0]._stack.GetMap(MapType.HeightMap).SizeX-1);

            var layers = new Layer[2,2];

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    layers[x, y] = dummyMaps[x, y]._stack.GetMap(MapType.HeightMap);
                }
            }

            var heightMap = Layer.CreateMapFromSubMapsAssumingOnePixelOverlap(layers);

            var _stack = new Map.Stack(rect);
            _stack.AddMap(MapType.HeightMap, heightMap);

            var data = new HeightmapData();
            data.Rect = rect;
            data._stack = _stack;

            return data;
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

        static Rect GrowRectByOne(Rect rect, int mapSize)
        {
            var positionSize = rect.height;
            var offsetSize = positionSize + (positionSize * (1f / mapSize));
            var offsetVector = new Vector2(offsetSize, offsetSize);

            return new Rect(rect.position, offsetVector);
        }

    }

}