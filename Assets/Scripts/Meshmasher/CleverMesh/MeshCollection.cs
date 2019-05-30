using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCollection
{
    private CleverMesh _cleverMesh;
    public SubMesh[] Meshes;
    public Bridge[] Bridges;

    public MeshCollection(CleverMesh parentLayer)
    {
        _cleverMesh = parentLayer;
        Meshes = SubMesh.FromMesh(parentLayer);

        var meshDict = new Dictionary<int, SubMesh>();

        for (int i = 0; i < Meshes.Length; i++)
        {
            meshDict.Add(Meshes[i].Code, Meshes[i]);
        }

        var targetLines = SubMesh.GetConnectonData(parentLayer, Meshes);

        Bridges = new Bridge[targetLines.connections.Count];

        for (int i = 0; i < targetLines.connections.Count; i++)
        {
            var a = meshDict[targetLines.connections[i].Key];
            var b = meshDict[targetLines.connections[i].Value];

            Bridges[i] = new Bridge(            
                targetLines.connections[i].Key,
                targetLines.connections[i].Value,
                a.GetSharedLines(b, targetLines.lines.ToArray())
            );
        }
    }

    public void DebugDisplayEnabledBridges(Color color, float duration)
    {
        for (int i = 0; i < Bridges.Length; i++)
        {
            var b = Bridges[i];

            for (int u = 0; u < b.Lines.Length; u++)
            {
                if (b.LineCode[u] == 0)
                    continue;

                _cleverMesh.Mesh.Lines[b.Lines[u]].DebugDraw(color, duration);
            }
        }
    }


}

public class Bridge
{
    public Bridge(int a, int b, int[] lines)
    {
        A = a;
        B = b;
        Lines = lines;
        LineCode = new int[Lines.Length];
    }

    public int A;
    public int B;
    public int[] Lines;
    public int[] LineCode;
}