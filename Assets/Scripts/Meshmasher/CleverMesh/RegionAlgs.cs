using MeshMasher;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class States
{
    public static MeshState<int> DikstraWithRandomisation(SubMesh subMesh)
    {
        var dik = Dikstra(subMesh, 0.8f, 1.2f);

        return dik;
    }

    public static MeshState<int> OpenPlains(SubMesh subMesh)
    {
        var nodes = subMesh.Nodes;
        var lines = subMesh.Lines;
        var mesh = subMesh.ParentMesh.Mesh;

        var state = new MeshState<int>();
        state.Nodes = new int[nodes.Length];
        state.Lines = new int[lines.Length];
        for (int i = 0; i < nodes.Length; i++)
        {
            state.Nodes[i] = 1;
        }

        for (int i = 0; i < lines.Length; i++)
        {
            state.Lines[i] = 1;
        }

        return state;
    }

    public static MeshState<int> SummedDikstra(SubMesh subMesh)
    {
        var one = DikstraWithRandomisation(subMesh);
        var two = DikstraWithRandomisation(subMesh);

        for (int i = 0; i < subMesh.Lines.Length; i++)
        {
            one.Lines[i] = two.Lines[i] == 1 ? 1 : one.Lines[i];
        }

        return one;
    }

    public static MeshState<int> MinimalCorridor(SubMesh subMesh)
    {
        var keyMeshState = DikstraWithRandomisation(subMesh);
        //var anotherMeshState = DikstraWithRandomisation(subMesh);
        //
        //for (int i = 0; i < lines.Length; i++)
        //{
        //    keyMeshState.Lines[i] = anotherMeshState.Lines[i] == 1 ? 1 : keyMeshState.Lines[i];
        //}

        //Getting nodeMaps

        keyMeshState = RecursivelyRemoveDeadEnds(subMesh, keyMeshState);              


        return keyMeshState;
    }

    //Private Functions

    private static MeshState<int> Dikstra(SubMesh subMesh, float lineLengthMultiplierMin = 1f, float lineLengthMultiplierMax = 1f)
    {
        var nodes = subMesh.Nodes;
        var lines = subMesh.Lines;
        var mesh = subMesh.ParentMesh.Mesh;

        var lineLengthRandomiser = new float[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            lineLengthRandomiser[i] = RNG.NextFloat(lineLengthMultiplierMin, lineLengthMultiplierMin);
        }

        var isPartOfCurrentSortingEvent = new bool[nodes.Length];
        var visitedNodesList = new List<SmartNode>();
        var firstNode = mesh.Nodes[nodes[0]];
        var visitedNodes = new int[nodes.Length];
        var visitedLines = new int[lines.Length];
        var visitedLinesIteration = new int[lines.Length];
        visitedNodesList.Add(firstNode);
        visitedNodes[subMesh.NodeMap[firstNode.Index]] = 1;
        var outputLines = new List<SmartLine>();
        var iteration = 0;

        while (visitedNodesList.Count < nodes.Length)
        {
            outputLines.Clear();
            iteration++;

            for (int i = 0; i < visitedNodesList.Count; i++)
            {
                var n = visitedNodesList[i];

                for (int u = 0; u < n.Lines.Count; u++)
                {
                    if (!subMesh.LineMap.ContainsKey(n.Lines[u].Index))
                        continue;

                    var lineIndex = subMesh.LineMap[n.Lines[u].Index];

                    if (visitedLines[lineIndex] == 0 &&
                        visitedLinesIteration[lineIndex] != iteration)
                    {
                        outputLines.Add(n.Lines[u]);
                        visitedLinesIteration[lineIndex] = iteration;
                    }
                }
            }

            var length = float.MaxValue;
            SmartLine bestLine = null;

            for (int l = 0; l < outputLines.Count; l++)
            {
                var line = outputLines[l];
                var randomMultiplier = lineLengthRandomiser[subMesh.LineMap[line.Index]];

                if (line.Length * randomMultiplier > length)
                    continue;

                if (isPartOfCurrentSortingEvent[subMesh.NodeMap[line.Nodes[0].Index]] &&
                    isPartOfCurrentSortingEvent[subMesh.NodeMap[line.Nodes[1].Index]])
                    continue;

                length = line.Length * randomMultiplier;
                bestLine = line;
            }
            try
            {
                isPartOfCurrentSortingEvent[subMesh.NodeMap[bestLine.Nodes[0].Index]] = true;
            }
            catch
            {
                Debug.Log("Whey");
            }

            isPartOfCurrentSortingEvent[subMesh.NodeMap[bestLine.Nodes[1].Index]] = true;
            visitedLines[subMesh.LineMap[bestLine.Index]] = 1;

            for (int i = 0; i < bestLine.Nodes.Count; i++)
            {
                var n = bestLine.Nodes[i];
                if (visitedNodes[subMesh.NodeMap[n.Index]] == 0)
                {
                    visitedNodesList.Add(n);
                    visitedNodes[subMesh.NodeMap[n.Index]] = 1;
                }
            }
        }

        var meshState = new MeshState<int>();

        meshState.Nodes = new int[nodes.Length];

        for (int i = 0; i < meshState.Nodes.Length; i++)
        {
            meshState.Nodes[i] = 1;
        }

        meshState.Lines = visitedLines;

        return meshState;
    }

    private static MeshState<int> RecursivelyRemoveDeadEnds(SubMesh subMesh, MeshState<int> state, int iterations = 99)
    {
        var nodes = subMesh.Nodes;
        var mesh = subMesh.ParentMesh.Mesh;
        var keyMeshState = state.Clone();

        var newKeyStateLines = (int[])state.Lines.Clone();

        for (int v = 0; v < iterations; v++)
        {
            var deadEndCount = 0;

            for (int i = 0; i < nodes.Length; i++)
            {
                if (NodeAtIndexConnectsToOtherSubMesh(subMesh, i))
                    goto skip;

                var node = mesh.Nodes[nodes[i]];

                var connectionsCount = 0;
                var lastTrueLine = -1;
                var lastIndex = -1;

                for (int u = 0; u < node.Lines.Count; u++)
                {
                    if (!subMesh.LineMap.ContainsKey(node.Lines[u].Index))
                        continue;

                    var testIndex = subMesh.LineMap[node.Lines[u].Index];

                    if (keyMeshState.Lines[testIndex] == 1)
                    {
                        lastTrueLine = node.Lines[u].Index;
                        lastIndex = testIndex;
                        connectionsCount++;
                        //mesh.Lines[lastTrueLine].DebugDraw(Color.magenta, 3);
                    }
                }

                if (connectionsCount == 1)
                {
     
                    newKeyStateLines[lastIndex] = 0;
                    mesh.Lines[lastTrueLine].DebugDraw(Color.magenta, 5);
                    deadEndCount++;

                }

            skip:
                continue;
            }

            if (deadEndCount == 0)
            {
                //Debug.Log("Short cuircuted " + iterations + " down to " + v);
                goto finalise;
            }


            keyMeshState.Lines = newKeyStateLines;

            newKeyStateLines = (int[])keyMeshState.Lines.Clone();

        }

        finalise:

        return keyMeshState;


    }

    private static MeshState<int> Entubben(SubMesh subMesh, MeshState<int> state)
    {
        var nodes = subMesh.Nodes;
        var mesh = subMesh.ParentMesh.Mesh;

        var startState = state.Clone();
        var endState = state.Clone();

        for (int i = 0; i < startState.Nodes.Length; i++)
        {
            var node = mesh.Nodes[nodes[i]];

            if (startState.Nodes[i] != 0)
                continue;

            for (int u = 0; u < node.Lines.Count; u++)
            {
                if (!subMesh.LineMap.ContainsKey(node.Lines[u].Index))
                    continue;

                var testIndex = subMesh.LineMap[node.Lines[u].Index];




            }
        }


    }

    private static bool NodeAtIndexConnectsToOtherSubMesh(SubMesh subMesh,int index)
    {
                        for (int u = 0; u<subMesh.Connections.Count; u++)
                {
                    var bridge = subMesh.Connections[u];
    var sub = bridge.A == subMesh.Code ? bridge.NodesA : bridge.NodesB;

                    for (int o = 0; o<bridge.LineCodes.Length; o++)
                    {
                        if (bridge.LineCodes[o] == 0)
                            continue;
                if (sub[o] == index)
                    return true;
                    }
                }
        return false;
    }


}
