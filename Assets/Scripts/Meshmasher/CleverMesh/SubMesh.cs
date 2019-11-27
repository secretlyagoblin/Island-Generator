using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MeshMasher;
using UnityEngine;

public class SubMesh<T> where T:IGraphable {
    public int[] Nodes;
    public int[] Lines;

    public List<int> BridgeConnectionIndices = new List<int>();

    public readonly Dictionary<int, int> NodeMap;
    public readonly Dictionary<int, int> LineMap;
    public MeshState<Connection> Connectivity { get; private set; }
    public int Id { get; private set; }

    public readonly SmartMesh SourceMesh;
    public BridgeCollection SourceBridges { get; private set; }
    private readonly T[] _nodeMetadata;
    //List<KeyValuePair<int, int>> _connections = new List<KeyValuePair<int, int>>();

    //public List<int> ConnectingLines = new List<int>();



    public SubMesh(int code, int[] nodes, int[] lines, T[] nodeMetadata, SmartMesh mesh)
    {
        Id = code;
        SourceMesh = mesh;
        _nodeMetadata = nodeMetadata;
        Nodes = nodes;
        Lines = lines;

        var nodeConnectivity = new Connection[nodes.Length];

        NodeMap = new Dictionary<int, int>();
        for (int i = 0; i < nodes.Length; i++)
        {
            NodeMap.Add(nodes[i], i);
            nodeConnectivity[i] = nodeMetadata[nodes[i]].ConnectionStatus;
        }

        LineMap = new Dictionary<int, int>();
        for (int i = 0; i < lines.Length; i++)
        {
            LineMap.Add(lines[i], i);
        }

        Connectivity = new MeshState<Connection>
        {
            Nodes = nodeConnectivity,
            Lines = new Connection[lines.Length]
        };
    }

