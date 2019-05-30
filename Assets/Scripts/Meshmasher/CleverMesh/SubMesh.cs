using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MeshMasher;

public class SubMesh {
    public int[] Nodes;
    int[] Lines;
    MeshState<int> State;
    public int Code { get; private set; }

    CleverMesh _mesh;
    //List<KeyValuePair<int, int>> _connections = new List<KeyValuePair<int, int>>();

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

    public SubMesh(int code, int[] nodes, CleverMesh mesh)
    {
        Code = code;
        _mesh = mesh;
        Nodes = nodes;
        Lines = nodes
            .SelectMany(x => mesh.Mesh.Nodes[x].Lines
                .Where(y => nodes.Contains(y.Nodes[0].Index) && nodes.Contains(y.Nodes[1].Index))
                .Select(y => y.Index))
            .Distinct()
            .ToArray();
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

    public int[] GetSharedLines(SubMesh mesh, int[] candidates)
    {
        var codeA = this.Code;
        var codeB = mesh.Code;
        var metadata = this._mesh.NodeMetadata;
        var smesh = this._mesh.Mesh;

        return candidates.Where(x =>
        {
            var line = smesh.Lines[x];
            var a = metadata[line.Nodes[0].Index].Code;
            var b = metadata[line.Nodes[1].Index].Code;

            if (a == codeA && b == codeB)
                return true;

            if (b == codeA && a == codeB)
                return true;

            return false;
        }).ToArray();
    }

    public static (List<KeyValuePair<int, int>> connections, List<int> lines) GetConnectonData(CleverMesh mesh, SubMesh[] meshes)
    {
        var lines = Enumerable.Range(0, mesh.Mesh.Lines.Count);

        for (int i = 0; i < meshes.Length; i++)
        {
            lines = lines.Except(meshes[i].Lines);
        }

        var finalLines = lines.Select(x => mesh.Mesh.Lines[x]);

        var connections = new List<KeyValuePair<int, int>>();

        for (int i = 0; i < meshes.Length; i++)
        {
            var m = meshes[i];
            
            var subConnections = mesh.NodeMetadata[m.Nodes[0]].Connections;

            for (int u = 0; u < subConnections.Length; u++)
            {
                var a = m.Code < subConnections[u] ?m.Code: subConnections[u];
                var b = m.Code < subConnections[u] ?subConnections[u] : m.Code;

                connections.Add(new KeyValuePair<int, int>(a, b));
            }                  
        }

        connections = connections.Distinct().ToList();

        var determinedLines = new List<SmartLine>();

        finalLines = finalLines.Where(x =>
        {
            if (determinedLines.SkipWhile(y => !y.IsConnectedTo(x)).Count() > 0)
                return false;

            var a = x.Nodes[0].Index;
            var b = x.Nodes[1].Index;

            var codeA = mesh.NodeMetadata[a].Code < mesh.NodeMetadata[b].Code ? mesh.NodeMetadata[a].Code : mesh.NodeMetadata[b].Code;
            var codeB = mesh.NodeMetadata[a].Code < mesh.NodeMetadata[b].Code ? mesh.NodeMetadata[b].Code : mesh.NodeMetadata[a].Code;

            var pair = new KeyValuePair<int, int>(codeA, codeB);

            if (!connections.Contains(pair))
                return false;

            //connections.Remove(pair);
            determinedLines.Add(x);

            return true;
        });

        return (connections, finalLines.Select(x => x.Index).ToList());
    }

    public int[] ConnectionsFromState(int nodeIndex)
    {
        for (int i = 0; i < Nodes.Length; i++)
        {
            if (nodeIndex == Nodes[i])
                goto FoundNode;
        }

        return new int[] { };

        FoundNode:

        var node = _mesh.Mesh.Nodes[nodeIndex];

        var lineMap = new Dictionary<int, int>();
        for (int i = 0; i < Lines.Length; i++)
        {
            lineMap.Add(Lines[i], i);
        }

        var connections = new List<int>(node.Lines.Count);

        for (int i = 0; i < node.Lines.Count; i++)
        {
            if(!lineMap.ContainsKey(node.Lines[i].Index))
                continue;

            if(State.Lines[lineMap[node.Lines[i].Index]] != 0)
            {
                connections.Add(_mesh.NodeMetadata[node.Lines[i].GetOtherNode(node).Index].Code);
            }
        }

        return connections.ToArray();
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
                subNodes.Add(nodeData, new List<int>() { i });
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

//class TopologyData {
//
//}

//class GameSpace {
//    public SubMesh[] Neighbourhood;
//    public TopologyData Topology;
//    public CleverMesh Mesh;
//
//    public GameSpace(CleverMesh mesh)
//    {
//        Neighbourhood = FromMesh(mesh);
//        Mesh = mesh;
//    }
//
//    public GameSpace(GameSpace space)
//    {
//
//
//        Mesh = new CleverMesh(space.Mesh, Neighbourhood.SelectMany(x => x.Nodes).ToArray());
//
//    }
//
//    public void ApplyState(Func<SmartMesh, int[], int[], MeshState<int>>[] stateArray)
//    {
//        for (int i = 0; i < Neighbourhood.Length; i++)
//        {
//            Neighbourhood[i].ApplyState(RNG.GetRandomItem(stateArray));
//        }
//    }
//
   
//}
