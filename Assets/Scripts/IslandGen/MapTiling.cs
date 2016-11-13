using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

public class MapTiling : MonoBehaviour {

    public int Size;
    public Material BaseMaterial;

    MeshGenerator _meshGen = new MeshGenerator();
    

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

        for (int i = 0; i < 10; i++)
        {
            var map = new Map(width, height).PerlinFillMap(47.454545f,0,3, 0, i, perlinSeed, 4, 0.5f, 1.87f);
            stack.RecordMapStateToStack(map);            
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
    }

}
