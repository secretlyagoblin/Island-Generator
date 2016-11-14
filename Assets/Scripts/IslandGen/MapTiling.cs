using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

public class MapTiling : MonoBehaviour {

    public int Size;
    public Material DebugMaterial;
    public Material TerrianMaterial;

    MarchingSquaresMeshGenerator _meshGen = new MarchingSquaresMeshGenerator();
    

    // Use this for initialization
    void Start()
    {
        RNG.Init(DateTime.Now.ToString());

        //_lens = new MeshLens(Size, Size, new Vector3(2f, 1.3f, 2f));

        GenerateMap(Size, Size);
    }

    // Update is called once per frame

    void GenerateMap(int width, int height)
    {
        var perlinSeed = RNG.NextFloat(-1000f, 1000f);
        var stack = new MeshDebugStack(DebugMaterial);
        var lens = new MeshLens(new Vector3(70, 50, 70));


        var size = 5;

        //for (int x = 0; x < 3; x++)
        //{
        //    for (int y = 0; y < 3; y++)
        //    {
        //        float perlinScale = 3f;
        //
        //        var map = new Map(size, size).PerlinFillMap(perlinScale, new Domain(0.3f, 1.8f), new Coord(x, y), new Vector2(0.5f, 0.5f), new Vector2(0, 0), 7, 0.5f, 1.87f);
        //        //stack.RecordMapStateToStack(map);
        //
        //        //Debug.Log("Start mesh");
        //
        //        var meshGen = HeightmeshGenerator.GenerateHeightmeshPatch(map, lens);
        //
        //        size++;
        //
        //        //Debug.Log("End Mesh");
        //
        //        var mesh = CreateHeightMesh(meshGen, new Coord(x, y), lens);
        //    }
        //}

        var coordA = new Coord(0, 0);
        var coordB = new Coord(0, -1);

        var mapA = new Map(4, 4).PerlinFillMap(3f, new Domain(0.3f, 1.8f), coordA, new Vector2(0.5f, 0.5f), new Vector2(0, 0), 7, 0.5f, 1.87f);
        CreateHeightMesh(HeightmeshGenerator.GenerateHeightmeshPatch(mapA, lens),coordA,lens);
        var mapB = new Map(4, 4).PerlinFillMap(3f, new Domain(0.3f, 1.8f), coordB, new Vector2(0.5f, 0.5f), new Vector2(0, 0), 7, 0.5f, 1.87f);
        CreateHeightMesh(HeightmeshGenerator.GenerateHeightmeshPatch(mapB, lens), coordB, lens);

        HeightmeshGenerator.GenerateMeshSeam(mapA, coordA, mapB, coordB, lens);
    }

    GameObject CreateHeightMesh(HeightmeshPatch heightMesh, Coord tile, MeshLens lens)
    {
        var mesh = heightMesh.CreateMesh();

        var parent = new GameObject();
        parent.name = "HeightMap";
        parent.transform.parent = transform;
        parent.transform.localPosition = Vector3.zero + lens.TransformNormalisedPosition(tile.TileX,0,tile.TileY);
        

        var renderer = parent.AddComponent<MeshRenderer>();
        var material = new Material(TerrianMaterial);
        //material.mainTexture = texture;
        material.color = new Color(material.color.r + (RNG.Next(-20, 20) * 0.01f), material.color.g, material.color.b + (RNG.Next(-20, 20) * 0.01f));
        renderer.material = material;
        var filter = parent.AddComponent<MeshFilter>();
        var collider = parent.AddComponent<MeshCollider>();

        collider.sharedMesh = mesh;
        filter.mesh = mesh;

        return parent;

    }

    void CreateDebugStack(MeshDebugStack stack, float height)
    {
        var gameObject = new GameObject();
        gameObject.transform.Translate(Vector3.up * (height));
        gameObject.name = "Debug Stack";
        gameObject.layer = 5;

        stack.CreateDebugStack(gameObject.transform);

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            gameObject.transform.GetChild(i).localScale = new Vector3(1.1f, 1.1f, 1.1f);
        }
    }

}
