using GraphTransformers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderingRoad.Core.Random;
using WanderingRoad.Procgen.Meshes;
using WanderingRoad.Procgen.Topology;
using System.Linq;
using System;

namespace WanderingRoad.Procgen.Levelgen
{    
    public static class Connectivity
    {
        public static MeshState<Connection> DikstraWithRandomisation<T>(SubMesh<T> subMesh) where T:IGraphable
        {
            var dik = Dikstra(subMesh, 0.3f, 1.7f);

            return dik;
        }

        public static MeshState<Connection> OpenPlains<T>(SubMesh<T> subMesh) where T : IGraphable
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

        public static MeshState<Connection> SummedDikstra<T>(SubMesh<T> subMesh) where T : IGraphable
        {
            var one = DikstraWithRandomisation(subMesh);
            var two = DikstraWithRandomisation(subMesh);

            for (int i = 0; i < subMesh.Lines.Length; i++)
            {
                one.Lines[i] = two.Lines[i] == Connection.Present ? Connection.Present : one.Lines[i];
            }

            return one;
        }

        public static MeshState<Connection> SummedDikstraRemoveDeadEnds<T>(SubMesh<T> subMesh) where T : IGraphable
        {
            var one = SummedDikstra(subMesh);

            var cleaned = RecursivelyRemoveDeadEnds(subMesh, one);
            //var finalided = RemoveShortCliffs(subMesh, cleaned);

            return cleaned;
        }

        public static MeshState<Connection> MinimalCorridor<T>(SubMesh<T> subMesh) where T : IGraphable
        {
            var keyMeshState = DikstraWithRandomisation(subMesh);
            keyMeshState = RecursivelyRemoveDeadEnds(subMesh, keyMeshState);

            return keyMeshState;
        }

        public static MeshState<Connection> TubbyCorridors<T>(SubMesh<T> subMesh) where T : IGraphable
        {
            var keyMeshState = DikstraWithRandomisation(subMesh);
            keyMeshState = RecursivelyRemoveDeadEnds(subMesh, keyMeshState);

            return Entubben(subMesh, keyMeshState, 3);
        }

        public static MeshState<Connection> DikstraWithRooms<T>(SubMesh<T> subMesh) where T : IGraphable
        {
            var dik = Dikstra(subMesh, 0.8f, 1.2f);
            var next = RecursivelyRemoveDeadEnds(subMesh, dik, 2);
            return Entubben(subMesh, next, 3);
        }

        public static MeshState<Connection> ConnectEverything<T>(SubMesh<T> subMesh) where T : IGraphable
        {
            var outNodes = new Connection[subMesh.Connectivity.Nodes.Length];

            for (int i = 0; i < outNodes.Length; i++)
            {
                outNodes[i] = subMesh.Connectivity.Nodes[i] == Connection.Critical ? Connection.Critical : Connection.Present;
            }


            return new MeshState<Connection>()
            {
                Nodes = outNodes,
                Lines = CreateAndPopulateArray(subMesh.Lines.Length, Connection.Present)  
            };
        }




        public static MeshState<Connection> ConnectEverythingExceptEdges<T>(SubMesh<T> subMesh) where T : IGraphable
        {

            var outNodes = new Connection[subMesh.Connectivity.Nodes.Length];
            var outLines = CreateAndPopulateArray(subMesh.Connectivity.Lines.Length, Connection.Present);

            for (int i = 0; i < outNodes.Length; i++)
            {

                outNodes[i] = subMesh.Connectivity.Nodes[i];

                if (outNodes[i] != Connection.NotPresent)
                    continue;

                    var node = i.LookupNode(subMesh);

                for (int u = 0; u < node.Lines.Count; u++)
                {
                    if(node.Lines[u].ReverseLookupIndex(subMesh, out var index))
                    {
                        outLines[index] = Connection.NotPresent;
                    }
                }
            }



            return new MeshState<Connection>()
            {
                Nodes = outNodes,
                Lines = outLines
            };
        }

        public static MeshState<Connection> ConnectOnlyEdges<T>(SubMesh<T> subMesh) where T : IGraphable
        {

            var outNodes = new Connection[subMesh.Connectivity.Nodes.Length];
            var outLines = CreateAndPopulateArray(subMesh.Connectivity.Lines.Length, Connection.Present);

            for (int i = 0; i < outNodes.Length; i++)
            {

                outNodes[i] = subMesh.Connectivity.Nodes[i];

                if (outNodes[i] == Connection.NotPresent)
                    continue;

                var node = i.LookupNode(subMesh);

                for (int u = 0; u < node.Lines.Count; u++)
                {
                    if (node.Lines[u].ReverseLookupIndex(subMesh, out var index))
                    {
                        outLines[index] = Connection.NotPresent;
                    }
                }
            }


            return new MeshState<Connection>()
            {
                Nodes = outNodes,
                Lines = outLines
            };
        }




