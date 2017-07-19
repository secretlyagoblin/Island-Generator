using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Maps;

namespace ProcTerrain {

    public partial class TerrainData {

        public static TerrainData BlankMap(int size, Rect rect, float value)
        {
            var parentHeightmap = new Map(size, size, value);
            var parentWalkablemap = new Map(size, size, value*2);

            return new TerrainData(rect, parentWalkablemap,parentHeightmap);
        }

        public static TerrainData VoronoiIsland(int size, Rect rect)
        {
            RNG.DateTimeInit();
            var seed = RNG.NextFloat(0, 1000);

            var walkableAreaMap = new Map(size, size);

            var cells = new List<VoronoiCell>();

            for (int i = 0; i < 130; i++)
            {
                cells.Add(new VoronoiCell(RNG.NextVector3(0f, 1f)));
            }

            var voronoiGenerator = new VoronoiGenerator(walkableAreaMap, cells);
            var fallOffMap = voronoiGenerator.GetVoronoiBoolMap(walkableAreaMap.Clone().CreateCircularFalloff(size*0.37f).Normalise()).Display();
            var fallOffFineAlright = Map.BooleanIntersection(
                walkableAreaMap
                    .Clone()
                    .CreateCircularFalloff(size * 0.37f)
                    .Invert(),
                walkableAreaMap
                    .Clone()
                    .CreateYGradient()
                    .BooleanMapFromThreshold(0.7f))
                .Display()
                ;
            var otherFallOffMap = voronoiGenerator.GetVoronoiBoolMap(fallOffFineAlright).Display();

            var heightMap = voronoiGenerator
                .GetHeightMap(walkableAreaMap);

            var smallSmooth = heightMap
                .Clone()
                .ApplyMask(fallOffMap)
                .Invert()
                .SmoothMap(2)
                .Display();
            //var absHeight = smallSmooth.ApplyMask(otherFallOffMap).Display();

            var highSmooth = Map.DecimateMap(smallSmooth, 2).SmoothMap(10).Resize(size, size).SmoothMap().Display();
            
            var otherSteepness = smallSmooth.Clone().GetAbsoluteBumpMap().Normalise().Display().BooleanMapFromThreshold(0.35f).Display();
            smallSmooth.Add(walkableAreaMap.Clone().PerlinFill(0.05f * size, 0, 0, seed).Remap(-0.025f, 0.025f));

            var exteriorMap = Map.DecimateMap(otherFallOffMap, 2).SmoothMap(2).Resize(size, size);
            var exteriorBump = exteriorMap.Clone().GetAbsoluteBumpMap().Normalise().Display().BooleanMapFromThreshold(0.5f).Display();
            var finalBump = Map.BooleanUnion(exteriorBump.Invert(), otherSteepness.Invert()).Invert().Display();

            var finalHeightMap = Map.Blend(smallSmooth.Remap(0f, 0.35f), heightMap.Clone().Remap(0.75f, 1f).SmoothMap(2), exteriorMap.Normalise()).Display();

            var terrainData = new TerrainData(rect, finalBump, finalHeightMap.Clamp(0.1f,1f).Normalise(), new ColorLayer(finalBump));
            return terrainData;

        }

