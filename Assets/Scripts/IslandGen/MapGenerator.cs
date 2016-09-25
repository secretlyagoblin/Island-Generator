using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

public class MapGenerator
    : MonoBehaviour {

    //REMINDER: 1 = Cell on, 0 = Cell off

    [Range(0, 100)]
    public int RandomFillPercent;

    [Range(0, 100)]
    public int NoiseIntensity;

    [Range(0.01f, 10f)]
    public float RandomMapPerlinScale;
    [Range(0.01f, 10f)]
    public float SubzoneGenerationPerlinScale;

    public string Seed;
    public bool UseRandomSeed;
    public int RegionSizeCutoff;

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
        if (Input.GetMouseButtonDown(0))
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            GenerateMap(Size, Size);
        }
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

        var map = new Map(width, height);

        map.RandomFillMap(0.5f, NoiseIntensity, RandomMapPerlinScale);
        map.ApplyMask(Map.CreateBlankMap(map).CreateCircularFalloff());
        map.SmoothMap(4);
        map.RemoveSmallRegions(RegionSizeCutoff);

        var roomMap = Map.CloneMap(map).AddRoomLogic();

        var heightmap = Map.CreateHeightMap(roomMap.GenerateSubMaps(6, 5));

        //var map2 = new int[width, height];
        //var propHeights2 = new int[width, height];
        //RandomFillMap(map2);
        //for (int i = 0; i < Iterations; i++)
        //{
        //    SmoothMap(map2);
        //}
        //
        //map2 = BooleanDifference(map, map2);

        //AddRoomLogic(map2);

        //CreateMesh(GetInvertedMap(map), 5);
        //CreateMesh(GetInvertedMap(map2), 10);


        var subMaps = GenerateSubMaps(6, map);
        var heightMap = CreateNewMapFromTemplate(map);

        var subRegions = new List<List<Coord>>();

        for (int i = 0; i < subMaps.Count; i++)
        {
            SetPropMapHeights(subMaps[i], heightMap, (i));

            subRegions.AddRange(GetRegions(subMaps[i], 0));
        
            //for (int u = 0; u < regions.Count; u++)
            //{
            //    CreateMesh(GetInvertedMap(ResizedMapFromRegion(regions[u])), i);
            //}
        
            //CreateMesh(GetInvertedMap(subMaps[i]), i );
        }



        var layers = GetLayersFromRegions(subRegions, map);



        for (int i = 0; i < layers.Length; i++)
        {
            SetPropMapHeights(layers[i], heightMap, i);
            var count = CountDensity(layers[i]);
            
            //if (count > cellCount)
            //{
            //    cellCount = count;
            //    keyLayer = layers[i];
            //    keyHeight = i;
            //}

            CreateMesh(GetInvertedMap(layers[i]), i);
        }

        CreateTrees(heightMap, 7, 0.4f);

        //MAP TWO ******************************************************

        //var map2 = new int[width, height];
        //propHeights = new int[width, height];
        //RandomFillMap(map2);
        //map2 = BooleanUnion(map2, layers.Last());
        //
        //for (int i = layers.Length-10; i <layers.Length; i++)
        //{
        //    map2 = BooleanDifference(layers[i], map2);
        //}
        //
        //
        //for (int i = 0; i < Iterations; i++)
        //{
        //    SmoothMap(map2);
        //}
        //
        //
        //
        //AddRoomLogic(map2);
        //
        //
        //
        ////map2 = BooleanDifference(layers.Last(), map2);
        //
        //var coord = FindClosestPointInB(layers.Last(), map2);
        //
        //
        //subMaps = GenerateSubMaps(6, map2);
        //heightMap = CreateNewMapFromTemplate(map2);
        //
        //subRegions = new List<List<Coord>>();
        //
        //var layerCountFinal = layers.Count();
        //var finalLayer = layers.Last();
        //
        //for (int i = 0; i < subMaps.Count(); i++)
        //{
        //    subRegions.AddRange(GetRegions(subMaps[i], 0));
        //}
        //
        //layers = GetLayersFromRegions(subRegions, map2, coord);
        //
        //for (int i = 0; i < layers.Length; i++)
        //{
        //    SetPropMapHeights(layers[i], heightMap, i + layerCountFinal);
        //
        //
        //    CreateMesh(GetInvertedMap(layers[i]), i + layerCountFinal);
        //}
        //
        //CreateTrees(heightMap, 7, 0.4f);


    }

    Coord FindClosestPointInB(int[,] mapA, int[,] mapB)
    {
        if (!MapsAreSameDimensions(mapA, mapB))
        {
            Debug.Log("Failed in FindClosestPoint because maps were not valid");
            return new Coord(0, 0);
        }

        Debug.Log("Why are you using this it's blatantly not working properly");

        var width = mapA.GetLength(0);
        var length = mapA.GetLength(1);

        var roomACoords = new List<Coord>();
        var roomBCoords = new List<Coord>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                if (mapA[x, y] == 0)
                {
                    roomACoords.Add(new Coord(x, y));
                }

                if (mapB[x, y] == 0)
                {
                    roomBCoords.Add(new Coord(x, y));
                }
            }
        }

        var bestDistance = int.MaxValue;
        var bestTileA = new Coord();
        var bestTileB = new Coord();

        Debug.Log("I get here");

        var roomA = new Room(roomACoords, mapA);


        var roomB = new Room(roomBCoords, mapB);

        for (int tileIndexA = 0; tileIndexA < roomA.EdgeTiles.Count; tileIndexA++)
        {
            for (int tileIndexB = 0; tileIndexB < roomB.EdgeTiles.Count; tileIndexB++)
            {
                var tileA = roomA.EdgeTiles[tileIndexA];
                var tileB = roomB.EdgeTiles[tileIndexB];
                var distanceBetweenRooms = (int)(Mathf.Pow(tileA.TileX - tileB.TileX, 2) + Mathf.Pow(tileA.TileY - tileB.TileY, 2));
        
                if (distanceBetweenRooms < bestDistance)
                {
                    bestDistance = distanceBetweenRooms;
                    Debug.Log("Best Distance: " + bestDistance);
                    bestTileA = tileA;
                    bestTileB = tileB;
                }
        
            }
        }

        Debug.DrawLine(CoordToWorldPoint(bestTileA,mapA), CoordToWorldPoint(bestTileB, mapB), Color.red, 100f);





        return bestTileB;
    }

    
    


    //Room Connection Functions

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

    //Subdividing Map

    

    void CreateMesh(int[,] subber, int height)
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

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                float perlinX = perlinSeed + (((float)x / (float)width) * perlinScale);
                float perlinY = perlinSeed + (((float)y / (float)length) * perlinScale);

                var perlin = Mathf.PerlinNoise(perlinX, perlinY);
                //Debug.Log(perlinSample);

                if (mask[x,y] != 1 && perlin > treeCutoff)
                {
                    if(perlin > 0.8 && RNG.Next(0, 3) < 1)
                    {
                        treePositions.Add(new Vector3(x - (width * 0.5f), heightMap[x, y], y - (length * 0.5f)));
                    } else if(RNG.Next(0, 10) < 1)
                        treePositions.Add( new Vector3(x-(width*0.5f), heightMap[x,y], y- (length * 0.5f)));
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