        //Really I should be finding the naked edges of the graph, for now, I'll do this assuming a hex grid as I have one.
        public static MeshState<Connection> RemoveUnnecessaryCriticalNodesAssumingHexGrid<T>(SubMesh<T> subMesh) where T : IGraphable
        {
            var outConnectivity = subMesh.Connectivity.Clone();

            for (int i = 0; i < subMesh.Nodes.Length; i++)
            {
                var node = subMesh.Nodes[i];

                if (subMesh.Connectivity.Nodes[i] != Connection.Critical)
                    continue;

                var mesh = subMesh.SourceMesh;

                var trueNode = mesh.Nodes[node];

                //if(trueNode.Lines.Count == 6)
                //{
                //    Debug.Log("I AM HAVING AN EFFECT");
                //}

                outConnectivity.Nodes[i] = trueNode.Lines.Count == 6? Connection.Present:Connection.Critical;
            }

            return outConnectivity;

        }

        public static MeshState<Connection> AddOneLayerOfEdgeBufferAroundNeighbourSubMeshesAssumingHexGrid<T>(SubMesh<T> subMesh) where T : IGraphable
        {
            var outConnectivity = subMesh.Connectivity.Clone();

            for (int i = 0; i < subMesh.Nodes.Length; i++)
            {
                var node = subMesh.Nodes[i];

                if (outConnectivity.Nodes[i] == Connection.Critical)
                    continue;

                var mesh = subMesh.SourceMesh;

                var trueNode = mesh.Nodes[node];

                var isBorder = false;

                //if(trueNode.Lines.Count != 6)
                //{
                //    //isBorder = true;
                //} else
                //{

                    for (int u = 0;u < trueNode.Lines.Count; u++)
                    {
                        if (!subMesh.LineMap.ContainsKey(trueNode.Lines[u].Index)){
                            isBorder = true;
                            goto end;
                        }
                            
                    }
                //}

                end:

                if(isBorder)
                {
                    outConnectivity.Nodes[i] = Connection.NotPresent;

                    for (int u = 0; u < trueNode.Lines.Count; u++)
                    {
                        if (subMesh.LineMap.ContainsKey(trueNode.Lines[u].Index))
                        {
                            var index = subMesh.LineMap[trueNode.Lines[u].Index];

                            outConnectivity.Lines[index] = Connection.NotPresent;
                            //mesh.Lines[trueNode.Lines[u].Index].DebugDraw(Color.magenta, 100f);



                        }
                    }
                }

            }

            return outConnectivity;

        }

        public static MeshState<Connection> RecoverOrphanedCriticalNodes<T>(SubMesh<T> subMesh) where T : IGraphable
        {
            var outConnectivity = subMesh.Connectivity.Clone();

            for (int i = 0; i < subMesh.Nodes.Length; i++)
            {
                var node = subMesh.Nodes[i];

                if (subMesh.Connectivity.Nodes[i] != Connection.Critical)
                    continue;

                var mesh = subMesh.SourceMesh;

                var trueNode = mesh.Nodes[node];

                var isOrphaned = true;

                //if(trueNode.Lines.Count != 6)
                //{
                //    //isBorder = true;
                //} else
                //{

                for (int u = 0; u < trueNode.Lines.Count; u++)
                {
                    if (subMesh.LineMap.ContainsKey(trueNode.Lines[u].Index))
                    {
                        var index = subMesh.LineMap[trueNode.Lines[u].Index];
                        if(outConnectivity.Lines[index] != Connection.NotPresent)
                        {
                            isOrphaned = false;
                        }                        
                    }
                }

                if (isOrphaned)
                {
                    var roadFound = false;
                    var currentNode = trueNode;

                    var options = new bool[6];
                    var optionsCount = 0;

                    var shortCircuit = 0;

                    while (!roadFound)
                    {
                        for (int u = 0; u < 6; u++)
                        {
                            options[u] = false;
                        }
                        optionsCount = 0;

                        SmartLine pickedLine = null;

                        for (int u = 0; u < currentNode.Lines.Count; u++)
                        {
                            if (subMesh.LineMap.ContainsKey(currentNode.Lines[u].Index))
                            {
                                options[u] = true;
                                optionsCount++;
                            }
                        }

                        var randomOption = RNG.Next(optionsCount);
                        var innerOptionCount = 0;

                        for (int u = 0; u < 6; u++)
                        {
                            if (options[u])
                            {
                                if(randomOption == innerOptionCount)
                                {
                                    pickedLine = currentNode.Lines[u];
                                    goto end;
                                }
                                innerOptionCount++;                                
                            }
                        }

                        if (pickedLine == null)
                            throw new System.Exception("Can't draw a line back to the source, good luck, nerd");

                        end:

                        var pickedLineIndex = subMesh.LineMap[pickedLine.Index];

                        currentNode = pickedLine.GetOtherNode(currentNode);

                        var pickedNodeIndex = subMesh.NodeMap[currentNode.Index];

                        outConnectivity.Lines[pickedLineIndex] = Connection.Present;
                        outConnectivity.Nodes[pickedNodeIndex] = Connection.Present;

                        var indexCount = 0;

                        for (int u = 0; u < currentNode.Lines.Count; u++)
                        {
                            if (subMesh.LineMap.ContainsKey(currentNode.Lines[u].Index))
                            {
                                var index = subMesh.LineMap[currentNode.Lines[u].Index];
                                if (outConnectivity.Lines[index] != Connection.NotPresent)
                                {
                                    indexCount++;
                                }
                            }
                        }

                        if(indexCount == 6) {
                            roadFound = true;
                        }

                        shortCircuit++;

                        if(shortCircuit > 999)
                        {
                            throw new System.Exception("This needs to be rewritten so that it can't get stuck in an infinite cycle");
                        }
                    }
                }

            }

            return outConnectivity;

        }


