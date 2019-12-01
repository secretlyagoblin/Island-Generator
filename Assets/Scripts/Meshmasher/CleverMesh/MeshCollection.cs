using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeshCollection<T> where T:IGraphable
{
    private MeshMasher.SmartMesh _smartMesh;
    public SubMesh<T>[] Meshes;
    public BridgeCollection Bridges;

    public MeshCollection(MeshMasher.SmartMesh parentLayer,T[] nodeMetadata, Func<T,int> defaultIdentifier, Func<T,int[]> defaultConnector)
    {
        _smartMesh = parentLayer;
        Meshes = SubMesh<T>.FromMesh(parentLayer, nodeMetadata, defaultIdentifier);

        var meshDict = new Dictionary<int, SubMesh<T>>();

        for (int i = 0; i < Meshes.Length; i++)
        {
            meshDict.Add(Meshes[i].Id, Meshes[i]);
        }

        var targetLines = SubMesh<T>.GetBridgePairs(parentLayer,nodeMetadata,defaultIdentifier,defaultConnector, Meshes);

        Bridges = new BridgeCollection(targetLines.connections.Count);

        for (int i = 0; i < Meshes.Length; i++)
        {
            Meshes[i].SetSharedBridgeCollection(Bridges);
        }

        for (int i = 0; i < targetLines.connections.Count; i++)
        {
            if(!(meshDict.ContainsKey(targetLines.connections[i].Key) && meshDict.ContainsKey(targetLines.connections[i].Value)))
            {
                Bridges[i] = new Bridge(
                      targetLines.connections[i].Key,
                      targetLines.connections[i].Value,
                      new int[0],
                      new int[0],
                      new int[0]
                  );
                continue;
            }

            var a = meshDict[targetLines.connections[i].Key];
            var b = meshDict[targetLines.connections[i].Value];
            var (lines, nodesA, nodesB) = a.GetSharedLines(b, targetLines.lines.ToArray(),defaultIdentifier);

            Bridges[i] = new Bridge(            
                targetLines.connections[i].Key,
                targetLines.connections[i].Value,
                nodesA,
                nodesB,
                lines
            );

            a.BridgeConnectionIndices.Add(i);
            b.BridgeConnectionIndices.Add(i);
        }
    }

    public int[][] GetConnectionMetadata()
    {
        var lineMap = new bool[_smartMesh.Lines.Count];

        for (int i = 0; i < Bridges.Length; i++)
        {
            var bridge = Bridges[i];
        
            for (int u = 0; u < bridge.Lines.Length; u++)
            {
                var lineId = bridge.Lines[u];
                if(bridge.LineCodes[u]!=0)
                    lineMap[lineId] = true;
            }            
        }

        for (int i = 0; i < Meshes.Length; i++)
        {
            var subMesh = Meshes[i];
            var state = subMesh.Connectivity;

            for (int u = 0; u < subMesh.Lines.Length; u++)
            {
                var lineId = subMesh.Lines[u];

                    if (state.Lines[u] != 0)
                        lineMap[lineId] = true;
            }
        }

        //for (int i = 0; i < lineMap.Length; i++)
        //{
        //    if (lineMap[i])
        //        _cleverMesh.Mesh.Lines[i].DebugDraw(Color.green, 100f);
        //}

        return _smartMesh.Nodes.Select(x => x.Lines.Where(y => lineMap[y.Index]).Select(y => y.GetOtherNode(x).Index).ToArray()).ToArray();
    }

    public void DebugDisplayEnabledBridges(Color color, float duration)
    {
        for (int i = 0; i < Bridges.Length; i++)
        {
            var b = Bridges[i];

            for (int u = 0; u < b.Lines.Length; u++)
            {
                if (b.LineCodes[u] == 0)
                    continue;

                _smartMesh.Lines[b.Lines[u]].DebugDraw(color, duration);
            }
        }
    }

    public void MarkBridgeInterfacesAsCritical()
    {
        Debug.Log("Bridge interfaces should be populating criticality - currently not");

        for (int i = 0; i < Meshes.Length; i++)
        {
            var mesh = Meshes[i];

            foreach (var bridgeIndex in mesh.BridgeConnectionIndices)
            {
                var bridge = Bridges[bridgeIndex];
                var nodes = bridge.A == mesh.Id ? bridge.NodesA : bridge.NodesB;

                for (int u = 0; u < nodes.Length; u++)
                {
                    mesh.Connectivity.Nodes[bridge.NodesA[u]] = Connection.Critical;
                }
            }
        }
    }

}

public class BridgeCollection
{
    Bridge[] _list;// = new List<Bridge>();

    public int Length { get { return _list.Length; } }

    public BridgeCollection(int length)
    {
        _list = new Bridge[length];
    }

    public Bridge this[int index]
    {
        get { return _list[index]; }
        set { _list[index] = value; }
    }

    public void LeaveSingleRandomConnection()
    {
        for (int i = 0; i < _list.Length; i++)
        {
            var a = _list[i];
            var empty = a.NodesA.Length == 0;

            var random = RNG.Next(a.Lines.Length);

            var b = new Bridge(
                a.A,
                a.B,
                empty ? new int[0]: new int[] { a.NodesA[random] },
                empty ? new int[0]: new int[] { a.NodesB[random] },
                empty ? new int[0]: new int[] { a.Lines[random] }
                );
            _list[i] = b;
        }
    }
}

public class Bridge
{
    public Bridge(int a, int b, int[] nodesA, int[] nodesB, int[] lines)
    {
        A = a;
        B = b;
        NodesA = nodesA;
        NodesB = nodesB;
        Lines = lines;
        LineCodes = new int[Lines.Length];
    }

    public int A;
    public int B;
    public int[] NodesA;
    public int[] NodesB;
    public int[] Lines;
    public int[] LineCodes;
}