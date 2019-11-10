using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RecursiveHex;
using System;

public class Graph<T> where T:struct
{
    private MeshMasher.SmartMesh _smartMesh;
    private MeshCollection<T> _collection;
    private T[] _nodeMetadata;

    public Graph(Vector3[] verts, int[] tris, T[] nodes, System.Func<T, int> identifier, System.Func<T,int[]> connector){
        _smartMesh = new MeshMasher.SmartMesh(verts, tris);
        _nodeMetadata = nodes;

        _collection = new MeshCollection<T>(_smartMesh,nodes, identifier,connector);

        }

    public Graph<T> DebugDraw(Transform transform)
    {
        _smartMesh.DrawMesh(transform, Color.green, Color.blue);

        return this;
    }

    public Graph<T> DebugDrawSubmeshConnectivity(Transform transform)
    {
        for (int i = 0; i < _collection.Meshes.Length; i++)
        {
            var mesh = _collection.Meshes[i];

            mesh.DebugDraw(Color.green, 100f);
        }

        return this;
    }

    internal Graph<T> ApplyBlueprint(Action<MeshCollection<T>> blueprint)
    {
        blueprint(_collection);

        return this;
    }

    internal T[] Finally(Func<T, int[],T> finallyDo)
    {
        var outT = new T[_nodeMetadata.Length];

        //for (int i = 0; i < _collection.Meshes.Length; i++)
        //{
        //    var m = _collection.Meshes[i];
        //
        //    for (int u = 0; u < m.ConnectionsFromState; u++)
        //    {
        //
        //    }
        //}

        throw new NotImplementedException();


        for (int i = 0; i < outT.Length; i++)
        {
            //outT[i] = finallyDo(_nodeMetadata[i]);
        }

        return outT;
    }
 }
