using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshMasher;

public class Testing : MonoBehaviour {

    public GameObject InstantiationBase;

    void Start()
    {
        var tiles = new List<SimpleVector2Int>();

        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                tiles.Add(new SimpleVector2Int(x, y));
            }
        }


        var dMesh = new NestedMesh(tiles.ToArray());

        var smartMesh = new SmartMesh(dMesh.CreateMesh());

        smartMesh.DrawMesh(transform);
    }
}

