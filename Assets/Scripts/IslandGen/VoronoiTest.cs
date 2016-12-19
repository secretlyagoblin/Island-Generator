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

        var map = new Map(size, size);

        map.RandomFillMap(0.51f, 0, 0)
            .AddToStack(stack)
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
            .AddToStack(stack)
            .BoolSmoothOperation(4)
            .AddToStack(stack)
            .RemoveSmallRegions(400)
            //.Invert()
            //.RemoveSmallRegions(300)
            //.Invert()
            .AddToStack(stack)
            .AddRoomLogic()
            .AddToStack(stack);

        var distanceMap = Map.Clone(map)
            .GetDistanceMap(15)
            .Clamp(0.5f, 1f)            
            .Normalise()
            .AddToStack(stack);

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
            .LerpHeightMap(map, ugh)
            .SmoothMap(10)
            .Normalise();
            //.AddToStack(stack);

        finalMap.Normalise().AddToStack(stack);

        heightMap.AddToStack(stack);

        finalMap += heightMap;
        finalMap.AddToStack(stack);
        finalMap.SmoothMap(1).AddToStack(stack);
        
            


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



    // Update is called once per frame
    void Update () {
		
	}
}
