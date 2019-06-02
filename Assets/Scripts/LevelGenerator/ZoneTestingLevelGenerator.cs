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

            for (int i = 0; i < layer2.NodeMetadata.Length; i++)
            {
                var n = layer2.NodeMetadata[i];
            
                var c = n.IsTrueBoundary ? Color.black : layer2.NodeMetadata[i].SmoothColor;
            
                layer2.NodeMetadata[i].SmoothColor = c;
            }

            this.CreateObjectXY(layer2);
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
                layer1.NodeMetadata[n].RoomConnections = subMesh.ConnectionsFromState(n);
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

                //if (RNG.CoinToss())
                //    if(RNG.CoinToss())
                //        m.ApplyState(DikstraWithRandomisation);
                //    else
                //        m.ApplyState(OpenPlains);
                //else
                //    m.ApplyState(SummedDikstra);


                var len = parentLayer.NodeMetadata[m.Nodes[0]].RoomConnections.Length;

                if(len > 3)
                {
                    
                    m.ApplyState(OpenPlains);
                    m.DebugDraw(Color.green, 20f);

                }
                else
                if (len < 2)
                {
                    m.ApplyState(DikstraWithRandomisation);
                    m.DebugDraw(Color.red, 20f);
                }
                else
                {
                    m.ApplyState(SummedDikstraTrimmed);
                    m.DebugDraw(Color.yellow, 20f);

                    //m.DebugDraw(RNG.NextColor(), 10f);
                }

                
            }

            for (int i = 0; i < meshCollection.Bridges.Length; i++)
            {
                var b = meshCollection.Bridges[i];
                var l = RNG.CoinToss(1, 2);

                for (int u = 0; u < b.LineCodes.Length; u++)
                {
                    b.LineCodes[u] = u < l ? 1 : 0;
                }
            }            

            meshCollection.DebugDisplayEnabledBridges(Color.blue, 50);

            int[][] nodeValues = meshCollection.GetConnectionData(); //< - need to impliment this function, going through edges and determining node connections
            
            for (int i = 0; i < parentLayer.NodeMetadata.Length; i++)
            {
                var nodeValue = nodeValues[i];
                parentLayer.NodeMetadata[i].RoomCode = i;
            
                parentLayer.NodeMetadata[i].RoomConnections = nodeValue;
            }

            return new CleverMesh(parentLayer);
        }

        private static MeshState<int> DikstraWithRandomisation(SubMesh subMesh)
        {
            var nodes = subMesh.Nodes;
            var lines = subMesh.Lines;
            var mesh = subMesh.ParentMesh.Mesh;

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

        private static MeshState<int> OpenPlains(SubMesh subMesh)
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

        private static MeshState<int> SummedDikstra(SubMesh subMesh)
        {
            var one = DikstraWithRandomisation(subMesh);
            var two = DikstraWithRandomisation(subMesh);

            for (int i = 0; i < subMesh.Lines.Length; i++)
            {
                one.Lines[i] = two.Lines[i] == 1 ? 1 : one.Lines[i];
            }

            return one;
        }

        private static MeshState<int> SummedDikstraTrimmed(SubMesh subMesh)
        {
            var nodes = subMesh.Nodes;
            var lines = subMesh.Lines;
            var mesh = subMesh.ParentMesh.Mesh;

            var keyMeshState = DikstraWithRandomisation(subMesh);
            //var anotherMeshState = DikstraWithRandomisation(mesh, nodes, lines);
            //
            //for (int i = 0; i < lines.Length; i++)
            //{
            //    keyMeshState.Lines[i] = anotherMeshState.Lines[i] == 1 ? 1 : keyMeshState.Lines[i];
            //}

            //Getting nodeMaps
            
            var nodeMap = new Dictionary<int, int>();
            for (int i = 0; i < nodes.Length; i++)
            {
                nodeMap.Add(nodes[i], i);
            }

            var lineMap = new Dictionary<int, int>();
            for (int i = 0; i < lines.Length; i++)
            {
                lineMap.Add(lines[i], i);
            }

            var newKeyStateLines = (int[])keyMeshState.Lines.Clone();

            for (int i = 0; i < nodes.Length; i++)
            {
                if (subMesh.ConnectionNodes.Contains(i))
                    continue;

                var node = mesh.Nodes[nodes[i]];



                var connectionsCount = 0;
                var lastTrueLine = -1;
                var lastIndex = -1;

                for (int u = 0; u < node.Lines.Count; u++)
                {
                    if (!lineMap.ContainsKey(node.Lines[u].Index))
                        continue;

                    var testIndex = lineMap[node.Lines[u].Index];

                    if (keyMeshState.Lines[testIndex] == 1)
                    {
                        lastTrueLine = node.Lines[u].Index;
                        lastIndex = testIndex;
                        connectionsCount++;
                        //mesh.Lines[lastTrueLine].DebugDraw(Color.magenta, 3);
                    }
                }

                if(connectionsCount == 1)
                {
                    //keyMeshState.Nodes[i] = 0;
                    //
                    //try
                    //{
                        newKeyStateLines[lastIndex] = 0;
                        mesh.Lines[lastTrueLine].DebugDraw(Color.magenta, 5);
                    //}
                    //catch
                    //{
                    //    Debug.Log("Whaaaa");
                    //}
                }
            }

            keyMeshState.Lines = newKeyStateLines;

            return keyMeshState;
        }
    }
}
