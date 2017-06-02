using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Maps;

namespace Terrain {

    public partial class TerrainData {

        public static TerrainData BlankMap(int size, Rect rect, float value)
        {
            var parentHeightmap = new Map(size, size, value);
            var parentWalkablemap = new Map(size, size, value*2);

            return new TerrainData(rect, parentWalkablemap,parentHeightmap);
        }



        public static TerrainData RegionIsland(int size, Rect rect)
        {
            RNG.DateTimeInit();
            var seed = RNG.NextFloat(0, 1000);

            //CreateWalkableSpace

            var walkableAreaMap = new Map(size, size);

            walkableAreaMap.FillWithBoolNoise(0.5f, 0, 0)
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

            var waterEdge = totalFalloffMap.Clone().Invert().Clamp(0.5f, 1f).Normalise().BooleanMapFromThreshold(1f).Invert().ThickenOutline(1).Invert();
            var outline = (walkableAreaMap.Clone().ThickenOutline(1) - walkableAreaMap);
            var waterOutline = Map.ApplyMask(outline, Map.BlankMap(outline).FillWith(0), waterEdge);
            var earthOutline = Map.ApplyMask(outline, Map.BlankMap(outline).FillWith(0), waterEdge.Invert());

            totalFalloffMap.Invert().Normalise();

            var heightMap = CreateHeightMap(walkableAreaMap);

            heightMap.Normalise()
                .LerpHeightMap(walkableAreaMap, AnimationCurve.EaseInOut(0, 0, 1, 1))
                .SmoothMap(10)
                .Normalise()
                .Add(Map.BlankMap(heightMap).PerlinFill(2.2f,0,0,12003.123f).Remap(-0.02f,0.02f))
                .Add(Map.BlankMap(heightMap).PerlinFill(7, 0, 0, 12003.123f).Remap(-0.04f, 0.04f))
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

            var terrainData = new TerrainData(rect, walkableAreaMap, realFinalMap.Clone().Multiply(200f), new ColorLayer(realFinalMap));
            terrainData.WaterOutline = waterOutline;
            terrainData.LandOutline = earthOutline;

            return terrainData;
        }

        public static Map[] VoronoiPreBake(TerrainData parentData, int size, Rect rect)
        {
            var grownRect = GrowRectByOne(rect, size);

            var parentHeightmap = new Map(size, size, 0).ToPhysical(grownRect).Add(parentData.HeightMap.ToPhysical(parentData.Rect)).ToMap();
            var parentWalkablemap = new Map(size, size, 0).ToPhysical(grownRect).Add(parentData.WalkableMap.ToPhysical(parentData.Rect)).ToMap();

            return new Map[] { parentHeightmap, parentWalkablemap };


        }

        public static TerrainData ChunkVoronoi(Map[] prebakeData, List<VoronoiCell> voronoiCells, int size, Rect rect)
        {
            var originalRect = rect;
            var grownRect = GrowRectByOne(rect, size);
            size = size + 1;     

            var parentHeightmap = prebakeData[0];
            var parentWalkablemap = prebakeData[1];

            var noiseMap = new Map(size, size).FillWithNoise().Multiply(10f);
            

            var voronoi = new VoronoiGenerator(parentHeightmap, voronoiCells);

            var diffMap = voronoi.GetVoronoiBoolMap(parentWalkablemap);
            var distanceMap = voronoi.GetDistanceMap().Invert().Remap(0f, 0.2f);

            var heightMap = parentHeightmap + voronoi.GetHeightMap(parentHeightmap+noiseMap) + distanceMap;
            heightMap.Multiply(0.5f);
            //heightMap.SmoothMap(3);

            //data._stack.AddMap(MapType.HeightMap, parentHeightmap);
            //data._stack.AddMap(MapType.WalkableMap, parentWalkablemap);

            //return new TerrainData(rect, Layer.Blend(heightMap, parentHeightmap, diffMap), diffMap);

            var colorMap = new ColorLayer(heightMap);

            var manager = PaletteManager.GetPalette();

            if(manager == null)
            {
                Debug.Log("No Colour Palette!");
            }

            colorMap.SetGradient(manager.GroundColor);
            colorMap.ApplyMap(parentHeightmap.Clone().Multiply(1f/200f));
            colorMap.SetGradient(PaletteManager.GetPalette().CliffColor);
            colorMap.ApplyMapWithMask(heightMap.Clone().Multiply(1f / 200f),diffMap);

            return new TerrainData(grownRect, diffMap, Map.Blend(heightMap, parentHeightmap, diffMap), colorMap);

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

            var parentHeightmap = new Map(size, size, 0).ToPhysical(rect).Add(parentData.HeightMap.ToPhysical(parentData.Rect)).ToMap();
            var parentWalkablemap = new Map(size, size, 0).ToPhysical(rect).Add(parentData.WalkableMap.ToPhysical(parentData.Rect)).ToMap();

            var colorLayer = new ColorLayer(parentWalkablemap.Clone().Invert().Remap(0.1f,1f));

            return new TerrainData(rect, parentWalkablemap, parentHeightmap, colorLayer);
        }

        public static TerrainData CreateCollisionData(TerrainData cellData, int decimationFactor, Rect rect)
        {

            //Debug.DrawLine(new Vector3(rect.min.x, 0, rect.min.y), new Vector3(rect.min.x, 0, rect.max.y), Color.white,100f);
            //Debug.DrawLine(new Vector3(rect.min.x, 0, rect.max.y), new Vector3(rect.max.x, 0, rect.max.y), Color.white, 100f);
            //Debug.DrawLine(new Vector3(rect.max.x, 0, rect.max.y), new Vector3(rect.max.x, 0, rect.min.y), Color.white, 100f);
            //Debug.DrawLine(new Vector3(rect.max.x, 0, rect.min.y), new Vector3(rect.min.x, 0, rect.min.y), Color.white, 100f);            

            var data = Decimate(rect, cellData, decimationFactor);
            rect = GrowRectByOne(rect, data.HeightMap.SizeX - 1);
            data.Rect = rect;

            //rect.position

            //Debug.DrawLine(new Vector3(rect.min.x, 0, rect.min.y), new Vector3(rect.min.x, 0, rect.max.y), Color.white, 100f);
            //Debug.DrawLine(new Vector3(rect.min.x, 0, rect.max.y), new Vector3(rect.max.x, 0, rect.max.y), Color.white, 100f);
            //Debug.DrawLine(new Vector3(rect.max.x, 0, rect.max.y), new Vector3(rect.max.x, 0, rect.min.y), Color.white, 100f);
            //Debug.DrawLine(new Vector3(rect.max.x, 0, rect.min.y), new Vector3(rect.min.x, 0, rect.min.y), Color.white, 100f);


            return data;
        }

        public static TerrainData DummyMap(TerrainData[,] dummyMaps, Rect rect)
        {
            rect = GrowRectByOne(rect, dummyMaps[0,0].HeightMap.SizeX-1);

            var layers = new Map[2,2];

            for (int x = 0; x < 2; x++)
                for (int y = 0; y < 2; y++)
                    layers[x, y] = dummyMaps[x, y].HeightMap;

            var newMap = CreateSubMap(rect, dummyMaps);

            return newMap;
        }

        public static Map CreateHeightMap(Map unionMap)
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

        public static Rect GrowRectByOne(Rect rect, int mapSize)
        {
            var positionSize = rect.height;
            var offsetSize = positionSize + (positionSize * (1f / mapSize));
            var offsetVector = new Vector2(offsetSize, offsetSize);

            return new Rect(rect.position, offsetVector);
        }
    }

}