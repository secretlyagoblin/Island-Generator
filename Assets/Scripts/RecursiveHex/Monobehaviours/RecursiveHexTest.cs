using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RecursiveHex;

public class RecursiveHexTest : MonoBehaviour
{
    public GameObject Prefab;

    // Start is called before the first frame update
    void Start()
    {
        RNG.Init();

        //var code = 0;

        //var finalMeshes = new HexGroup()
        //    .Subdivide()
        //    .ForEach(x => { x.Height = RNG.NextFloat(10); x.Code = code; code++; })
        //    .Subdivide()
        //    .Subdivide(x => x.Code)
        //    .ForEachHexGroup(x => x.Subdivide())
        //    .ForEachHexGroup(x => x.ToMesh());

        //var code = 0;

        var layer1 = new HexGroup();
        var layer2 = layer1.Subdivide()
            .ForEach(x => new HexPayload()
            {
                Height = RNG.NextFloat(0, 5),
                Color = RNG.NextColor()
            }
            )            
            .Subdivide().Subdivide().Subdivide()//.Subdivide().Subdivide();
        ;

        //layer2.ToGameObjects(Prefab);

        this.gameObject.GetComponent<MeshFilter>().sharedMesh = layer2.ToMesh(x => x.Payload.Height);

        //var gob = new GameObject();
        //gob.AddComponent<MeshFilter>().sharedMesh = layer1.ToMesh();
        //gob.AddComponent<MeshRenderer>();





    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

