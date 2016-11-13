using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

public class MapGenerator
    : MonoBehaviour {

    //REMINDER: 1 = Cell on, 0 = Cell off

    public bool CreateProps;
    public GameObject UICamera;

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
    public AnimationCurve LerpMapFalloff;

    public GameObject ParticleBud;

    public Gradient stoneGradient;
    public Gradient grassGradient;
    public Gradient plantTinting;

    public GameObject ThirdPersonController;

    public Material Material;

    public int Size;

    public GameObject[] plantObjects;

    MeshLens _lens;

    [Range(0, 10)]
    public int Iterations;

    MeshGenerator _meshGen = new MeshGenerator();

    //Monobehaviour Stuff

    void Start()
    {
        RNG.Init(DateTime.Now.ToString());

        _lens = new MeshLens(Size, Size, new Vector3(2f,1.3f,2f));        

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

        var perlinSeed = RNG.NextFloat(-1000f, 1000f);

        var distanceMap = Map.Clone(unionMap).GetDistanceMap(15).Normalise();
        var validStartPoint = Map.Clone(distanceMap).Clamp(0.1f, 1).Normalise().GetValidStartLocation();
        var cameraPosition = Map.Clone(distanceMap).Normalise().Clamp(0.3f, 1f).Normalise().GetValidStartLocation();


        distanceMap.Clamp(0.5f,1f);
        distanceMap.Normalise();
        stack.RecordMapStateToStack(distanceMap);

        var cutoffMap = Map.Clone(distanceMap).Clamp(0.5f, 1f);
        cutoffMap.Normalise();
        stack.RecordMapStateToStack(cutoffMap);

        var perlinMap = Map.BlankMap(Size, Size).PerlinFillMap(47.454545f, 0, 2, perlinSeed, 4, 0.5f, 1.87f);
        stack.RecordMapStateToStack(perlinMap);

        //var cliffHeightMap = distanceMap;

        var cliffHeightMap = Map.Blend(perlinMap, distanceMap, Map.Clone(cutoffMap).Clamp(0f, 1f).Normalise());
        stack.RecordMapStateToStack(cliffHeightMap);



        var voronoiGenerator = new VoronoiGenerator(map, 0, 0, 0.2f);

        var voronoiMap = voronoiGenerator.GetDistanceMap().Normalise().Remap(voronoiFalloff).Invert();
        stack.RecordMapStateToStack(voronoiMap);

        var cellEdgeMap = voronoiGenerator.GetFalloffMap(2).Normalise();
        stack.RecordMapStateToStack(cellEdgeMap);

        var mountainMap = voronoiGenerator.GetHeightMap(cliffHeightMap).Normalise();
        stack.RecordMapStateToStack(mountainMap);

        var blurMap = Map.Clone(mountainMap).SmoothMap(2);
        stack.RecordMapStateToStack(blurMap);

        mountainMap = blurMap + (Map.Clone(voronoiMap).Clamp(0, 0.9f).Remap(0, 0.3f));
        mountainMap.Normalise();
        stack.RecordMapStateToStack(mountainMap);

        //var vormap = HeightmeshGenerator.GenerateTerrianMesh(vorHeightmap.Multiply(200), _lens);
        //CreateHeightMesh(vormap);

        var hillVoronoiGenerator = new VoronoiGenerator(map, 0, 0, 0.13f);

        var hillVoronoiMap = hillVoronoiGenerator.GetDistanceMap().Normalise().Remap(voronoiFalloff).Invert();
        stack.RecordMapStateToStack(voronoiMap);

        var hillVoronoiFalloffMap = hillVoronoiGenerator.GetFalloffMap(3).Normalise();
        stack.RecordMapStateToStack(hillVoronoiFalloffMap);

        var hillMap = Map.BlankMap(Size,Size).FillWilth(0f) + (Map.Clone(hillVoronoiMap).Remap(0, 0.07f));
        stack.RecordMapStateToStack(hillMap);

        var isInside = voronoiGenerator.GetVoronoiBoolMap(unionMap);
        //stack.RecordMapStateToStack(insideMap);

        


        var terrain = Map.Blend(mountainMap.Clamp(0.15f,1f).Normalise(), hillMap, Map.Clone(isInside).SmoothMap(1));
        stack.RecordMapStateToStack(terrain);

        /*

        //var distanceMesh = HeightmeshGenerator.GenerateTerrianMesh(distanceMap.Multiply(200), _lens);

        //var text = new Texture2D(Size, Size);
        // distanceMap.ApplyTexture(text);

        // CreateHeightMesh(distanceMesh, text);

        stack.RecordMapStateToStack(distanceMap);


        var perlinSeed = RNG.NextFloat(-1000f, 1000f);

        var perlinMap = Map.BlankMap(Size, Size).PerlinFillMap(47.454545f, 0, 2, perlinSeed, 4, 0.5f, 1.87f);
        stack.RecordMapStateToStack(perlinMap);

        var cliffHeightMap = Map.Blend(perlinMap, new Map(Size,Size,0),Map.Clone(distanceMap).Clamp(0.3f, 1f).Normalise());
        stack.RecordMapStateToStack(cliffHeightMap);

        /*

        //Here we will merge these two maps


        */

        /*

        var voronoiGenerator = new VoronoiGenerator(map, 0, 0, 0.2f);

        var voronoiMap = voronoiGenerator.GetDistanceMap().Normalise().Remap(voronoiFalloff).Invert();
        stack.RecordMapStateToStack(voronoiMap);

        var cellEdgeMap = voronoiGenerator.GetFalloffMap(2).Normalise();
        stack.RecordMapStateToStack(cellEdgeMap);

        var mountainMap = voronoiGenerator.GetHeightMap(cliffHeightMap).Normalise();
        stack.RecordMapStateToStack(mountainMap);

        var blurMap = Map.Clone(mountainMap).SmoothMap(2);
        stack.RecordMapStateToStack(blurMap);

        mountainMap = blurMap + (Map.Clone(voronoiMap).Clamp(0, 0.9f).Remap(0,0.3f));
        mountainMap.Normalise();
        stack.RecordMapStateToStack(mountainMap);

        //var vormap = HeightmeshGenerator.GenerateTerrianMesh(vorHeightmap.Multiply(200), _lens);
        //CreateHeightMesh(vormap);

        var nextMesh = Map.Clone(distanceMap).Remap(distanceFieldFalloff).SmoothMap(2).Normalise();

        var smallerVoronoi = new VoronoiGenerator(map, 0, 0, 0.27f);

        var smallerVoronoiMap = smallerVoronoi.GetDistanceMap().Normalise().Remap(voronoiFalloff).Invert();
        stack.RecordMapStateToStack(voronoiMap);

        var smallerboosts = smallerVoronoi.GetFalloffMap(3).Normalise();
        stack.RecordMapStateToStack(smallerboosts);

        var hillMap = Map.Clone(nextMesh).Remap(0, 0.2f) + (Map.Clone(smallerVoronoiMap).Remap(0, 0.04f));
        stack.RecordMapStateToStack(hillMap);

        var isInside = voronoiGenerator.GetVoronoiBoolMap(unionMap);
        //stack.RecordMapStateToStack(insideMap);

        var offset = 0.03f;

        hillMap.Remap(0f+offset, 0.25f+ offset);

        var terrain = Map.Blend(mountainMap, hillMap, Map.Clone(isInside).SmoothMap(1));
        stack.RecordMapStateToStack(terrain);

        //blendedResult += (voronoiMap.Remap(0f, 0.05f));
        */




        //TextureStuff

        /*

         mapHeight = 100f;

        var superHeight = new Texture2D(Size, Size);
        terrain.ApplyTexture(superHeight);




        

        var heightMesh = HeightmeshGenerator.GenerateTerrianMesh(terrain.Multiply(mapHeight), _lens);
        var heightObject = CreateHeightMesh(heightMesh, texture);
        var couldBeBetterMesh = heightObject.GetComponent<MeshFilter>().mesh;
        var collider = heightObject.GetComponent<MeshCollider>();

        var propMap = new PoissonDiscSampler(Size, Size, 0.45f);

        foreach (var sample in propMap.Samples())
        {
            var tex = texture.GetPixelBilinear(Mathf.InverseLerp(0, Size, sample.x), Mathf.InverseLerp(0, Size, sample.y));
            var heig = superHeight.GetPixelBilinear(Mathf.InverseLerp(0, Size, sample.x), Mathf.InverseLerp(0, Size, sample.y));

            if (tex.grayscale < 0.4f)
            {
                if (heig.grayscale > 0.8f)
                    continue;

                if (heig.grayscale > 0.5f && RNG.NextFloat() < 0.7f)
                    continue;

                if (heig.grayscale < 0.12f && RNG.NextFloat() < 0.99f)
                    continue;


                //Debug.DrawRay(_lens.TransformPosition(new Vector3(sample.x, heig.grayscale*mapHeight, sample.y)), Vector3.up,Color.red,100f);

                var hitpoint = _lens.TransformPosition(new Vector3(sample.x, (heig.grayscale * mapHeight)+1f, sample.y));

                RaycastHit hit;

                if (collider.Raycast(new Ray(hitpoint, -Vector3.up), out hit, 2f))
                {
                    var obj = Instantiate(RNG.GetRandomItem(plantObjects));
                    obj.transform.position = hit.point + (hit.normal*0.1f);
                    obj.transform.up = hit.normal;
                    obj.transform.localScale *= 1.7f;
                }




            }

        }


        
    

        //stack.RecordMapStateToStack(perlinMap);

        distanceMap.Normalise().Clamp(0.4f, 0.8f).Normalise();
        stack.RecordMapStateToStack(distanceMap);

        var mergeMap = Map.Blend(mountainMap.Remap(0.2f,1), new Map(Size,Size,0), isInside);
        stack.RecordMapStateToStack(mergeMap);

    */

        //var mergeMap = perlinMap;

        //var distanceHeightMap = HeightmeshGenerator.GenerateTerrianMesh(mergeMap.Multiply(100f), _lens);
        //CreateHeightMesh(distanceHeightMap);

        var heightmap = CreateHeightMap(unionMap).Normalise();
        stack.RecordMapStateToStack(heightmap);


        heightmap = heightmap.LerpHeightMap(unionMap, LerpMapFalloff).SmoothMap(10).Normalise();
        stack.RecordMapStateToStack(heightmap);

        var terrainTexture = new Texture2D(Size, Size);
        terrain.ApplyTexture(terrainTexture);

        var additiveMap = heightmap + (terrain.Multiply(2.5f));
        stack.RecordMapStateToStack(additiveMap);

        additiveMap.Normalise();

        var heightTexture = new Texture2D(Size, Size);
        additiveMap.ApplyTexture(heightTexture);

        //TextureStuff

        var texture = new Texture2D(Size, Size);

        //var textureStuff = isInside + (Map.BlankMap(Size,Size).RandomFillMap().Remap(0,0.05f) + (Map.BlankMap(Size, Size).RandomFillMap().Remap(0, 0.05f))) + voronoiMap + Map.Clone(voronoiMap).Clamp(0,0.5f).Normalise();
        var textureStuff = Map.Clone(cellEdgeMap).Clamp(0.25f, 1f).Normalise();
        //textureStuff += Map.BlankMap(Size, Size).RandomFillMap().Remap(0, 0.2f) + Map.BlankMap(Size, Size).RandomFillMap().Remap(0, 0.2f);
        textureStuff.Normalise();

        var secondTexture = hillVoronoiGenerator.GetDistanceMap().Invert().Normalise();

        //var secondTexture = Map.Clone(hillVoronoiFalloffMap).Clamp(0.2f, 0.8f).Normalise();
        secondTexture += Map.BlankMap(Size, Size).RandomFillMap().Remap(0, 0.1f) + Map.BlankMap(Size, Size).RandomFillMap().Remap(0, 0.1f);
        secondTexture += Map.Clone(heightmap).Remap(0.3f,0.6f).Normalise().Multiply(2f);
        secondTexture.Normalise();

        stack.RecordMapStateToStack(secondTexture);

        textureStuff.ApplyTexture(texture, stoneGradient);
        secondTexture.ApplyTexture(texture, grassGradient, Map.Clone(isInside).ThickenOutline(0).Invert());
        //textureStuff.ApplyTexture(texture);
        texture.Apply();


        //SampleMapStuff

        var sampleTexture = new Texture2D(Size, Size);

        var secondSampleTexture = Map.Clone(hillVoronoiFalloffMap).Clamp(0.2f, 0.8f).Normalise();
        secondSampleTexture += Map.BlankMap(Size, Size).RandomFillMap().Remap(0, 0.1f) + Map.BlankMap(Size, Size).RandomFillMap().Remap(0, 0.1f);
        secondSampleTexture.Normalise();


        textureStuff.ApplyTexture(sampleTexture, stoneGradient);
        secondSampleTexture.ApplyTexture(sampleTexture, stoneGradient, Map.Clone(isInside).ThickenOutline(0).Invert());
        //textureStuff.ApplyTexture(texture);
        sampleTexture.Apply();




        var mapHeight = 90f;

        var sickHeight = Map.Clone(additiveMap).BooleanMapFromThreshold(0.6f).GetRegions(1);
        var boostHeight = Map.GetCenters(sickHeight);

        

        for (int i = 0; i < boostHeight.Count; i++)
        {
            var vec3 = _lens.TransformPosition(new Vector3(boostHeight[i].TileX, (0.6f + RNG.NextFloat(-0.1f,0f)) * 90f, boostHeight[i].TileY));
            var obj = Instantiate(ParticleBud, vec3, Quaternion.identity);
        }

        var insideTexture = new Texture2D(Size, Size);
        var circleMap = Map.BlankMap(Size,Size).CreateCircularFalloff(Size*0.42f);
        circleMap.ApplyTexture(insideTexture);
        stack.RecordMapStateToStack(circleMap);
        
        

        //stack.RecordMapStateToStack(sickHeight);


        var distanceHeightMap = HeightmeshGenerator.GenerateTerrianMesh(additiveMap.Multiply(mapHeight), _lens);
        var heightObject = CreateHeightMesh(distanceHeightMap, texture);

        var couldBeBetterMesh = heightObject.GetComponent<MeshFilter>().mesh;
        var collider = heightObject.GetComponent<MeshCollider>();

        var propMap = new PoissonDiscSampler(Size, Size, 0.4f);

        if (CreateProps)
        {

            foreach (var sample in propMap.Samples())
            {
                if (insideTexture.GetPixelBilinear(Mathf.InverseLerp(0, Size, sample.x), Mathf.InverseLerp(0, Size, sample.y)).grayscale > 0f)
                    continue;

                var tex = sampleTexture.GetPixelBilinear(Mathf.InverseLerp(0, Size, sample.x), Mathf.InverseLerp(0, Size, sample.y));
                var heig = heightTexture.GetPixelBilinear(Mathf.InverseLerp(0, Size, sample.x), Mathf.InverseLerp(0, Size, sample.y));
                var terra = terrainTexture.GetPixelBilinear(Mathf.InverseLerp(0, Size, sample.x), Mathf.InverseLerp(0, Size, sample.y));

                if (tex.grayscale < 0.4f)
                {

                    if (terra.grayscale > 0.8f)
                        continue;

                    if (terra.grayscale > 0.5f)
                        continue;

                    if (terra.grayscale < 0.05f && RNG.NextFloat() < 0.99f)
                        continue;



                    //Debug.DrawRay(_lens.TransformPosition(new Vector3(sample.x, heig.grayscale*mapHeight, sample.y)), Vector3.up,Color.red,100f);

                    var hitpoint = _lens.TransformPosition(new Vector3(sample.x, (heig.grayscale * mapHeight) + 1f, sample.y));

                    RaycastHit hit;

                    var materialProperties = new MaterialPropertyBlock();
                    MeshRenderer[] renderers;


                    if (collider.Raycast(new Ray(hitpoint, -Vector3.up), out hit, 2f))
                    {

                        float t = RNG.NextFloat(0.0f, 1.0f);
                        materialProperties.SetColor("_Color", plantTinting.Evaluate(t));

                        var obj = Instantiate(RNG.GetRandomItem(plantObjects));

                        renderers = obj.GetComponentsInChildren<MeshRenderer>();

                        for (int i = 0; i < renderers.Length; i++)
                        {
                            renderers[i].SetPropertyBlock(materialProperties);
                        }

                        obj.transform.position = hit.point + (hit.normal * 0.1f);
                        obj.transform.up = hit.normal;
                        obj.transform.localScale *= 1.7f;
                    }




                }
            }

        }

        var heightmapPoint = heightTexture.GetPixelBilinear(Mathf.InverseLerp(0, Size, validStartPoint.TileX), Mathf.InverseLerp(0, Size, validStartPoint.TileY));
        var userLandPoint = _lens.TransformPosition(new Vector3(validStartPoint.TileX, (heightmapPoint.grayscale * mapHeight) + 1f, validStartPoint.TileY));

        

        RaycastHit userHit;


        if (collider.Raycast(new Ray(userLandPoint, -Vector3.up), out userHit, 2f))
        {
            ThirdPersonController.transform.position = userHit.point+(Vector3.up*0.8f);
        }


        userLandPoint = _lens.TransformPosition(new Vector3(cameraPosition.TileX, (heightmapPoint.grayscale * mapHeight+30f) + 1f, cameraPosition.TileY));
        UICamera.transform.position = userLandPoint;



        //CreateTrees(heightmap, unionMap, 7, 0.4f);

        //CreateDebugStack(stack, 200f);
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

    GameObject CreateHeightMesh(HeightMesh heightMesh, Texture2D texture)
    {
        var mesh = heightMesh.CreateMesh();

        var parent = new GameObject();
        parent.name = "HeightMap";
        parent.transform.parent = transform;
        parent.transform.localPosition = Vector3.zero;

        var renderer = parent.AddComponent<MeshRenderer>();
        var material = new Material(Material);
        material.mainTexture = texture;
        material.color = new Color(material.color.r + (RNG.Next(-20, 20) * 0.01f), material.color.g, material.color.b + (RNG.Next(-20, 20) * 0.01f));
        renderer.material = material;
        var filter = parent.AddComponent<MeshFilter>();
        var collider = parent.AddComponent<MeshCollider>();

        collider.sharedMesh = mesh;
        filter.mesh = mesh;

        return parent;

    }

    Map CreateWalkableMap(Map map)
    {
        //CreateWalkableSpace

        map.RandomFillMap(RandomFillPercent, NoiseIntensity, RandomMapPerlinScale);
        //stack.RecordMapStateToStack(map);
        map.ApplyMask(Map.BlankMap(map).CreateCircularFalloff(Size * 0.4f));
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