using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

public class MapTiling : MonoBehaviour {

    public int Size;
    public Material BaseMaterial;

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
        var stack = new MeshDebugStack(BaseMaterial);

        var size = 5;

        for (int i = 0; i < 10; i++)
        {
            //float perlinScale = 47.454545f;
            float perlinScale = 3f;

            var map = new Map(size, size).PerlinFillMap(perlinScale, new Domain(0.3f,1.8f),new Coord(0,i),new Vector2(0.5f,0.5f), new Vector2(0,0), 2, 0.5f, 1.87f);
            stack.RecordMapStateToStack(map);

            size = (int)(size*1.5f);     
        }

        CreateDebugStack(stack, 0);
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
