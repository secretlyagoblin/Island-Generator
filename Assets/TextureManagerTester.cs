using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureManagerTester : MonoBehaviour {

    public Material BaseMaterial;

	// Use this for initialization
	void Start () {

        var textureManager = new TextureManager();

        var blurt = MapPattern.CliffHillDiffMap(300);
        var gurt = MapPattern.SimpleIsland(400,200);

        textureManager.ApplyTextureAndReturnDomain(blurt.GetMap(MapType.HeightMap));
        textureManager.ApplyTextureAndReturnDomain(gurt);
        textureManager.ApplyTextureAndReturnDomain(blurt.GetMap(MapType.WalkableMap));

        var material = new Material(BaseMaterial);
        material.name = "UGH";
        material.mainTexture = textureManager.Texture;

        var obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        obj.GetComponent<MeshRenderer>().sharedMaterial = material;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
