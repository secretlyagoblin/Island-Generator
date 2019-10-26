using System.Collections;
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
        RNG.Init("I'd kill");
        //RandomSeedProperties.SetRandomSeed(RNG.NextFloat(-1000, 1000), RNG.NextFloat(-1000, 1000));
        //RandomSeedProperties.Disable();


        //var code = 0;

        //var finalMeshes = new HexGroup()
        //    .Subdivide()
        //    .ForEach(x => { x.Height = RNG.NextFloat(10); x.Code = code; code++; })
        //    .Subdivide()
        //    .Subdivide(x => x.Code)
        //    .ForEachHexGroup(x => x.Subdivide())
        //    .ForEachHexGroup(x => x.ToMesh());

        //var code = 0;

        var colours = new List<Color>();

        for (int i = 0; i < 100; i++)
        {
            colours.Add(RNG.NextColor());
        }



        var layer1 = new HexGroup().ForEach(x => new HexPayload() { Height = 4, Color = Color.white });
        var layer2 = layer1.DebugSubdivide().Subdivide()
            .ForEach((x,i) => new HexPayload()
            {
                Height = 0f,
                Color = RNG.NextColor(),
                //Color = x.Index == Vector2Int.zero ? Color.white:Color.black,
                Code = i
            });;
            ;

        var layer3 = layer2.Subdivide().Subdivide()//.Subdivide()
            .ForEach(x => new HexPayload()
            {
                Height = 0f,
                Color = colours[x.Payload.Code],
                Code = x.Payload.Code
            });
            ;
            
            //.Subdivide();
            //.Subdivide().Subdivide().Subdivide()//.Subdivide().Subdivide();
        ;

        //layer3.ToGameObjects(Prefab);
        //layer1.ToGameObjectsBorder(BorderPrefab);

        this.gameObject.GetComponent<MeshFilter>().sharedMesh = layer3.ToMesh();//(x => x.Payload.Height);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
