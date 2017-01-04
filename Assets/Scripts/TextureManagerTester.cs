﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureManagerTester : MonoBehaviour {

    public Material BaseMaterial;

	// Use this for initialization
	void Start () {

        var textureManager = new TextureManager();

        var blurt = HeightmapPattern.MajorMap(400);
        var gurt = HeightmapPattern.SimpleIsland(400,400);

        
        textureManager.ApplyTexture(blurt,textureManager.RequestCoord());
        textureManager.ApplyTexture(gurt, textureManager.RequestCoord());
        //textureManager.ApplyTextureAndReturnDomain(blurt.GetMap(MapType.WalkableMap));

        var material = new Material(BaseMaterial);
        material.name = "UGH";
        material.mainTexture = textureManager.Texture;

        material.mainTexture.filterMode = FilterMode.Point;

        var obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        obj.GetComponent<MeshRenderer>().sharedMaterial = material;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
