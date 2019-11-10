using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeshCollection<T>
{
    private MeshMasher.SmartMesh _smartMesh;
    public SubMesh<T>[] Meshes;
    public Bridge[] Bridges;

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

        Bridges = new Bridge[targetLines.connections.Count];

        for (int i = 0; i < targetLines.connections.Count; i++)
        {
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

            a.Connections.Add(Bridges[i]);
            b.Connections.Add(Bridges[i]);
        }
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