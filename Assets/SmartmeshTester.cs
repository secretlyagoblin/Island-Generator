using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshMasher;

public class SmartmeshTester : MonoBehaviour {

    public Mesh Mesh;
    SmartMesh _mesh;

	// Use this for initialization
	void Start () {

        _mesh = new SmartMesh(Mesh);
        var lines = _mesh.Lines;

        _mesh.DrawMesh(transform);
		
	}
	
	// Update is called once per frame
	void Update () {

        _mesh.DrawMesh(transform);

    }
}
