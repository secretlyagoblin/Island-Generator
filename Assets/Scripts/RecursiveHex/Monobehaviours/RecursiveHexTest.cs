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

        for (int i = 0; i < 1000; i++)
        {
            colours.Add(RNG.NextColor());
        }

        var layer1 = new HexGroup().ForEach(x => new HexPayload() { Code = 1, Height = 0, Color = Color.white });

        var layer2 = layer1
            .Subdivide()
            .Subdivide();

        layer2.ToGameObjects(Prefab);
        layer2.ToGameObjectsBorder(BorderPrefab);


        var graph = layer2
            .ToGraph(
                regionIndentifier: x => x.Code,
                regionConnector: x => new int[0])
            .ApplyBlueprint(HighLevelConnectivity.CreateSingleRegion)
            .DebugDrawSubmeshConnectivity(transform);

        //layer2.MassUpdateHexes(graph.Finally(x => { 
        //    var done = x;
        //    done.Code = 3;
        //    return done;
        //}));
            



        //var layer2 = layer1.Subdivide()//.Subdivide()//.Subdivide();
        //    .ForEach((x, i) => new HexPayload()
        //    {
        //        Height = RNG.NextFloat(0, 5),
        //        Color = RNG.NextColor(),
        //        //Color = x.Index == Vector2Int.zero ? Color.white:Color.black,
        //        Code = i
        //    }).Subdivide()
        //                .ForEach((x, i) => new HexPayload()
        //                {
        //                    Height = x.Payload.Height + RNG.NextFloat(-1, 1),
        //                    Color = x.Payload.Color,
        //                    //Color = x.Index == Vector2Int.zero ? Color.white:Color.black,
        //                    Code = i
        //                }).Subdivide()
        //                                       .ForEach((x, i) => new HexPayload()
        //                                       {
        //                                           Height = x.Payload.Height + RNG.NextFloat(-0.25f, 0.25f),
        //                                           Color = x.Payload.Color,
        //                                           //Color = x.Index == Vector2Int.zero ? Color.white:Color.black,
        //                                           Code = i
        //                                       }).Subdivide();
            ;



        //var layer3 = layer2.Subdivide().Subdivide()//.Subdivide()//.Subdivide()
        //    .ForEach(x => new HexPayload()
        //    {
        //        Height = 0f,
        //        //Color = x.Payload.Color,
        //        Color = x.IsBorder?Color.black:colours[x.Payload.Code],
        //        Code = x.Payload.Code
        //    });
        //    ;

        //this.StartCoroutine(FinaliseHexgroup(
        //    layer3.GetSubGroups(x => x.Payload.Code),
        //    x => Finalise(x.Subdivide()))
        //    );

        //layer3.GetSubGroups(x => x.Payload.Code).ForEach(x=> Finalise(x.Subdivide().Subdivide()));
            
            //.Subdivide();
            //.Subdivide().Subdivide().Subdivide()//.Subdivide().Subdivide();
        ;

        //layer2.ToGameObjects(Prefab);
        //Finalise(layer2);
        //layer3.ToGameObjectsBorder(BorderPrefab);
        //
        //this.gameObject.GetComponent<MeshFilter>().sharedMesh = subgroup.ToMesh();//(x => x.Payload.Height);

    }

    IEnumerator FinaliseHexgroup(List<HexGroup> hexGroup, Action<HexGroup> func)
    {
        for (int i = 0; i < hexGroup.Count; i++)
        {
            func(hexGroup[i]);
            yield return null;
        }
    }

    private void Finalise(HexGroup group)
    {
        var gobject = new GameObject();
        gobject.name = "Subregion";
        var renderer = gobject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = this.GetComponent<MeshRenderer>().sharedMaterial;
        gobject.AddComponent<MeshFilter>().sharedMesh = group.ToConnectedMesh(x => x.Height*3, x=> x.Color);
        gobject.transform.parent = this.transform;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(Vector3.up, 5f * Time.deltaTime);
    }
}

public static class HighLevelConnectivity
{
    public static void CreateSingleRegion<T>(MeshCollection<T> meshCollection)
    {
        for (int i = 0; i < meshCollection.Meshes.Length; i++)
        {
            var mesh = meshCollection.Meshes[i];

            if(mesh.Id < 0)
            {
                continue;
            }

            mesh.SetConnectivity(LevelGen.States.DikstraWithRandomisation);


        }
    }
}
