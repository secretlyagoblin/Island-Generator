using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Terrain;

public class RegionController : MonoBehaviour {

    public Material material;

    Terrain.Region _region;
    GameObject _regionObject;

	// Use this for initialization
	void Start () {
        _region = new Region(HeightmapData.RegionIsland(400, new Rect(Vector2.zero, Vector2.one * 1000f)));
        _region.InstantiateRegionCells(transform, material);

        var obj = new GameObject();
        obj.transform.parent = transform;
        obj.name = "DummyCells";

        _region.InstantiateDummyCells(obj.transform, material);
    }

    void Update()
    {
        _region.Update(Vector3.zero, 20);
    }



}
