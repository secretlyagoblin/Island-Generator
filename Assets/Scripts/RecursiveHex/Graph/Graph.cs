using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RecursiveHex;
using System;

public abstract class Graph<T> where T:struct, IGraphable
{
    protected MeshMasher.SmartMesh _smartMesh;
    protected MeshCollection<T> _collection;
    protected T[] _nodeMetadata;

    private Func<T, int> _identifier;

    public Graph(Vector3[] verts, int[] tris, T[] nodes, Func<T, int> identifier, Func<T,int[]> connector)
    {
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

        Color.RGBToHSV(color, out var h, out var s, out var v);
        var shiftedColor = Color.HSVToRGB(h+0.2f, s, v);


        for (int i = 0; i < _collection.Meshes.Length; i++)
        {
            var mesh = _collection.Meshes[i];

            mesh.DebugDraw(color, 100f);

            mesh.BridgeConnectionIndices.ForEach(x =>
            {
                var bridge = _collection.Bridges[x];                
                
                for (int u = 0; u < bridge.Lines.Length; u++)
                {
                    this._smartMesh.Lines[bridge.Lines[u]].DebugDraw(shiftedColor, 100f);
                }
            });
        }


        return this;
    }

    protected abstract void Generate();

    internal T[] Finalise(Func<T,Connection, int[],T> finallyDo)
    {
        Generate();

        var outT = new T[_nodeMetadata.Length];

        //Debug.LogError("The current error is in the way the nodes get propogated through connections from state - should really not have anything be set in here, instead rely on 'Generate' call in graph to do the work");

        for (int i = 0; i < _collection.Meshes.Length; i++)
        {
            var m = _collection.Meshes[i];
            var nodes = m.Nodes;
        
            for (int u = 0; u < nodes.Length; u++)
            {
                var index = nodes[u];
                var (status, neighbours) = m.ConnectionsFromState(index, _identifier);
                outT[index] = finallyDo(_nodeMetadata[index], status, neighbours);
            }
        }

        return outT;
    }
 }
