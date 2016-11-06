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

    }

    //Map Gen Functions

    void GenerateMap(int width, int height)
    {
        if (UseRandomSeed)
        {
            Seed = RNG.Next().ToString();
        }
        RNG.ForceInit(Seed);


        var stack = new MeshDebugStack(Material);

        var map = new Map(width, height);

        var unionMap = CreateWalkableMap(map);
        unionMap.AddRoomLogic();
        stack.RecordMapStateToStack(unionMap);

        var distanceMap = Map.Clone(unionMap).GetDistanceMap(15);
        distanceMap.Normalise();
        stack.RecordMapStateToStack(distanceMap);


        var perlinSeed = RNG.NextFloat(-1000f, 1000f);

        var perlinMap = Map.BlankMap(Size, Size).PerlinFillMap(47.454545f, 0, 2, perlinSeed, 4, 0.5f, 1.87f);
        stack.RecordMapStateToStack(perlinMap);

        var cliffHeightMap = Map.Blend(perlinMap, new Map(Size,Size,0),Map.Clone(distanceMap).Clamp(0.3f, 1f).Normalise());
        stack.RecordMapStateToStack(cliffHeightMap);

        /*

        //Here we will merge these two maps


    */

        var voronoiGenerator = new VoronoiGenerator(map, 0, 0, 0.2f);

        var voronoiMap = voronoiGenerator.GetDistanceMap().Normalise().Remap(voronoiFalloff).Invert();
        stack.RecordMapStateToStack(voronoiMap);

        //var cellEdgeMap = voronoiGenerator.GetFalloffMap(2).Normalise();
        //stack.RecordMapStateToStack(cellEdgeMap);

        var mountainMap = voronoiGenerator.GetHeightMap(cliffHeightMap).Normalise();
        stack.RecordMapStateToStack(mountainMap);

        var blurMap = Map.Clone(mountainMap).SmoothMap(2);
        stack.RecordMapStateToStack(blurMap);

        mountainMap = blurMap + (Map.Clone(voronoiMap).Clamp(0, 0.9f).Remap(0,0.3f));
        mountainMap.Normalise();
        stack.RecordMapStateToStack(mountainMap);

        //var vormap = HeightmeshGenerator.GenerateTerrianMesh(vorHeightmap.Multiply(200), _lens);
        //CreateHeightMesh(vormap);

        var hillMap = cliffHeightMap + (Map.Clone(voronoiMap).Remap(0, 0.05f));
        stack.RecordMapStateToStack(hillMap);

        var isInside = voronoiGenerator.GetVoronoiBoolMap(unionMap);
        //stack.RecordMapStateToStack(insideMap);

        var terrain = Map.Blend(mountainMap, hillMap.Remap(0.1f,1f), isInside.SmoothMap(2));
        stack.RecordMapStateToStack(terrain);

        var heightMesh = HeightmeshGenerator.GenerateTerrianMesh(terrain.Multiply(200), _lens);
        CreateHeightMesh(heightMesh);


        //blendedResult += (voronoiMap.Remap(0f, 0.05f));







        //stack.RecordMapStateToStack(perlinMap);

        distanceMap.Normalise().Clamp(0.4f, 0.8f).Normalise();
        stack.RecordMapStateToStack(distanceMap);

        var mergeMap = Map.Blend(mountainMap.Remap(0.2f,1), new Map(Size,Size,0), isInside);
        stack.RecordMapStateToStack(mergeMap);

        //var mergeMap = perlinMap;

        //var distanceHeightMap = HeightmeshGenerator.GenerateTerrianMesh(mergeMap.Multiply(100f), _lens);
        //CreateHeightMesh(distanceHeightMap);

        var heightmap = CreateHeightMap(unionMap);
        stack.RecordMapStateToStack(heightmap);

        CreateTrees(heightmap, unionMap, 7, 0.4f);

        CreateDebugStack(stack, 200f);
    }

    //Subdividing Map

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

    Map CreateWalkableMap(Map map)
    {
        //CreateWalkableSpace

        map.RandomFillMap(RandomFillPercent, NoiseIntensity, RandomMapPerlinScale);
        //stack.RecordMapStateToStack(map);
        map.ApplyMask(Map.BlankMap(map).CreateCircularFalloff(Size * 0.45f));
        //stack.RecordMapStateToStack(map);
        map.BoolSmoothOperation(4);
        //stack.RecordMapStateToStack(map);
        map.RemoveSmallRegions(RegionSizeCutoff);
        //stack.RecordMapStateToStack(map);

        var roomMap = Map.Clone(map).AddRoomLogic();
        //stack.RecordMapStateToStack(roomMap);

        var thickMap = Map.Clone(roomMap).Invert().ThickenOutline(1).Invert();
        //stack.RecordMapStateToStack(thickMap);

        var differenceMap = Map.BooleanDifference(roomMap, thickMap);
        //stack.RecordMapStateToStack(differenceMap);

        var staticMap = new Map(map);
        staticMap.RandomFillMap(0.4f);

        differenceMap = Map.BooleanIntersection(differenceMap, staticMap);
        //stack.RecordMapStateToStack(differenceMap);

        var unionMap = Map.BooleanUnion(roomMap, differenceMap);
        //stack.RecordMapStateToStack(unionMap);

        unionMap.BoolSmoothOperation(4);
        unionMap.RemoveSmallRegions(100);

        return unionMap;
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