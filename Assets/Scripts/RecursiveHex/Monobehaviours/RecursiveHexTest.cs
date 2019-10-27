using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RecursiveHex;
using Unity.Jobs;
using Unity.Collections;

public class RecursiveHexTest : MonoBehaviour
{
    public GameObject Prefab;
    public GameObject BorderPrefab;

    // Start is called before the first frame update
    void Start()
    {
        //RNG.Init("I'd kill");
        RNG.Init();
        RandomSeedProperties.SetRandomSeed(RNG.NextFloat(-1000, 1000), RNG.NextFloat(-1000, 1000));
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
        var layer2 = layer1.Subdivide().Subdivide()//.Subdivide();
            .ForEach((x,i) => new HexPayload()
            {
                Height = 0f,
                Color = RNG.NextColor(),
                //Color = x.Index == Vector2Int.zero ? Color.white:Color.black,
                Code = i
            });;
            ;

        var layer3 = layer2.Subdivide()//.Subdivide().Subdivide()//.Subdivide()
            .ForEach(x => new HexPayload()
            {
                Height = 0f,
                Color = x.Payload.Color,
                //Color = x.IsBorder?Color.black:colours[x.Payload.Code],
                Code = x.Payload.Code
            });
            ;

        //this.StartCoroutine(FinaliseHexgroup(
        //    layer3.GetSubGroups(x => x.Payload.Code),
        //    x => TryFinaliseHexGroup(x.Subdivide().Subdivide()))
        //    );

        this.StartCoroutine(TryFinaliseHexGroup(layer3));

        //layer3.GetSubGroups(x => x.Payload.Code).ForEach(x=> Finalise(x.Subdivide().Subdivide()));
            
            //.Subdivide();
            //.Subdivide().Subdivide().Subdivide()//.Subdivide().Subdivide();
        ;

        //layer3.ToGameObjects(Prefab);
        //layer3.ToGameObjectsBorder(BorderPrefab);
        //
        //this.gameObject.GetComponent<MeshFilter>().sharedMesh = subgroup.ToMesh();//(x => x.Payload.Height);

    }

    IEnumerator FinaliseHexgroup(List<HexGroup> hexGroups, Action<HexGroup> func)
    {
        for (int i = 0; i < hexGroups.Count; i++)
        {
            Debug.Log("Whoo");
            func(hexGroups[i]);
            yield return null;
        }
    }

    IEnumerator TryFinaliseHexGroup(HexGroup hexGroup)
    {
        var result = hexGroup.ToMeshParallel();

        foreach (var iteration in result)
        {
            Debug.Log("Whaa");
            if (iteration == null)
            {
                yield return null;
            }
            else
            {
                Debug.Log("Scunt");
                Finalise(iteration);
            }
        }

    }

    private void Finalise(Mesh mesh)
    {
        var gobject = new GameObject();
        gobject.name = "Subregion";
        var renderer = gobject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = this.GetComponent<MeshRenderer>().sharedMaterial;
        gobject.AddComponent<MeshFilter>().sharedMesh = mesh;
        gobject.transform.parent = this.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


