﻿using UnityEngine;
using System.Collections.Generic;
using Map;

public class MeshDebugStack {

    List<Layer> _maps;

    Material _defaultMaterial;

    public MeshDebugStack(Material baseMaterial)
    {
        _defaultMaterial = baseMaterial;
        _maps = new List<Layer>();
    }

    public void RecordMapStateToStack(Layer map)
    {
        _maps.Add(Layer.Clone(map));
    }

    public void CreateDebugStack(Transform parent)
    {
        for (int i = 0; i < _maps.Count; i++)
        {
            CreateDebugLayer(_maps[i], i,11, parent);
        }
    }

    GameObject CreateDebugLayer(Layer map, int layer, float heightMultiplier, Transform parent)
    {
        map.Normalise();


        var colors = new List<Color>(map.SizeX * map.SizeY);

        for (int x = 0; x < map.SizeX; x++)
        {
            for (int y = 0; y < map.SizeY; y++)
            {
                var value = map[x, y];
                colors.Add(new Color(value,value,value));
            }
        }

        //Create texture

        var texture = new Texture2D(map.SizeX, map.SizeY);
        texture.SetPixels(colors.ToArray());
        texture.Apply();
        texture.filterMode = FilterMode.Point;


        //Create Material

        var material = new Material(_defaultMaterial);
        material.name = "Layer " + layer;

        material.SetTexture("_MainTex", texture);
        

        //Create Object

        var obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        obj.GetComponent<MeshCollider>().enabled = false;
        obj.GetComponent<MeshRenderer>().sharedMaterial = material;
        obj.transform.parent = parent;
        obj.name = "Layer " + layer;
        obj.layer = parent.gameObject.layer;
        obj.transform.localPosition =(Vector3.right * layer * heightMultiplier);
        

        return obj;

    }

    public void CreateDebugStack(float height)
    {
        var gameObject = new GameObject();
        gameObject.transform.Translate(Vector3.up * (height));
        gameObject.name = "Debug Stack";
        gameObject.layer = 5;

        CreateDebugStack(gameObject.transform);
    }


}
