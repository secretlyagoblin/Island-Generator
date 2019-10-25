﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RecursiveHex;

public class RecursiveHexTest : MonoBehaviour
{
    public GameObject Prefab;
    public GameObject BorderPrefab;

    // Start is called before the first frame update
    void Start()
    {
        RNG.Init();
        RandomSeedProperties.Disable();


        //var code = 0;

        //var finalMeshes = new HexGroup()
        //    .Subdivide()
        //    .ForEach(x => { x.Height = RNG.NextFloat(10); x.Code = code; code++; })
        //    .Subdivide()
        //    .Subdivide(x => x.Code)
        //    .ForEachHexGroup(x => x.Subdivide())
        //    .ForEachHexGroup(x => x.ToMesh());

        //var code = 0;

        var colours = new List<Color>()
        {
            Color.white,
            Color.blue,
            Color.blue,
            Color.blue,
            Color.blue,
            Color.blue,
            Color.blue,
            Color.blue,
            Color.blue,
        };


        var layer1 = new HexGroup().ForEach(x => new HexPayload() { Height = 1, Color = Color.white });
        var layer2 = layer1.Subdivide()
            .ForEach((x,i) => new HexPayload()
            {
                Height = x.Payload.Height,
                Color = colours[i]
            });
            ;

        var layer3 = layer2.Subdivide().Subdivide().Subdivide();
            //.Subdivide().Subdivide().Subdivide()//.Subdivide().Subdivide();
        ;

        //layer1.ToGameObjects(Prefab);
        //layer1.ToGameObjectsBorder(BorderPrefab);

        this.gameObject.GetComponent<MeshFilter>().sharedMesh = layer3.ToMesh(x => x.Payload.Height > 0.75? 5:-5);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
