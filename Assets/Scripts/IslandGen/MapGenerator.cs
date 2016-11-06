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

    public AnimationCurve distanceFieldFalloff;
    public AnimationCurve voronoiFalloff;

    public GameObject ThirdPersonController;

    public Material Material;

    public int Size;

    MeshLens _lens;

    [Range(0, 10)]
    public int Iterations;

    MeshGenerator _meshGen = new MeshGenerator();

    //Monobehaviour Stuff

    void Start()
    {
        RNG.Init(DateTime.Now.ToString());

        _lens = new MeshLens(Size, Size, new Vector3(4f,1,4f));        

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

        //CreateWalkableSpace

        map.RandomFillMap(RandomFillPercent, NoiseIntensity, RandomMapPerlinScale);
        //stack.RecordMapStateToStack(map);
        map.ApplyMask(Map.BlankMap(map).CreateCircularFalloff(Size*0.45f));
        //stack.RecordMapStateToStack(map);
        map.SmoothMap(4);
        //stack.RecordMapStateToStack(map);
        map.RemoveSmallRegions(RegionSizeCutoff);
        //stack.RecordMapStateToStack(map);

        var roomMap = Map.Clone(map).AddRoomLogic();
        //stack.RecordMapStateToStack(roomMap);

        var thickMap = Map.Clone(roomMap).Invert().ThickenOutline(1).Invert();
        //stack.RecordMapStateToStack(thickMap);

        var differenceMap = Map.BooleanDifference(roomMap, thickMap);
        //stack.RecordMapStateToStack(differenceMap);

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

        var finalMap = unionMap;


        stack.RecordMapStateToStack(unionMap);

        var distanceMap = unionMap.GetDistanceMap(15);
        distanceMap.Normalise();

        //Here we will merge these two maps

        var perlinSeed = RNG.NextFloat(-1000f, 1000f);

        var perlinMap = Map.BlankMap(Size, Size).PerlinFillMap(27.454545f, 0, 0, perlinSeed, 5, 0.5f, 1.87f);
        stack.RecordMapStateToStack(perlinMap);

        //No tile no nicely
        perlinMap = Map.BlankMap(Size, Size).PerlinFillMap(47.454545f, 0, 2, perlinSeed, 4, 0.5f, 1.87f);
        stack.RecordMapStateToStack(perlinMap);

        var voronoiGenerator = new VoronoiGenerator(map, 0, 0, 0.2f);

        var voronoiMap = voronoiGenerator.GetDistanceMap().Normalise().Remap(voronoiFalloff).Invert();
        stack.RecordMapStateToStack(voronoiMap);

        var cellEdgeMap = voronoiGenerator.GetFalloffMap(2).Normalise();
        stack.RecordMapStateToStack(cellEdgeMap);

        var vorHeightmap = voronoiGenerator.GetHeightMap(perlinMap).Normalise().Multiply(100);
        stack.RecordMapStateToStack(vorHeightmap);

        var blendedResult = Map.Blend(vorHeightmap.Normalise(), perlinMap.Normalise(), cellEdgeMap);
        stack.RecordMapStateToStack(blendedResult);

        blendedResult += voronoiMap.Remap(0f, 0.2f);
        stack.RecordMapStateToStack(blendedResult);

        blendedResult.Normalise().Multiply(200f);

        //var vormap = HeightmeshGenerator.GenerateTerrianMesh(blendedResult.Multiply(200), _lens);
        //CreateHeightMesh(vormap);

        //(perlinMap += (voronoiMap.Remap(0,0.3f))).Normalise();

        //stack.RecordMapStateToStack(perlinMap);

        perlinMap.Remap(0.3f, 1f).Normalise();

        var mergeMap = Map.Blend(blendedResult, new Map(Size,Size,0), distanceMap.Normalise().Clamp(0.4f,0.7f).Normalise());
        stack.RecordMapStateToStack(distanceMap);

        //var mergeMap = perlinMap;

        mergeMap.Multiply(100f);
        stack.RecordMapStateToStack(mergeMap);

        //distanceMap.Remap(distanceFieldFalloff);
        distanceMap.Normalise();
        //distanceMap.Multiply(100f);
        stack.RecordMapStateToStack(distanceMap);

        var distanceHeightMap = HeightmeshGenerator.GenerateTerrianMesh(blendedResult, _lens);
        CreateHeightMesh(distanceHeightMap);



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
            //CreateMesh(finalSubMaps[i].RemoveSmallRegions(3).InvertMap(),i);
        }


        CreateTrees(heightmap, unionMap, 7, 0.4f);

        CreateDebugStack(stack, 200f);

    }

    //Subdividing Map

    void CreateDebugStack(MeshDebugStack stack, float height)
    {
        var gameObject = new GameObject();
        gameObject.transform.Translate(Vector3.up * (height));
        gameObject.name = "Debug Stack";
        gameObject.layer = 5;

        stack.CreateDebugStack(gameObject.transform);
    }

    void CreateMesh(Map map, int height)
    {
        var mesh = _meshGen.GenerateMesh(map, _lens, RNG.Next());

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

    void CreateHeightMesh(HeightMesh heightMesh)
    {
        var mesh = heightMesh.CreateMesh();

        var parent = new GameObject();
        parent.name = "HeightMap";
        parent.transform.parent = transform;
        parent.transform.localPosition = Vector3.zero;

        var renderer = parent.AddComponent<MeshRenderer>();
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
                        treePositions.Add(_lens.TransformPosition(x, heightMap[x, y], y));
                    }
                    else if (RNG.Next(0, 10) < 1)
                    {
                        treePositions.Add(_lens.TransformPosition(x, heightMap[x,y],y));
                    }
                }
                else if (mask[x, y] != 1 && RNG.Next(0, 100) < 1)
                {
                    treePositions.Add(_lens.TransformPosition(x, heightMap[x, y], y));
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
}