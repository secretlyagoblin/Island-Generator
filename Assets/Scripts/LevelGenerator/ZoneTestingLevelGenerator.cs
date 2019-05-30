using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshMasher;
using System.Linq;

namespace LevelGenerator {
    public class ZoneTestingLevelGenerator : LevelGenerator {

        int _cellIndex;

        public ZoneTestingLevelGenerator(int startIndex, LevelGeneratorSettings settings) : base(settings)
        {
            _cellIndex = startIndex;
        }

        public override void Generate()
        {
            var layer1 = Layer1();
            var layer2 = Layer2(layer1);
        }

        protected override void FinaliseMesh(CleverMesh mesh)
        {
            throw new System.NotImplementedException();
        }

        private CleverMesh Layer1()
        {
            /// 1: Create a series of regions
            var layer1 = new CleverMesh(new List<Vector2Int>() { Vector2Int.zero }, _meshTile);
            var layer1NodeIndex = _cellIndex;
            var layer1Neighbourhood = layer1.Mesh.Nodes[layer1NodeIndex].Nodes.ConvertAll(x => x.Index);
            var layer1WiderNeighbourhood = layer1Neighbourhood.SelectMany(x => layer1.Mesh.Nodes[x].Nodes).Distinct().ToList().ConvertAll(x => x.Index);
            layer1Neighbourhood.Add(layer1NodeIndex);

            var subMesh = new SubMesh(1, layer1WiderNeighbourhood.ToArray(), layer1);
            subMesh.ApplyState(SummedDikstra);
            //subMesh.DebugDraw(Color.red, 4f);

            /// 2: Initialise wider neighbourhood with different colours
            for (int i = 0; i < layer1WiderNeighbourhood.Count; i++)
            {
                var n = layer1.Mesh.Nodes[layer1WiderNeighbourhood[i]].Index;
                layer1.NodeMetadata[n] = new NodeMetadata(i+1, RNG.NextColor(), new int[] { }, RNG.NextFloat(5)) { Height = RNG.NextFloat(-3, 3) };
            }

            //var gameState = new GameSpace(layer1);
            //gameState.ApplyState(new System.Func<SmartMesh, int[], int[], MeshState<int>>[] { SummedDikstra });

            for (int i = 0; i < layer1WiderNeighbourhood.Count; i++)
            {
                var n = layer1.Mesh.Nodes[layer1WiderNeighbourhood[i]].Index;
                layer1.NodeMetadata[n].Connections = subMesh.ConnectionsFromState(n);
                //layer1.NodeMetadata[n].Code = i + 1;
            }    

            return new CleverMesh(layer1, layer1WiderNeighbourhood.ToArray());
        }

        private CleverMesh Layer2(CleverMesh parentLayer)
        {
            //var layer = new CleverMesh(parentLayer);

            var meshCollection = new MeshCollection(parentLayer);

            for (int i = 0; i < meshCollection.Meshes.Length; i++)
            {
                var m = meshCollection.Meshes[i];

                if (RNG.CoinToss())
                    m.ApplyState(DikstraWithRandomisation);
                else
                    m.ApplyState(SummedDikstra);
                m.DebugDraw(RNG.NextColor(), 10f);
            }

            for (int i = 0; i < meshCollection.Bridges.Length; i++)
            {
                var b = meshCollection.Bridges[i];
                var l = RNG.CoinToss(1, 2);

                for (int u = 0; u < b.LineCode.Length; u++)
                {
                    b.LineCode[u] = u < l ? 1 : 0;
                }
            }            

            meshCollection.DebugDisplayEnabledBridges(Color.green, 50);

            //int[][] nodeValues = meshCollection.GetConnectionData(); //< - need to impliment this function, going through edges and determining node connections
            //
            //for (int i = 0; i < parentLayer.NodeMetadata.Length; i++)
            //{
            //    var nodeValue = nodeValues[i];
            //
            //    parentLayer.NodeMetadata[i].Connections = nodeValue;
            //}



            return parentLayer;
        }

        private MeshState<int> DikstraWithRandomisation(SmartMesh mesh, int[] nodes, int[] lines)
        {
            var nodeMap = new Dictionary<int, int>();
            for (int i = 0; i < nodes.Length; i++)
            {
                nodeMap.Add(nodes[i],i);
            }

            var lineMap = new Dictionary<int, int>();
            var lineLengthRandomiser = new float[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                lineMap.Add(lines[i], i);
                lineLengthRandomiser[i] = RNG.NextFloat(0.8f, 1.2f);
            }

            var isPartOfCurrentSortingEvent = new bool[nodes.Length];
            var visitedNodesList = new List<SmartNode>();
            var firstNode = mesh.Nodes[nodes[0]];
            var visitedNodes = new int[nodes.Length];
            var visitedLines = new int[lines.Length];
            var visitedLinesIteration = new int[lines.Length];
            visitedNodesList.Add(firstNode);
            visitedNodes[nodeMap[firstNode.Index]] = 1;
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
                        if (!lineMap.ContainsKey(n.Lines[u].Index))
                            continue;

                        var lineIndex = lineMap[n.Lines[u].Index];

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
                    var randomMultiplier = lineLengthRandomiser[lineMap[line.Index]];

                    if (line.Length* randomMultiplier > length)
                        continue;

                    if (isPartOfCurrentSortingEvent[nodeMap[line.Nodes[0].Index]] &&
                        isPartOfCurrentSortingEvent[nodeMap[line.Nodes[1].Index]])
                        continue;

                    length = line.Length* randomMultiplier;
                    bestLine = line;
                }
                try
                {
                    isPartOfCurrentSortingEvent[nodeMap[bestLine.Nodes[0].Index]] = true;
                }
                catch
                {
                    Debug.Log("Whey");
                }

                isPartOfCurrentSortingEvent[nodeMap[bestLine.Nodes[1].Index]] = true;
                visitedLines[lineMap[bestLine.Index]] = 1;

                for (int i = 0; i < bestLine.Nodes.Count; i++)
                {
                    var n = bestLine.Nodes[i];
                    if (visitedNodes[nodeMap[n.Index]] == 0)
                    {
                        visitedNodesList.Add(n);
                        visitedNodes[nodeMap[n.Index]] = 1;
                    }
                }
            }

            var meshState = new MeshState<int>();
            meshState.Nodes = new int[nodes.Length];
            meshState.Lines = visitedLines;

            return meshState;
        }

        private MeshState<int> SummedDikstra(SmartMesh mesh, int[] Nodes, int[] Lines)
        {
            var one = DikstraWithRandomisation(mesh, Nodes, Lines);
            var two = DikstraWithRandomisation(mesh, Nodes, Lines);

            for (int i = 0; i < Lines.Length; i++)
            {
                one.Lines[i] = two.Lines[i] == 1 ? 1 : one.Lines[i];
            }

            return one;
        }
    }
}
