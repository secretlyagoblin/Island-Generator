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

        var layer1 = new HexGroup();
        var layer2 = layer1.Subdivide()
            .ForEach(x => new HexPayload()
            {
                Height = RNG.NextFloat(0, 5),
                Color = x.Index == Vector2Int.zero? Color.blue: Color.green
            }
            ); ;
        
        var layer3 = layer2.Subdivide().Subdivide()
            //.Subdivide().Subdivide().Subdivide()//.Subdivide().Subdivide();
        ;

        layer3.ToGameObjects(Prefab);

        this.gameObject.GetComponent<MeshFilter>().sharedMesh = layer3.ToMesh();//(x => x.Payload.Height);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