        public static TerrainData DelaunayIsland(int size, Rect rect,Transform transform)
        {
            RNG.DateTimeInit();
            var stack = Map.SetGlobalDisplayStack();
            var baseNoiseMap = new Map(size, size);

            baseNoiseMap
                .PerlinFill(size*0.25f,0,0,RNG.Next(1000))
                .Add(
                    baseNoiseMap
                    .Clone()
                    .PerlinFill(size * 0.55f, 0, 0, RNG.Next(1000)).Remap(-0.5f,0.5f))
                .Display();

            var falloffMap = baseNoiseMap.Clone().CreateCircularFalloff(size * 0.45f).Display();
            falloffMap = Map.DecimateMap(falloffMap,4).GetDistanceMap((int)(size * 0.05f)).Resize(size,size).Clamp(0f, 0.5f).Display();

            baseNoiseMap = Map.Blend(Map.BlankMap(size, size).FillWith(0), baseNoiseMap.Remap(0.1f,1f), falloffMap.Normalise()).Display();


            var mesh = MeshMasher.DelaunayGen.GetMeshFromMap(baseNoiseMap, 0.06f);
            mesh = MeshMasher.MeshConnectionsRemover.RemoveEdges(new MeshMasher.SmartMesh(mesh));

            var heightMap = Map.Clone(baseNoiseMap).GetHeightmapFromSquareXZMesh(mesh).SmoothMap(3).Display().Add(
                Map.BlankMap(size, size)
                .PerlinFill(size * 0.125f, 0, 0, RNG.Next(1000))
                .Remap(-0.05f,0.05f))
                .Clamp(0.06f,1f).Normalise();




            var walkableMap = heightMap.Clone().Normalise().GetAbsoluteBumpMap().Display().Normalise().BooleanMapFromThreshold(0.15f).Display();

            heightMap.Add(
                    baseNoiseMap
                    .Clone()
                    .PerlinFill(size * 0.02f, 0, 0, RNG.Next(1000)).Remap(-0.01f, 0.01f));

            var terrainData = new TerrainData(rect, walkableMap, heightMap.Clone().Multiply(200f), new ColorLayer(walkableMap));

            stack.CreateDebugStack(transform);

            return terrainData;
        }

        public static TerrainData DelaunayValley(int size, Rect rect, Transform transform)
        {
            RNG.DateTimeInit();
            var stack = Map.SetGlobalDisplayStack();
            var baseNoiseMap = new Map(size, size);

            baseNoiseMap
                .PerlinFill(size * 0.25f, 0, 0, RNG.Next(1000))
                .Add(
                    baseNoiseMap
                    .Clone()
                    .PerlinFill(size * 0.55f, 0, 0, RNG.Next(1000)).Remap(-0.5f, 0.5f))
                .Display();

            var falloffMap = baseNoiseMap.Clone().CreateCircularFalloff(size * 0.45f).Display();
            falloffMap = Map.DecimateMap(falloffMap, 4).GetDistanceMap((int)(size * 0.1f)).Resize(size, size).Clamp(0f, 0.5f).Display();

            var halfSize = size;

            var blackout = new Map(halfSize, halfSize);
            blackout = blackout
                .SetRow(0, 1)
                .SetRow(halfSize - 1, 1)
                .SetColumn(0, 1)
                .SetColumn(halfSize - 1, 1)
                .GetDistanceMap((int)(halfSize * 0.1f))
                .Resize(size, size)
                .Remap(0f,0.7f)
                .Display();

            var added = falloffMap + blackout;
            added.Normalise().Display();

            baseNoiseMap = added.Add(baseNoiseMap.Remap(0, 0.3f)).Display();




            var mesh = MeshMasher.DelaunayGen.GetMeshFromMap(baseNoiseMap, 0.06f);
            mesh = MeshMasher.MeshConnectionsRemover.RemoveEdges(new MeshMasher.SmartMesh(mesh));

            var heightMap = Map.Clone(baseNoiseMap).GetHeightmapFromSquareXZMesh(mesh).SmoothMap(3).Display().Add(
                Map.BlankMap(size, size)
                .PerlinFill(size * 0.125f, 0, 0, RNG.Next(1000))
                .Remap(-0.05f, 0.05f))
                .Clamp(0.06f, 1f).Normalise();




            var walkableMap = heightMap.Clone().Normalise().GetAbsoluteBumpMap().Display().Normalise().BooleanMapFromThreshold(0.1f).Display();

            var terrainData = new TerrainData(rect, walkableMap, heightMap.Clone().Multiply(200f), new ColorLayer(walkableMap));

            stack.CreateDebugStack(transform);

            return terrainData;
        }