    public SubMesh(int code, int[] nodes, T[] payloads, SmartMesh mesh)
    {
        Id = code;
        SourceMesh = mesh;
        _nodeMetadata = payloads;
        Nodes = nodes;
        Lines = nodes
            .SelectMany(x => mesh.Nodes[x].Lines
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

    public void SetSharedBridgeCollection(BridgeCollection collection)
    {
        SourceBridges = collection;
    }

    public void SetConnectivity(System.Func<SubMesh<T>, MeshState<Connection>> func)
    {
        try
        {
            Connectivity = func(this);
        }
        catch (Exception ex)
        {
            throw new Exception("Error in Room Code " + Id, ex);
        }
    }

    public void DebugDraw(UnityEngine.Color color, float duration)
    {
        for (int i = 0; i < Lines.Length; i++)
        {
            if (Connectivity != null)
            {
                if (Connectivity.Lines[i] == Connection.NotPresent)
                    continue;
            }
            SourceMesh.Lines[Lines[i]].DebugDraw(color, duration);
        }
    }

    public (int[] lines, int[] nodesA, int[] nodesB) GetSharedLines(SubMesh<T> mesh, int[] candidates, Func<T,int> identifier)
    {
        var codeA = this.Id;
        var codeB = mesh.Id;
        var nodeMetadata = this._nodeMetadata;
        var smesh = this.SourceMesh;

        var lines = candidates.Where(x =>
        {
            var line = smesh.Lines[x];
            var a = identifier(nodeMetadata[line.Nodes[0].Index]);
            var b = identifier(nodeMetadata[line.Nodes[1].Index]);

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
            var lineId = lines[i];
            var line = smesh.Lines[lineId];
            var a = identifier(nodeMetadata[line.Nodes[0].Index]);
            var b = identifier(nodeMetadata[line.Nodes[1].Index]);

            if (a == codeA)
            {
                nodesA[i] = this.NodeMap[line.Nodes[0].Index];
                nodesB[i] = mesh.NodeMap[line.Nodes[1].Index];               
            }
            else
            {
                nodesA[i] = this.NodeMap[line.Nodes[1].Index];
                nodesB[i] = mesh.NodeMap[line.Nodes[0].Index];
            }
        }

        return (lines, nodesA, nodesB);

    }

    public static (List<KeyValuePair<int, int>> connections, List<int> lines) GetBridgePairs(SmartMesh mesh,T[] nodeMetadata, Func<T, int> identifier, Func<T,int[]> connector, SubMesh<T>[] meshes)
    {
        var lines = Enumerable.Range(0, mesh.Lines.Count);

        for (int i = 0; i < meshes.Length; i++)
        {
            lines = lines.Except(meshes[i].Lines);
        }

        var finalLines = lines.Select(x => mesh.Lines[x]);

        var connections = new List<KeyValuePair<int, int>>();

        for (int i = 0; i < meshes.Length; i++)
        {
            var m = meshes[i];
            
            var subConnections = connector(nodeMetadata[m.Nodes[0]]);

            for (int u = 0; u < subConnections.Length; u++)
            {
                var a = m.Id < subConnections[u] ?m.Id: subConnections[u];
                var b = m.Id < subConnections[u] ?subConnections[u] : m.Id;

                connections.Add(new KeyValuePair<int, int>(a, b));
            }                  
        }

        connections = connections.Distinct().ToList();

        var determinedLines = new List<SmartLine>();

        //finalLines = finalLines.Where(x =>
        //{
        //    if (determinedLines.SkipWhile(y => !y.IsConnectedTo(x)).Count() > 0)
        //        return false;
        //
        //    var a = x.Nodes[0].Index;
        //    var b = x.Nodes[1].Index;
        //
        //    var codeA = identifier(nodeMetadata[a]) < identifier(nodeMetadata[b]) ? identifier(nodeMetadata[a]) : identifier(nodeMetadata[b]);
        //    var codeB = identifier(nodeMetadata[a]) < identifier(nodeMetadata[b]) ? identifier(nodeMetadata[b]) : identifier(nodeMetadata[a]);
        //
        //    var pair = new KeyValuePair<int, int>(codeA, codeB);
        //
        //    if (!connections.Contains(pair))
        //        return false;
        //
        //    //connections.Remove(pair);
        //    determinedLines.Add(x);
        //
        //    return true;
        //});

        return (connections, finalLines.Select(x => x.Index).ToList());
    }

    /// <summary>
    /// Iterate through each node and record its connections to other nodes
    /// </summary>
    /// <param name="nodeIndex"></param>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public (Connection status, int[] neighbours) ConnectionsFromState(int nodeIndex, Func<T, int> identifier)
    {
        if(Connectivity == null)
        {
            throw new Exception("Submesh Connectivity has not been set and therefore connection info can't be found. Check your level generator to make sure it is setting it correctly.");
        }

        var node = SourceMesh.Nodes[nodeIndex];


        var connections = new List<int>(node.Lines.Count);

        var bridgeSubset = this.BridgeConnectionIndices.SelectMany(x => this.SourceBridges[x].Lines).Distinct().ToHashSet();

        var connection = this.Connectivity.Nodes[this.NodeMap[nodeIndex]];

        for (int i = 0; i < node.Lines.Count; i++)
        {
            var isInLineMap = LineMap.ContainsKey(node.Lines[i].Index);
            var isInBridges = bridgeSubset.Contains(node.Lines[i].Index);

            if (isInLineMap)
            {
                if (Connectivity.Lines[LineMap[node.Lines[i].Index]] != Connection.NotPresent)
                {
                    connections.Add(
                        identifier(
                            this._nodeMetadata[node.Lines[i].GetOtherNode(node).Index])
                        );

                   
                }
            }
            else if (isInBridges)
            {
                connections.Add(
                    identifier(
                        this._nodeMetadata[node.Lines[i].GetOtherNode(node).Index])
                    );

                connection = Connection.Critical;
            }
            else
            {
                continue;
            }
        }

        return (connection, connections.ToArray());
    }

    public static SubMesh<T>[] FromMesh(SmartMesh mesh, T[] payloads, Func<T, int> identifier)
    {
        var subNodes = new Dictionary<int, List<int>>();
        

        //var codes = mesh.NodeMetadata.Select(x => x.Code).Where(x => x!= 0).Distinct().ToArray();     

        for (int i = 0; i < payloads.Length; i++)
        {
            var nodeData = identifier(payloads[i]);

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
        var finalSubmeshes = new SubMesh<T>[nodeArray.Length];

        for (int i = 0; i < nodeArray.Length; i++)
        {
            var lines = new List<int>();
            var key = nodeArray[i].Key;
            var nodes = nodeArray[i].Value;

            for (int u = 0; u < nodes.Count; u++)
            {
                var node = nodes[u];

                lines.AddRange(mesh
                    .Nodes[node]
                    .Lines
                    .Where(x => identifier(payloads[x.Nodes[0].Index]) == key && identifier(payloads[x.Nodes[1].Index]) == key)
                    .Select(x => x.Index));
            }

            

            finalSubmeshes[i] = new SubMesh<T>(key, nodes.ToArray(), lines.Distinct().ToArray(),payloads, mesh);

        }
        return finalSubmeshes;
    }

}

public enum Connection
{
   NotPresent = 0, Present = 1, Critical = 2
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
