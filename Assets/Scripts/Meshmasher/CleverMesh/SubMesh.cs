using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MeshMasher;

class SubMesh {
    int[] Nodes;
    int[] Lines;
    MeshState<int> State;

    CleverMesh _mesh;

    //public List<int> ConnectingLines = new List<int>();

    public void ApplyState(System.Func<SmartMesh,int[],int[], MeshState<int>> func)
    {
        State = func(_mesh.Mesh,Nodes,Lines);
    }

    public SubMesh(int[] nodes, int[] lines, CleverMesh mesh)
    {
        _mesh = mesh;
        Nodes = nodes;
        Lines = lines;
    }

    public void DebugDraw(UnityEngine.Color color, float duration)
    {
        for (int i = 0; i < Lines.Length; i++)
        {
            _mesh.Mesh.Lines[Lines[i]].DebugDraw(color, duration);
        }
    }

    public static SubMesh[] FromMesh(CleverMesh mesh)
    {
        var subNodes = new Dictionary<int, List<int>>();

        var codes = mesh.NodeMetadata.Select(x => x.Code).Distinct().ToArray();

        for (int i = 0; i < mesh.NodeMetadata.Length; i++)
        {
            var nodeData = mesh.NodeMetadata[i].Code;
            if (subNodes.ContainsKey(nodeData))
            {
                subNodes[nodeData].Add(i);
            }
            else
            {
                subNodes.Add(nodeData, new List<int>() { i});
            }
        }

        var nodeArray = subNodes.ToArray();
        var subLines = new int[nodeArray.Length][];
        var finalSubmeshes = new SubMesh[nodeArray.Length];

        for (int i = 0; i < nodeArray.Length; i++)
        {
            var lines = new List<int>();
            var key = nodeArray[i].Key;
            var nodes = nodeArray[i].Value;

            for (int u = 0; u < nodes.Count; u++)
            {
                var node = nodes[u];

                lines.AddRange(mesh
                    .Mesh
                    .Nodes[node]
                    .Lines
                    .Where(x => mesh.NodeMetadata[x.Nodes[0].Index].Code == key && mesh.NodeMetadata[x.Nodes[1].Index].Code == key)
                    .Select(x => x.Index));
            }

            finalSubmeshes[i] = new SubMesh(nodes.ToArray(), lines.Distinct().ToArray(), mesh);
        }
        return finalSubmeshes;
    }
}
