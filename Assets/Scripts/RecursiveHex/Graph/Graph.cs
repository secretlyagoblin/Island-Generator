using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecursiveHex;

public class Graph<T>
{
    private MeshMasher.SmartMesh _smartMesh;
    private MeshCollection<T> _collection;
    private T[] _nodeMetadata;

    public Graph(Vector3[] verts, int[] tris, T[] nodes, System.Func<T, int> identifier, System.Func<T,int[]> connector){
        _smartMesh = new MeshMasher.SmartMesh(verts, tris);
        _nodeMetadata = nodes;

        _collection = new MeshCollection<T>(_smartMesh,nodes, identifier,connector);

        }



    public void DebugDraw(Transform transform)
    {
        _smartMesh.DrawMesh(transform, Color.green, Color.blue);
    }


}
