using MeshMasher;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelGen
{    
    public static class States
    {
        public static MeshState<Connection> DikstraWithRandomisation<T>(SubMesh<T> subMesh)
        {
            var dik = Dikstra(subMesh, 0.8f, 1.2f);

            return dik;
        }

        public static MeshState<Connection> OpenPlains<T>(SubMesh<T> subMesh)
        {
            var nodes = subMesh.Nodes;
            var lines = subMesh.Lines;

            var state = new MeshState<Connection>();
            state.Nodes = new Connection[nodes.Length];
            state.Lines = new Connection[lines.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                state.Nodes[i] = Connection.Present;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                state.Lines[i] = Connection.Present;
            }

            return state;
        }

        public static MeshState<Connection> SummedDikstra<T>(SubMesh<T> subMesh)
        {
            var one = DikstraWithRandomisation(subMesh);
            var two = DikstraWithRandomisation(subMesh);

            for (int i = 0; i < subMesh.Lines.Length; i++)
            {
                one.Lines[i] = two.Lines[i] == Connection.Present ? Connection.Present : one.Lines[i];
            }

            return one;
        }

        public static MeshState<Connection> SummedDikstraRemoveDeadEnds<T>(SubMesh<T> subMesh)
        {
            var one = SummedDikstra(subMesh);

            var cleaned = RecursivelyRemoveDeadEnds(subMesh, one);
            //var finalided = RemoveShortCliffs(subMesh, cleaned);

            return cleaned;
        }

        public static MeshState<Connection> MinimalCorridor<T>(SubMesh<T> subMesh)
        {
            var keyMeshState = DikstraWithRandomisation(subMesh);
            keyMeshState = RecursivelyRemoveDeadEnds(subMesh, keyMeshState);

            return keyMeshState;
        }

        public static MeshState<Connection> TubbyCorridors<T>(SubMesh<T> subMesh)
        {
            var keyMeshState = DikstraWithRandomisation(subMesh);
            keyMeshState = RecursivelyRemoveDeadEnds(subMesh, keyMeshState);

            return Entubben(subMesh, keyMeshState, 3);
        }

        public static MeshState<Connection> DikstraWithRooms<T>(SubMesh<T> subMesh)
        {
            var dik = Dikstra(subMesh, 0.8f, 1.2f);
            var next = RecursivelyRemoveDeadEnds(subMesh, dik, 2);
            return Entubben(subMesh, next, 3);
        }

        public static MeshState<Connection> ConnectEverything<T>(SubMesh<T> subMesh)
        {
            return new MeshState<Connection>()
            {
                Nodes = Populate(subMesh.Nodes.Length, Connection.Present),
                Lines = Populate(subMesh.Lines.Length, Connection.Present)  
            };
        }


        //Private Functions

        private static MeshState<Connection> Dikstra<T>(SubMesh<T> subMesh, float lineLengthMultiplierMin = 1f, float lineLengthMultiplierMax = 1f)
        {
            var nodes = subMesh.Nodes;
            var lines = subMesh.Lines;
            var mesh = subMesh.SourceMesh;

            var lineLengthRandomiser = new float[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                lineLengthRandomiser[i] = RNG.NextFloat(lineLengthMultiplierMin, lineLengthMultiplierMax);
            }

            var isPartOfCurrentSortingEvent = new bool[nodes.Length];
            var visitedNodesList = new List<SmartNode>();
            var firstNode = mesh.Nodes[nodes[0]];
            var visitedNodes = new Connection[nodes.Length];
            var visitedLines = new Connection[lines.Length];
            var visitedLinesIteration = new int[lines.Length];
            visitedNodesList.Add(firstNode);
            visitedNodes[subMesh.NodeMap[firstNode.Index]] = Connection.Present;
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

                        if (visitedLines[lineIndex] == Connection.NotPresent &&
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
                visitedLines[subMesh.LineMap[bestLine.Index]] = Connection.Present;

                for (int i = 0; i < bestLine.Nodes.Count; i++)
                {
                    var n = bestLine.Nodes[i];
                    if (visitedNodes[subMesh.NodeMap[n.Index]] == Connection.NotPresent)
                    {
                        visitedNodesList.Add(n);
                        visitedNodes[subMesh.NodeMap[n.Index]] = Connection.Present;
                    }
                }
            }

            var meshState = new MeshState<Connection>
            {
                Nodes = new Connection[nodes.Length]
            };

            for (int i = 0; i < meshState.Nodes.Length; i++)
            {
                meshState.Nodes[i] = Connection.Present;
            }

            meshState.Lines = visitedLines;

            return meshState;
        }

        private static MeshState<Connection> RecursivelyRemoveDeadEnds<T>(SubMesh<T> subMesh, MeshState<Connection> state, int iterations = 99)
        {
            var nodes = subMesh.Nodes;
            var mesh = subMesh.SourceMesh;
            var startState = state.Clone();
            var endState = state.Clone();

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

                        if (startState.Lines[testIndex] == Connection.Present)
                        {
                            lastTrueLine = node.Lines[u].Index;
                            lastIndex = testIndex;
                            connectionsCount++;
                            //mesh.Lines[lastTrueLine].DebugDraw(Color.magenta, 3);
                        }
                    }

                    if (connectionsCount == 1)
                    {

                        endState.Nodes[i] = Connection.NotPresent;
                        endState.Lines[lastIndex] = Connection.NotPresent;
                        //mesh.Lines[lastTrueLine].DebugDraw(Color.magenta, 5);
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


                startState = endState;
                endState = startState.Clone();

            }

        finalise:

            return startState;


        }

        private static MeshState<Connection> Entubben<T>(SubMesh<T> subMesh, MeshState<Connection> state, int nodeConnectionsThreshold)
        {
            var nodes = subMesh.Nodes;
            var mesh = subMesh.SourceMesh;

            var startState = state.Clone();
            var endState = state.Clone();

            var tempLineStorage = new List<int>(10);

            for (int i = 0; i < startState.Nodes.Length; i++)
            {
                var node = mesh.Nodes[nodes[i]];

                if (startState.Nodes[i] == Connection.Present)
                    continue;

                var embigginCount = 0;
                tempLineStorage.Clear();

                for (int u = 0; u < node.Lines.Count; u++)
                {
                    if (!subMesh.LineMap.ContainsKey(node.Lines[u].Index))
                        continue;

                    var other = node.Lines[u].GetOtherNode(node);

                    if (!subMesh.NodeMap.ContainsKey(other.Index))
                        continue;

                    var testIndex = subMesh.NodeMap[other.Index];

                    if (startState.Nodes[testIndex] == Connection.NotPresent)
                        continue;

                    tempLineStorage.Add(subMesh.LineMap[node.Lines[u].Index]);

                    embigginCount++;
                }

                if (embigginCount >= nodeConnectionsThreshold)
                {
                    endState.Nodes[i] = Connection.Present;
                    tempLineStorage.ForEach(x => endState.Lines[x] = Connection.Present);
                }

            }

            return endState;

        }

        //  Not Working as intended...
        //
        //private static MeshState<int> RemoveShortCliffs(SubMesh<NodeMetadata> subMesh, MeshState<int> state)
        //{
        //    var lines = subMesh.Lines;
        //    var mesh = subMesh.ParentMesh.Mesh;
        //
        //    var startState = state.Clone();
        //    var endState = state.Clone();
        //
        //    for (int i = 0; i < startState.Lines.Length; i++)
        //    {
        //        if (startState.Lines[i] != 0)
        //            continue;
        //
        //        var line = mesh.Lines[lines[i]];
        //
        //        var connections = line.CollectConnectedLines();
        //
        //        for (int u = 0; u < connections.Count; u++)
        //        {
        //            if (!subMesh.LineMap.ContainsKey(connections[u].Index))
        //                continue;
        //
        //            var testIndex = subMesh.LineMap[connections[u].Index];
        //
        //            if(startState.Lines[testIndex] == 0)
        //            {
        //                goto InConnectedSet;
        //            }
        //        }
        //
        //        endState.Lines[i] = 1;
        //
        //        line.DebugDraw(Color.red, 70f);
        //
        //    InConnectedSet:
        //        continue;
        //    }
        //
        //    return endState;
        //}

        private static bool NodeAtIndexConnectsToOtherSubMesh<T>(SubMesh<T> subMesh, int index)
        {
            for (int u = 0; u < subMesh.BridgeConnectionIndices.Count; u++)
            {

                var bridge = subMesh.SourceBridges[subMesh.BridgeConnectionIndices[u]];
                var sub = bridge.A == subMesh.Id ? bridge.NodesA : bridge.NodesB;

                for (int o = 0; o < bridge.LineCodes.Length; o++)
                {
                    if (bridge.LineCodes[o] == 0)
                        continue;
                    if (sub[o] == index)
                        return true;
                }
            }
            return false;
        }

        private static T[] Populate<T>(int length, T value)
        {
            var array = new T[length];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }

            return array;
        }


    }


}