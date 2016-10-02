using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

public class MapGenerator
    : MonoBehaviour {

    //REMINDER: 1 = Cell on, 0 = Cell off

    [Range(0.4f, 0.6f)]
    public float RandomFillPercent;

    [Range(0, 100)]
    public int NoiseIntensity;

    [Range(0.01f, 10f)]
    public float RandomMapPerlinScale;
    [Range(0.01f, 10f)]
    public float SubzoneGenerationPerlinScale;

    public string Seed;
    public bool UseRandomSeed;
    public int RegionSizeCutoff;

    public GameObject ThirdPersonController;

    public Material Material;

    public int Size;

    [Range(0, 10)]
    public int Iterations;

    MeshGenerator _meshGen = new MeshGenerator();

    //Monobehaviour Stuff

    void Start()
    {
        RNG.Init(DateTime.Now.ToString());        

        GenerateMap(Size,Size);
    }

    void Update()
    {
       // if (Input.GetMouseButtonDown(0))
       // {
       //     for (int i = 0; i < transform.childCount; i++)
       //     {
       //         Destroy(transform.GetChild(i).gameObject);
       //     }
       //     GenerateMap(Size, Size);
       // }
    }

    //Map Gen Functions

    void GenerateMap(int width, int height)
    {
        if (UseRandomSeed)
        {
            Seed = RNG.Next().ToString();
        }
        RNG.ForceInit(Seed);

        //Version 1


        var stack = new MeshDebugStack(Material);

        var map = new Map(width, height);

        map.RandomFillMap(RandomFillPercent, NoiseIntensity, RandomMapPerlinScale);
        stack.RecordMapStateToStack(map);


        map.ApplyMask(Map.CreateBlankMap(map).CreateCircularFalloff());
        stack.RecordMapStateToStack(map);


        map.SmoothMap(4);
        stack.RecordMapStateToStack(map);


        map.RemoveSmallRegions(RegionSizeCutoff);
        stack.RecordMapStateToStack(map);


        var roomMap = Map.Clone(map).AddRoomLogic();
        stack.RecordMapStateToStack(roomMap);


        var thickMap = Map.Clone(roomMap).InvertMap().ThickenOutline(1).InvertMap();
        stack.RecordMapStateToStack(thickMap);

        var differenceMap = Map.BooleanDifference(roomMap, thickMap);
        stack.RecordMapStateToStack(differenceMap);

        var staticMap = new Map(width, height);
        staticMap.RandomFillMap(0.4f);

        differenceMap = Map.BooleanIntersection(differenceMap, staticMap);
        stack.RecordMapStateToStack(differenceMap);

        var unionMap = Map.BooleanUnion(roomMap, differenceMap);
        stack.RecordMapStateToStack(unionMap);

        unionMap.SmoothMap(4);
        unionMap.RemoveSmallRegions(100);
        stack.RecordMapStateToStack(unionMap);



        unionMap.AddRoomLogic();


        stack.RecordMapStateToStack(unionMap);

        var subMaps = unionMap.GenerateSubMaps(6, 12);
        var heightmap = Map.CreateHeightMap(subMaps);
        stack.RecordMapStateToStack(heightmap);

        var allRegions = new List<List<Coord>>();

        for (int i = 0; i < subMaps.Length; i++)
        {
            var subMap = subMaps[i];
            //stack.RecordMapStateToStack(subMap);
            allRegions.AddRange(subMap.GetRegions(0));
            //CreateMesh(subMaps[i].RemoveSmallRegions(10).InvertMap(),i);
        }



        var finalSubMaps = Map.BlankMap(width, height).CreateHeightSortedSubmapsFromDijkstrasAlgorithm(allRegions);
        heightmap = Map.CreateHeightMap(finalSubMaps);
        stack.RecordMapStateToStack(heightmap);

        for (int i = 0; i < finalSubMaps.Length; i++)
        {
            //stack.RecordMapStateToStack(finalSubMaps[i]);
            CreateMesh(finalSubMaps[i].RemoveSmallRegions(3).InvertMap(),i);
        }


        CreateTrees(heightmap, unionMap, 7, 0.4f);


        var gameObject = new GameObject();
        gameObject.transform.Translate(Vector3.up * (finalSubMaps.Length+10));
        gameObject.name = "Debug Stack";
        gameObject.layer = 5;

        stack.CreateDebugStack(gameObject.transform);


    }

    //Coord FindClosestPointInB(Map mapA, Map mapB)
    //{
    //    if (!Map.MapsAreSameDimensions(mapA, mapB))
    //    {
    //        Debug.Log("Failed in FindClosestPoint because maps were not valid");
    //        return new Coord(0, 0);
    //    }
    //
    //    Debug.Log("Why are you using this it's blatantly not working properly");
    //
    //    var width = mapA.GetLength(0);
    //    var length = mapA.GetLength(1);
    //
    //    var roomACoords = new List<Coord>();
    //    var roomBCoords = new List<Coord>();
    //
    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int y = 0; y < length; y++)
    //        {
    //            if (mapA[x, y] == 0)
    //            {
    //                roomACoords.Add(new Coord(x, y));
    //            }
    //
    //            if (mapB[x, y] == 0)
    //            {
    //                roomBCoords.Add(new Coord(x, y));
    //            }
    //        }
    //    }
    //
    //    var bestDistance = int.MaxValue;
    //    var bestTileA = new Coord();
    //    var bestTileB = new Coord();
    //
    //    Debug.Log("I get here");
    //
    //    var roomA = new Room(roomACoords, mapA);
    //
    //
    //    var roomB = new Room(roomBCoords, mapB);
    //
    //    for (int tileIndexA = 0; tileIndexA < roomA.EdgeTiles.Count; tileIndexA++)
    //    {
    //        for (int tileIndexB = 0; tileIndexB < roomB.EdgeTiles.Count; tileIndexB++)
    //        {
    //            var tileA = roomA.EdgeTiles[tileIndexA];
    //            var tileB = roomB.EdgeTiles[tileIndexB];
    //            var distanceBetweenRooms = (int)(Mathf.Pow(tileA.TileX - tileB.TileX, 2) + Mathf.Pow(tileA.TileY - tileB.TileY, 2));
    //    
    //            if (distanceBetweenRooms < bestDistance)
    //            {
    //                bestDistance = distanceBetweenRooms;
    //                Debug.Log("Best Distance: " + bestDistance);
    //                bestTileA = tileA;
    //                bestTileB = tileB;
    //            }
    //    
    //        }
    //    }
    //
    //    Debug.DrawLine(CoordToWorldPoint(bestTileA,mapA), CoordToWorldPoint(bestTileB, mapB), Color.red, 100f);
    //
    //
    //
    //
    //
    //    return bestTileB;
    //}

    
    


    //Room Connection Functions

        /*

    int[,] ResizedMapFromRegion(List<Coord> regions)
    {
        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = 0;
        var maxY = 0;

        for (int i = 0; i < regions.Count; i++)
        {
            var x = regions[i].TileX;
            var y = regions[i].TileY;

            if (x < minX)
                minX = x;
            if (y < minY)
                minY = y;
            if (x > maxX)
                maxX = x;
            if (y > maxY)
                maxY = y;
        }

        var rangeX = maxX - minX;
        var rangeY = maxY - minY;

        var map = new int[rangeX+2, rangeY+2]; //might be wrong

        map = CreateNewMapFromTemplate(map, 1);

        for (int i = 0; i < regions.Count; i++)
        {
            var x = regions[i].TileX - minX +1;
            var y = regions[i].TileY - minY +1;

            map[x, y] = 0;
        }

        return map;
    }
    */

    //Subdividing Map

    

    void CreateMesh(Map subber, int height)
    {
        var mesh = _meshGen.GenerateMesh(subber, 1f, RNG.Next());

        var parent = new GameObject();
        parent.name = "Geometry Layer " + height;
        parent.transform.parent = transform;
        parent.transform.localPosition = Vector3.zero+(Vector3.up* height);

        var renderer = parent.AddComponent < MeshRenderer>();
        var material = new Material(Material);
        material.color = new Color(material.color.r + (RNG.Next(-20, 20) * 0.01f), material.color.g, material.color.b + (RNG.Next(-20, 20) * 0.01f));
        renderer.material = material;
        var filter = parent.AddComponent<MeshFilter>();
        var collider = parent.AddComponent<MeshCollider>();

        collider.sharedMesh = mesh;
        filter.mesh = mesh;

    }

    //Prop Placement

    void CreateTrees(Map heightMap, Map mask,float perlinScale, float treeCutoff)
    {
        var width = heightMap.SizeX;
        var length = heightMap.SizeY;

        var treePositions = new List<Vector3>();

        var perlinSeed = RNG.NextFloat(0,1000);

        var thirdPersonPositionSet = false;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                float perlinX = perlinSeed + (((float)x / (float)width) * perlinScale);
                float perlinY = perlinSeed + (((float)y / (float)length) * perlinScale);

                var perlin = Mathf.PerlinNoise(perlinX, perlinY);
                //Debug.Log(perlinSample);

                if (mask[x, y] != 1 && perlin > treeCutoff)
                {
                    if (perlin > 0.8 && RNG.Next(0, 3) < 1)
                    {
                        treePositions.Add(new Vector3(x - (width * 0.5f), heightMap[x, y], y - (length * 0.5f)));
                    }
                    else if (RNG.Next(0, 10) < 1)
                    {
                        treePositions.Add(new Vector3(x - (width * 0.5f), heightMap[x, y], y - (length * 0.5f)));
                    }
                }
                else if (mask[x, y] != 1 && RNG.Next(0, 100) < 1)
                {
                    treePositions.Add(new Vector3(x - (width * 0.5f), heightMap[x, y], y - (length * 0.5f)));
                }

            }
        }

        var meshes = TreeCreatorAndBatcher.CreateTreeMeshesFromPositions(1f, 0.3f, 4f, 0.3f,1f, treePositions.ToArray());

        for (int i = 0; i < meshes.Length; i++)
        {
            var mesh = meshes[i];

            var parent = new GameObject();
            parent.name = "Tree Mesh " + i;
            parent.transform.parent = transform;
            parent.transform.localPosition = Vector3.zero;

            var renderer = parent.AddComponent<MeshRenderer>();
            var material = new Material(Material);
            material.color = new Color(material.color.r + (RNG.Next(-20, 20) * 0.01f), 1f, material.color.b + (RNG.Next(-20, 20) * 0.01f));
            renderer.material = material;
            var filter = parent.AddComponent<MeshFilter>();

            filter.mesh = mesh;
        }        
    }

    //Debug

    void DebugZone(int[,] map, float height)
    {
        var parent = new GameObject();
        parent.transform.parent = transform;
        parent.transform.localPosition = Vector3.zero;

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y] == 1)
                {
                    var gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    gameObject.transform.parent = parent.transform;
                    gameObject.transform.localPosition = new Vector3(x - (map.GetLength(0) * 0.5f), height, y - (map.GetLength(1) * 0.5f));
                }
            }
        }
    }

    //A* through zonez



}