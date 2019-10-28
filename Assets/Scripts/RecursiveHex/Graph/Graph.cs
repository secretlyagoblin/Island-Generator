using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecursiveHex;

public class Graph<T>
{
    private MeshMasher.SmartMesh _smartMesh;
    private T[] _nodeMetadata;

    public Graph(Vector3[] verts, int[] tris, T[] nodes){
        _smartMesh = new MeshMasher.SmartMesh(verts, tris);
        _nodeMetadata = nodes;
        }

    public void DebugDraw(Transform transform)
    {
        _smartMesh.DrawMesh(transform, Color.green, Color.blue);
    }


}
