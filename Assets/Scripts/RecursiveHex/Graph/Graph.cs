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

    private Func<T, int> _identifier;

    public Graph(Vector3[] verts, int[] tris, T[] nodes, Func<T, int> identifier, Func<T,int[]> connector){
        _smartMesh = new MeshMasher.SmartMesh(verts, tris);
        _nodeMetadata = nodes;
        _identifier = identifier;

        _collection = new MeshCollection<T>(_smartMesh,nodes, _identifier, connector);

        }

    public Graph<T> DebugDraw(Transform transform)
    {
        _smartMesh.DrawMesh(transform, Color.white*0.5f, Color.blue);

        return this;
    }

    public Graph<T> DebugDrawSubmeshConnectivity(Color color)
    {
        //_collection.DebugDisplayEnabledBridges(Color.white, 100f);

        for (int i = 0; i < _collection.Meshes.Length; i++)
        {
            var mesh = _collection.Meshes[i];

            mesh.DebugDraw(color, 100f);

            mesh.Connections.ForEach(x =>
            {
                
                for (int u = 0; u < x.Lines.Length; u++)
                {
                    this._smartMesh.Lines[x.Lines[u]].DebugDraw(color, 100f);
                }
            });
        }


        return this;
    }

    internal Graph<T> ApplyBlueprint(Action<Graph<T>> blueprint)
    {
        blueprint(this);

        return this;
    }

    internal Graph<T> ApplyBlueprintToSubMesh(Action<MeshCollection<T>> blueprint)
    {
        blueprint(_collection);

        return this;
    }

    internal Graph<T> ApplyBlueprintToNodeMetadata(Action<T[]> blueprint)
    {
        blueprint(_nodeMetadata);

        return this;
    }

    internal T[] Finally(Func<T, int[],T> finallyDo)
    {
        var outT = new T[_nodeMetadata.Length];

        for (int i = 0; i < _collection.Meshes.Length; i++)
        {
            var m = _collection.Meshes[i];
            var nodes = m.Nodes;
        
            for (int u = 0; u < nodes.Length; u++)
            {
                var index = nodes[u];
                var result = m.ConnectionsFromState(index, _identifier);
                outT[index] = finallyDo(_nodeMetadata[index], result);
            }
        }

        return outT;
    }
 }
