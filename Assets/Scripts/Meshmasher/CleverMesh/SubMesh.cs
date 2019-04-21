using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MeshMasher;

class SubMesh {
    int[] Nodes;
    int[] Lines;
    MeshState<int> State;
    public int Code { get; private set; }

    CleverMesh _mesh;

    //public List<int> ConnectingLines = new List<int>();

    public void ApplyState(System.Func<SmartMesh,int[],int[], MeshState<int>> func)
    {
        try
        {
            State = func(_mesh.Mesh, Nodes, Lines);
        }
        catch(Exception ex)
        {
            throw new Exception("Error in Room Code " + Code, ex);
        }
    }

    public SubMesh(int code, int[] nodes, int[] lines, CleverMesh mesh)
    {
        Code = code;
        _mesh = mesh;
        Nodes = nodes;
        Lines = lines;
    }

    public void DebugDraw(UnityEngine.Color color, float duration)
    {
        for (int i = 0; i < Lines.Length; i++)
        {
            if (State != null)
            {
                if (State.Lines[i] == 0)
                    continue;
            }
            _mesh.Mesh.Lines[Lines[i]].DebugDraw(color, duration);
        }
    }

    public static SubMesh[] FromMesh(CleverMesh mesh)
    {
        var subNodes = new Dictionary<int, List<int>>();

        //var codes = mesh.NodeMetadata.Select(x => x.Code).Where(x => x!= 0).Distinct().ToArray();     

        for (int i = 0; i < mesh.NodeMetadata.Length; i++)
        {
            var nodeData = mesh.NodeMetadata[i].Code;

            if (nodeData == 0)
                continue;

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

            finalSubmeshes[i] = new SubMesh(key, nodes.ToArray(), lines.Distinct().ToArray(), mesh);
        }
        return finalSubmeshes;
    }
}