        public static TerrainData DelaunayValleyControlled(int size, Rect rect, Transform transform, Texture2D level,  AnimationCurve curve)
        {
            RNG.DateTimeInit();

            var walkableAreaMap = new Map(size, size);
            var stack = Map.SetGlobalDisplayStack();

            var levelMap = Map.MapFromGrayscaleTexture(level);//.Display();

            var chunks = levelMap.CreateLevelSubMapsFromThisLevelMap(16);

            for (int x = 0; x < chunks.GetLength(0); x++)
            {
                for (int y = 0; y < chunks.GetLength(1); y++)
                {
                    chunks[x, y].GetDistanceMap((16 / 2)).Clamp(0f, 0.5f);
                }
            }

            var knittedChunks = Map.CreateMapFromSubMapsWithoutResizing(chunks);//.Display();

            var seed = RNG.Next(10000);

            var finalMap = knittedChunks.Clone().Resize(size, size)
                .Normalise()
                .Display()
                
                .Add(Map.BlankMap(walkableAreaMap).PerlinFill(size * 0.15f, 1, 0, seed).Remap(-0.25f, 0.25f))
                .Clamp(-0.25f, 0.9f)
                .Normalise()
                .Remap(curve)
                //.Display()
                //BooleanMapFromThreshold(0.5f)
                .Display();


            var mesh = MeshMasher.DelaunayGen.GetMeshFromMap(finalMap, 0.04f);
            mesh = MeshMasher.MeshConnectionsRemover.RemoveEdges(new MeshMasher.SmartMesh(mesh));

            var heightMap = Map.Clone(finalMap).GetHeightmapFromSquareXZMesh(mesh).SmoothMap(3).Normalise().Clamp(0,0.8f).Normalise().Display();

            var walkableMap = heightMap.Clone().Normalise().GetAbsoluteBumpMap().Display().Normalise().BooleanMapFromThreshold(0.1f).Display();

            var terrainData = new TerrainData(rect, walkableMap, heightMap.Clone().Multiply(200f), new ColorLayer(walkableMap));

            stack.CreateDebugStack(transform);

            return terrainData;
        }

        public static TerrainData DelaunayValleyControlledAssumingLevelSet(Maps.Map level, Rect rect, AnimationCurve curve)
        {
            RNG.DateTimeInit();

            var size = TerrainStaticValues.HeightmapResolution;
            var walkableAreaMap = new Map(size, size);
            var stack = Map.SetGlobalDisplayStack();

            var seed = RNG.Next(10000);

            var finalMap = level.Clone().Resize(size, size)
                .Normalise()
                .Display()
                .Add(Map.BlankMap(walkableAreaMap).PerlinFill(size * 0.15f, 1, 0, seed).Remap(-0.25f, 0.25f))
                .Clamp(-0.25f, 0.9f)
                .Normalise()
                .Remap(curve)
                //.Display()
                //BooleanMapFromThreshold(0.5f)
                .Display();

            var mesh = MeshMasher.DelaunayGen.GetMeshFromMap(finalMap, 0.04f);
            mesh = MeshMasher.MeshConnectionsRemover.RemoveEdges(new MeshMasher.SmartMesh(mesh));

            var heightMap = Map.Clone(finalMap).GetHeightmapFromSquareXZMesh(mesh).SmoothMap(3).Normalise().Clamp(0, 0.8f).Normalise().Display();

            var walkableMap = heightMap.Clone().Normalise().GetAbsoluteBumpMap().Display().Normalise().BooleanMapFromThreshold(0.1f).Display();

            var terrainData = new TerrainData(rect, walkableMap, heightMap.Clone().Multiply(200f), new ColorLayer(walkableMap));

            return terrainData;
        }

        static Map _storedFalloffMap = null;

        public static Map Falloffmap(int size)
        {
            if (_storedFalloffMap == null)
            {
                var origDimension = 32;
                var map = new Map(origDimension, origDimension, 1);
                map.SetColumn(0, 0).SetColumn(origDimension-1, 0).SetRow(0, 0).SetRow(origDimension-1, 0);
                map.GetDistanceMap(4);
                map.Clamp(0.6f, 9f);
                map.Normalise();
                map = map.Resize(size, size);
                _storedFalloffMap = map;
            }

            return _storedFalloffMap;

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