        public static MeshState<Connection> ConnectNothing<T>(SubMesh<T> subMesh) where T : IGraphable
        {
            return new MeshState<Connection>()
            {
                Nodes = CreateAndPopulateArray(subMesh.Nodes.Length, Connection.NotPresent),
                Lines = CreateAndPopulateArray(subMesh.Lines.Length, Connection.NotPresent)
            };
        }


        //Private Functions

        private static MeshState<Connection> Dikstra<T>(SubMesh<T> subMesh, float lineLengthMultiplierMin = 1f, float lineLengthMultiplierMax = 1f) where T : IGraphable
        {
            var nodes = subMesh.Nodes;
            var lines = subMesh.Lines;
            var mesh = subMesh.SourceMesh;

            var lineLengthRandomiser = new float[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                lineLengthRandomiser[i] = RNG.NextFloat(lineLengthMultiplierMin, lineLengthMultiplierMax);
            }

            var counter = -1;

            var firstIndex = subMesh.Nodes.First(x => {
                counter++;
                return subMesh.Connectivity.Nodes[counter] != Connection.NotPresent;
                });

            var isPartOfCurrentSortingEvent = new bool[nodes.Length];
            var visitedNodesList = new List<SmartNode>();


            var firstNode = mesh.Nodes[firstIndex];
            var visitedNodes = CreateAndPopulateArray(nodes.Length,Connection.NotPresent);
            var visitedLines = CreateAndPopulateArray(lines.Length,Connection.NotPresent);
            var visitedLinesIteration = new int[lines.Length];
            visitedNodesList.Add(firstNode);
            visitedNodes[subMesh.NodeMap[firstNode.Index]] = Connection.Present;
            var outputLines = new List<SmartLine>(firstNode.Lines); //hacky
            var iteration = 0;

            while (outputLines.Count > 0)
            {
                outputLines.Clear();
                iteration++;

                for (int i = 0; i < visitedNodesList.Count; i++)
                {
                    var n = visitedNodesList[i];

                    for (int u = 0; u < n.Lines.Count; u++)
                    {
                        var currentLine = n.Lines[u];
                        if (!subMesh.LineMap.ContainsKey(currentLine.Index))
                            continue;

                        var lineIndex = subMesh.LineMap[currentLine.Index];

                        if (visitedLines[lineIndex] == Connection.NotPresent &&
                            visitedLinesIteration[lineIndex] != iteration &&
                            subMesh.Connectivity.Lines[currentLine.ReverseLookupIndexUnsafe(subMesh)] != Connection.NotPresent) 

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

                if (bestLine == null)
                    break;

                try
                {
                    isPartOfCurrentSortingEvent[subMesh.NodeMap[bestLine.Nodes[0].Index]] = true;
                }
                catch(Exception ex)
                {
                    throw new System.Exception("Dikstra Failed. Ensure that there are connected lines and nodes in the graph you are manipulating.",ex);
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
                Nodes = subMesh.Connectivity.Nodes.Clone() as Connection[]
            };

            meshState.Lines = visitedLines;

            return meshState;
        }

        private static MeshState<Connection> RecursivelyRemoveDeadEnds<T>(SubMesh<T> subMesh, MeshState<Connection> state, int iterations = 99) where T : IGraphable
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
                    if (subMesh.Connectivity.Nodes[i] == Connection.Critical)
                        goto skip;

                    var node = i.LookupNode(subMesh);

                    var connectionsCount = 0;
                    var lastTrueLine = -1;
                    var lastIndex = -1;

                    for (int u = 0; u < node.Lines.Count; u++)
                    {
                        var testLine = node.Lines[u];

                        if (!subMesh.LineMap.ContainsKey(testLine.Index))
                            continue;

                        var testIndex = subMesh.LineMap[testLine.Index];

                        if (startState.Lines[testIndex] != Connection.NotPresent)
                        {
                            lastTrueLine = testLine.Index;
                            lastIndex = testIndex;
                            connectionsCount++;
                            //mesh.Lines[lastTrueLine].DebugDraw(Color.magenta, 3);
                        }
                    }

                    if (connectionsCount == 1)
                    {
                        var otherNode = subMesh.SourceMesh.Lines[lastTrueLine].GetOtherNode(node);

                        if (otherNode.ReverseLookupIndex(subMesh, out var otherIndex))
                        {
                            if (startState.Nodes[otherIndex] == Connection.Critical)
                                continue;
                        }

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

        private static MeshState<Connection> Entubben<T>(SubMesh<T> subMesh, MeshState<Connection> state, int nodeConnectionsThreshold) where T : IGraphable
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

        private static bool NodeAtIndexConnectsToOtherSubMesh<T>(SubMesh<T> subMesh, int index) where T : IGraphable
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

        private static T[] CreateAndPopulateArray<T>(int length, T value)
        {
            var array = new T[length];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }

            return array;
        }


    }

    public static class Height
    {


            public static MeshState<int> GetDistanceFromEdge<T>(SubMesh<T> subMesh, int iterations) where T : IGraphable
        {
            var nodes = subMesh.Nodes;
            var lines = subMesh.Lines;
            var mesh = subMesh.SourceMesh;

            var state = new MeshState<int>();
            state.Nodes = new int[nodes.Length];

                for (int i = 0; i < nodes.Length; i++)
                {

                    var node = i.LookupNode(subMesh);

                    for (int u = 0; u < node.Lines.Count; u++)
                    {
                        var testLine = node.Lines[u];

                        if (!subMesh.LineMap.ContainsKey(testLine.Index))
                        {
                            state.Nodes[i] = 1;
                        goto skip;
                        }
                    }

                skip:
                    continue;
                }

            for (int u = 0; u < iterations-1; u++)
            {

                for (int i = 0; i < nodes.Length; i++)
                {
                    if (state.Nodes[i] > 0)
                        continue;

                    var node = i.LookupNode(subMesh);

                    for (int v = 0; v < node.Lines.Count; v++)
                    {
                        var testLine = node.Lines[v];

                        if (!subMesh.LineMap.ContainsKey(testLine.Index))
                            continue;

                        var testIndex = subMesh.LineMap[testLine.Index];

                        if (subMesh.Connectivity.Lines[testIndex] == Connection.Present)
                        {
                            var index = testLine.GetOtherNode(node).ReverseLookupIndexUnsafe(subMesh);

                            if(state.Nodes[index] == u+1)
                            state.Nodes[i] = u + 2;
                        }
                    }
                }
            }

            return state;
        }
    }

    internal static class SharedUtils
    {
        public static SmartNode LookupNode<T>(this int i, SubMesh<T> subMesh) where T : IGraphable
        {
            return subMesh.SourceMesh.Nodes[subMesh.Nodes[i]];
        }


        public static SmartLine LookupLine<T>(this int i, SubMesh<T> subMesh) where T : IGraphable
        {
            return subMesh.SourceMesh.Lines[subMesh.Lines[i]];
        }

        public static int ReverseLookupIndexUnsafe<T>(this SmartNode node, SubMesh<T> subMesh) where T : IGraphable
        {
            return subMesh.NodeMap[node.Index];
        }

        public static int ReverseLookupIndexUnsafe<T>(this SmartLine node, SubMesh<T> subMesh) where T : IGraphable
        {
            return subMesh.LineMap[node.Index];
        }

        public static bool ReverseLookupIndex<T>(this SmartNode node, SubMesh<T> subMesh, out int index) where T : IGraphable
        {
            if (!subMesh.NodeMap.ContainsKey(node.Index))
            {
                index = -1;
                return false;
            }

            index = subMesh.NodeMap[node.Index];

            return true;
        }

        public static bool ReverseLookupIndex<T>(this SmartLine line, SubMesh<T> subMesh, out int index) where T : IGraphable
        {
            if (!subMesh.LineMap.ContainsKey(line.Index))
            {
                index = -1;
                return false;
            }

            index = subMesh.LineMap[line.Index];

            return true;
        }
    }
        


}