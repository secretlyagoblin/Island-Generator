using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nurbz;

public class TriangleTest : MonoBehaviour {

    public Mesh Mesh;

	// Use this for initialization
	void Start () {

        RNG.DateTimeInit();

        var mesh = new MeshContour(new Plane(Vector3.up,0.47f));

        mesh.ContourMesh(Mesh);
		
	}
	
	// Update is called once per frame
	void Update () {

        //if (Input.GetMouseButtonDown(0))
        //{
        //    var mesh = new MeshContour(new Plane(Vector3.up, 0.47f));
        //    mesh.ContourMesh(Mesh);
        //}
		
	}
}
