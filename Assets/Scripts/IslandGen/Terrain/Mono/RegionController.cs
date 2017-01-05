using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Terrain;

public class RegionController : MonoBehaviour {

    public Material Material;
    public Transform TestTransform;

    Region _region;

    

	// Use this for initialization
	void Start () {
        _region = new Region(HeightmapData.BlankMap(400, new Rect(Vector2.zero, Vector2.one * 1000f),32f));

        var obj = new GameObject();
        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;
        obj.name = "CollisionCells";

        _region.InstantiateRegionCells(obj.transform, Material);

        obj = new GameObject();
        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;
        obj.name = "DummyCells";

        _region.InstantiateDummyCells(obj.transform, Material);
    }

    void Update()
    {
        var pos = TestTransform.position;
        _region.Update(transform.InverseTransformPoint(pos), 150);
    }



}
