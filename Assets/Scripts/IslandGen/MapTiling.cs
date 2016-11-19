using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

public class MapTiling : MonoBehaviour {

    public int Size;
    public Material DebugMaterial;
    public Material TerrianMaterial;
    //public Transform center;

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
        var lens = new MeshLens(new Vector3(400, 400, 400));
        var size = 128;

        var perlinScale = 3f;

        var mapCount = 6;

        var MapArray = new Map[mapCount, mapCount];

        for (int x = 0; x < mapCount; x++)
        {
            for (int y = 0; y < mapCount; y++)
            {

                MapArray[x,y] = new Map(size, size).PerlinFillMap(perlinScale, new Domain(0.3f, 1.8f), new Coord(x, y), new Vector2(0.5f, 0.5f), new Vector2(0, 0), 7, 0.5f, 1.87f).Clamp(1,2f);        
                //size += 5;
            }
        }

        for (int x = 0; x < mapCount - 1; x++)
        {
            for (int y = 0; y < mapCount - 1; y++)
            {
                if (x != mapCount - 1 & y != mapCount - 1)
                {
                    var meshBase = HeightmeshGenerator.GenerateHeightmeshPatch(MapArray[x, y], lens).CreateMesh();
                    var meshSeamA = HeightmeshGenerator.GenerateMeshSeam(MapArray[x, y], new Coord(x, y), MapArray[x + 1, y], new Coord(x + 1, y), lens).CreateMesh();
                    var meshSeamB = HeightmeshGenerator.GenerateMeshSeam(MapArray[x, y], new Coord(x, y), MapArray[x , y+1], new Coord(x, y+1), lens).CreateMesh();

                    CreateHeightMesh(meshBase, new Coord(x, y), lens);
                    CreateHeightMesh(meshSeamA, new Coord(x, y), lens);
                    CreateHeightMesh(meshSeamB, new Coord(x, y), lens);



                }
            }
        }
        //
        //        var coordA = new Coord(0, 0);
        //var coordB = new Coord(0, 1);
        //var coordC = new Coord(1, 0);
        //var coordD = new Coord(1, 1);
        //
        //var mapA = new Map(8, 8).PerlinFillMap(3f, new Domain(0.3f, 1.8f), coordA, new Vector2(0.5f, 0.5f), new Vector2(0, 0), 7, 0.5f, 1.87f);
        //CreateHeightMesh(HeightmeshGenerator.GenerateHeightmeshPatch(mapA, lens),coordA,lens);
        //var mapB = new Map(16, 16).PerlinFillMap(3f, new Domain(0.3f, 1.8f), coordB, new Vector2(0.5f, 0.5f), new Vector2(0, 0), 7, 0.5f, 1.87f);
        //CreateHeightMesh(HeightmeshGenerator.GenerateHeightmeshPatch(mapB, lens), coordB, lens);
        //var mapC = new Map(28, 28).PerlinFillMap(3f, new Domain(0.3f, 1.8f), coordC, new Vector2(0.5f, 0.5f), new Vector2(0, 0), 7, 0.5f, 1.87f);
        //CreateHeightMesh(HeightmeshGenerator.GenerateHeightmeshPatch(mapC, lens), coordC, lens);
        //var mapD = new Map(28, 28).PerlinFillMap(3f, new Domain(0.3f, 1.8f), coordD, new Vector2(0.5f, 0.5f), new Vector2(0, 0), 7, 0.5f, 1.87f);
        //CreateHeightMesh(HeightmeshGenerator.GenerateHeightmeshPatch(mapD, lens), coordD, lens);
        //
        //CreateSeam(HeightmeshGenerator.GenerateMeshSeam(mapB, coordB, mapA, coordA, lens), coordA, lens);
        //CreateSeam(HeightmeshGenerator.GenerateMeshSeam(mapC, coordC, mapA, coordA, lens), coordC, lens);
        //
        //CreateSeam(HeightmeshGenerator.GenerateMeshSeam(mapD, coordD, mapB, coordB, lens), coordD, lens);
        //CreateSeam(HeightmeshGenerator.GenerateMeshSeam(mapD, coordD, mapC, coordC, lens), coordD, lens);
         //HeightmeshGenerator.GenerateMeshSeam(mapA, coordA, mapC, coordC, lens);
    }

    GameObject CreateHeightMesh(Mesh mesh, Coord tile, MeshLens lens)
    {
        var parent = new GameObject();
        parent.name = "Patch " + tile.TileX + " " + tile.TileY;
        parent.transform.parent = transform;
        parent.transform.localPosition = Vector3.zero + lens.TransformNormalisedPosition(tile.TileX,0,tile.TileY);
        

        var renderer = parent.AddComponent<MeshRenderer>();
        var material = TerrianMaterial;
        //material.mainTexture = texture;
        //material.color = new Color(material.color.r + (RNG.Next(-20, 20) * 0.01f), material.color.g, material.color.b + (RNG.Next(-20, 20) * 0.01f));
        renderer.sharedMaterial = material;
        var filter = parent.AddComponent<MeshFilter>();
        var collider = parent.AddComponent<MeshCollider>();

        collider.sharedMesh = mesh;
        filter.mesh = mesh;

        return parent;

    }

    GameObject CreateSeam(HeightmeshSeam seam, Coord tile, MeshLens lens)
    {
        var mesh = seam.CreateMesh();

        var parent = new GameObject();
        parent.name = "Seam";
        parent.transform.parent = transform;
        parent.transform.localPosition = Vector3.zero + lens.TransformNormalisedPosition(tile.TileX, 0, tile.TileY);


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
