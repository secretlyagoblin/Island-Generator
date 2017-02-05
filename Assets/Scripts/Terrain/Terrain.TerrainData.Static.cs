using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Map;

namespace Terrain {

    public partial class TerrainData {

        public static TerrainData BlankMap(int size, Rect rect, float value)
        {
            var parentHeightmap = new Layer(size, size, value);
            var parentWalkablemap = new Layer(size, size, value*2);

            return new TerrainData(rect, parentWalkablemap,parentHeightmap);
        }

        public static TerrainData RegionIsland(int size, Rect rect)
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

            return new TerrainData(rect,walkableAreaMap, realFinalMap.Multiply(200f));
        }

        public static TerrainData ChunkVoronoi(TerrainData parentData, Coord coord, int size, Rect rect)
        {
            rect = GrowRectByOne(rect, size);
            size = size + 1;     

            var parentHeightmap = new Layer(size, size, 0).ToPhysical(rect).Add(parentData.HeightMap.ToPhysical(parentData.Rect)).ToMap();
            var parentWalkablemap = new Layer(size, size, 0).ToPhysical(rect).Add(parentData.WalkableMap.ToPhysical(parentData.Rect)).ToMap();

            var noiseMap = new Layer(size, size).FillWithNoise().Multiply(10f);
            

            if (VoronoiGenerator.Generator == null)
            {
                VoronoiGenerator.Generator = new VoronoiGenerator(parentHeightmap, coord.TileX, coord.TileY, 0.075f, 23245.2344335454f);
            }

            var voronoi = VoronoiGenerator.Generator;

            var diffMap = voronoi.GetVoronoiBoolMap(parentWalkablemap);
            var distanceMap = voronoi.GetDistanceMap().Invert().Remap(0f, 0.2f);

            var heightMap = parentHeightmap + voronoi.GetHeightMap(parentHeightmap+noiseMap) + distanceMap;
            heightMap.Multiply(0.5f);
            //heightMap.SmoothMap(3);

            //data._stack.AddMap(MapType.HeightMap, parentHeightmap);
            //data._stack.AddMap(MapType.WalkableMap, parentWalkablemap);

            //return new TerrainData(rect, Layer.Blend(heightMap, parentHeightmap, diffMap), diffMap);
            return new TerrainData(rect, diffMap, heightMap);

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

        public static TerrainData PassThroughUntouched(TerrainData parentData, Coord coord, int size, Rect rect)
        {
            rect = GrowRectByOne(rect, size);
            size = size + 1;

            var parentHeightmap = new Layer(size, size, 0).ToPhysical(rect).Add(parentData.HeightMap.ToPhysical(parentData.Rect)).ToMap();
            var parentWalkablemap = new Layer(size, size, 0).ToPhysical(rect).Add(parentData.WalkableMap.ToPhysical(parentData.Rect)).ToMap();

            var colorLayer = new ColorLayer(parentWalkablemap.Clone().Invert().Remap(0.1f,1f));

            return new TerrainData(rect, parentWalkablemap, parentHeightmap, colorLayer);
        }

        public static TerrainData CreateCollisionData(TerrainData cellData, int decimationFactor, Rect rect)
        {

            //Debug.DrawLine(new Vector3(rect.min.x, 0, rect.min.y), new Vector3(rect.min.x, 0, rect.max.y), Color.white,100f);
            //Debug.DrawLine(new Vector3(rect.min.x, 0, rect.max.y), new Vector3(rect.max.x, 0, rect.max.y), Color.white, 100f);
            //Debug.DrawLine(new Vector3(rect.max.x, 0, rect.max.y), new Vector3(rect.max.x, 0, rect.min.y), Color.white, 100f);
            //Debug.DrawLine(new Vector3(rect.max.x, 0, rect.min.y), new Vector3(rect.min.x, 0, rect.min.y), Color.white, 100f);

            var map = Layer.DecimateMap(cellData.HeightMap, decimationFactor);
            rect = GrowRectByOne(rect, map.SizeX-1);
            //rect.position

            //Debug.DrawLine(new Vector3(rect.min.x, 0, rect.min.y), new Vector3(rect.min.x, 0, rect.max.y), Color.white, 100f);
            //Debug.DrawLine(new Vector3(rect.min.x, 0, rect.max.y), new Vector3(rect.max.x, 0, rect.max.y), Color.white, 100f);
            //Debug.DrawLine(new Vector3(rect.max.x, 0, rect.max.y), new Vector3(rect.max.x, 0, rect.min.y), Color.white, 100f);
            //Debug.DrawLine(new Vector3(rect.max.x, 0, rect.min.y), new Vector3(rect.min.x, 0, rect.min.y), Color.white, 100f);


            return new TerrainData(rect,Layer.BlankMap(map), map);
        }

        public static TerrainData DummyMap(TerrainData[,] dummyMaps, Rect rect)
        {
            rect = GrowRectByOne(rect, dummyMaps[0,0].HeightMap.SizeX-1);

            var layers = new Layer[2,2];

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    layers[x, y] = dummyMaps[x, y].HeightMap;
                }
            }

            var heightMap = Layer.CreateMapFromSubMapsAssumingOnePixelOverlap(layers);

            return new TerrainData(rect,Layer.BlankMap(heightMap), heightMap);
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