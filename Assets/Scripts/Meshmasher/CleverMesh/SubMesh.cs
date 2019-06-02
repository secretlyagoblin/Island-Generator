using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MeshMasher;
using UnityEngine;

public class SubMesh {
    public int[] Nodes;
    public int[] Lines;
    public List<int> ConnectionNodes = new List<int>();

    public readonly Dictionary<int, int> NodeMap;
    public readonly Dictionary<int, int> LineMap;
    public MeshState<int> State { get; private set; }
    public int Code { get; private set; }

    public CleverMesh ParentMesh;
    //List<KeyValuePair<int, int>> _connections = new List<KeyValuePair<int, int>>();

    //public List<int> ConnectingLines = new List<int>();

    public void ApplyState(System.Func<SubMesh, MeshState<int>> func)
    {
        try
        {
            State = func(this);
        }
        catch(Exception ex)
        {
            throw new Exception("Error in Room Code " + Code, ex);
        }
    }

    public SubMesh(int code, int[] nodes, int[] lines, CleverMesh mesh)
    {
        Code = code;
        ParentMesh = mesh;
        Nodes = nodes;
        Lines = lines;

        NodeMap = new Dictionary<int, int>();
        for (int i = 0; i < nodes.Length; i++)
        {
            NodeMap.Add(nodes[i], i);
        }

        LineMap = new Dictionary<int, int>();
        for (int i = 0; i < lines.Length; i++)
        {
            LineMap.Add(lines[i], i);
        }

    }

    public SubMesh(int code, int[] nodes, CleverMesh mesh)
    {
        Code = code;
        ParentMesh = mesh;
        Nodes = nodes;
        Lines = nodes
            .SelectMany(x => mesh.Mesh.Nodes[x].Lines
                .Where(y => nodes.Contains(y.Nodes[0].Index) && nodes.Contains(y.Nodes[1].Index))
                .Select(y => y.Index))
            .Distinct()
            .ToArray();

        NodeMap = new Dictionary<int, int>();
        for (int i = 0; i < nodes.Length; i++)
        {
            NodeMap.Add(nodes[i], i);
        }

        LineMap = new Dictionary<int, int>();
        for (int i = 0; i < Lines.Length; i++)
        {
            LineMap.Add(Lines[i], i);
        }
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
            ParentMesh.Mesh.Lines[Lines[i]].DebugDraw(color, duration);
        }
    }

    public (int[] lines, int[] nodesA, int[] nodesB) GetSharedLines(SubMesh mesh, int[] candidates)
    {
        var codeA = this.Code;
        var codeB = mesh.Code;
        var metadata = this.ParentMesh.NodeMetadata;
        var smesh = this.ParentMesh.Mesh;

        var lines = candidates.Where(x =>
        {
            var line = smesh.Lines[x];
            var a = metadata[line.Nodes[0].Index].RoomCode;
            var b = metadata[line.Nodes[1].Index].RoomCode;

            if (a == codeA && b == codeB)
                return true;

            if (b == codeA && a == codeB)
                return true;

            return false;
        }).ToArray();

        var nodesA = new int[lines.Length];
        var nodesB = new int[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            var line = smesh.Lines[i];
            var a = metadata[line.Nodes[0].Index].RoomCode;
            var b = metadata[line.Nodes[1].Index].RoomCode;

            if(a == codeA)
            {
                nodesA[i] = this.NodeMap[line.Nodes[0].Index];
                try
                {
                nodesB[i] = mesh.NodeMap[line.Nodes[1].Index];
               
                }
                catch
                {
                    Debug.Log("What in the living fuck is going on");
                }
               
            }
            else
            {
                nodesA[i] = this.NodeMap[line.Nodes[1].Index];
                nodesB[i] = mesh.NodeMap[line.Nodes[0].Index];
            }
        }

        return (lines, nodesA, nodesB);

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
            
            var subConnections = mesh.NodeMetadata[m.Nodes[0]].RoomConnections;

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

            var codeA = mesh.NodeMetadata[a].RoomCode < mesh.NodeMetadata[b].RoomCode ? mesh.NodeMetadata[a].RoomCode : mesh.NodeMetadata[b].RoomCode;
            var codeB = mesh.NodeMetadata[a].RoomCode < mesh.NodeMetadata[b].RoomCode ? mesh.NodeMetadata[b].RoomCode : mesh.NodeMetadata[a].RoomCode;

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

        var node = ParentMesh.Mesh.Nodes[nodeIndex];

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
                connections.Add(ParentMesh.NodeMetadata[node.Lines[i].GetOtherNode(node).Index].RoomCode);
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
            var nodeData = mesh.NodeMetadata[i].RoomCode;

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
                    .Where(x => mesh.NodeMetadata[x.Nodes[0].Index].RoomCode == key && mesh.NodeMetadata[x.Nodes[1].Index].RoomCode == key)
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
