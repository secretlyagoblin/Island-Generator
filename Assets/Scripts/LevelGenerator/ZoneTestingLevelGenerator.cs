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

            for (int i = 0; i < layer2.Length; i++)
            {
                var layer = layer2[i];

                for (int u = 0; u < layer.NodeMetadata.Length; u++)
                {
                    var n = layer.NodeMetadata[u];

                    var c = n.IsTrueBoundary ? Color.black : layer.NodeMetadata[u].SmoothColor;

                    layer.NodeMetadata[u].SmoothColor = c;
                }

                var layer3 = new CleverMesh(layer);

                this.CreateObjectXY(layer3);
                //this.crea
            }


        }

        protected override void FinaliseMesh(CleverMesh mesh)
        {
            throw new System.NotImplementedException();
        }

        private CleverMesh Layer1()
        {
            var ints = new Vector2Int[4];
            var count = 0;

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    ints[count] = new Vector2Int(x, y);
                    count++;
                }
            }


            /// 1: Create a series of regions
            var layer1 = new CleverMesh(ints.ToList(), _meshTile);
            var layer1NodeIndex = _cellIndex;
            var layer1Neighbourhood = layer1.Mesh.Nodes[layer1NodeIndex].Nodes.ConvertAll(x => x.Index);
            var layer1WiderNeighbourhood = layer1Neighbourhood.SelectMany(x => layer1.Mesh.Nodes[x].Nodes).Distinct().ToList().ConvertAll(x => x.Index);
            layer1Neighbourhood.Add(layer1NodeIndex);

            var layer1IncludingBorder = layer1WiderNeighbourhood.SelectMany(x => layer1.Mesh.Nodes[x].Nodes).Distinct().ToList().ConvertAll(x => x.Index);

            var subMesh = new SubMesh(1, layer1WiderNeighbourhood.ToArray(), layer1);
            subMesh.ApplyState(States.SummedDikstra);
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

            return new CleverMesh(layer1, layer1IncludingBorder.ToArray());
        }

        private CleverMesh[] Layer2(CleverMesh parentLayer)
        {
            //var layer = new CleverMesh(parentLayer);

            var meshCollection = new MeshCollection(parentLayer);

            for (int i = 0; i < meshCollection.Bridges.Length; i++)
            {
                var b = meshCollection.Bridges[i];
                var l = RNG.CoinToss(1, 1);

                for (int u = 0; u < b.LineCodes.Length; u++)
                {
                    b.LineCodes[u] = u < l ? 1 : 0;
                    //b.LineCodes[u] = 1;
                }
            }

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

                var firstNode = parentLayer.NodeMetadata[m.Nodes[0]];
                var roomConnections = firstNode.RoomConnections.Length;

                if(roomConnections >1)
                {
                    if (RNG.SmallerThan(0.7))
                    {
                        m.ApplyState(States.TubbyCorridors);
                        //m.DebugDraw(Color.green, 20f);
                    }
                    else
                    {
                        m.ApplyState(States.SummedDikstraRemoveDeadEnds);
                        //m.DebugDraw(Color.yellow, 20f);
                    }
                }
                else
                {
                    m.ApplyState(States.DikstraWithRooms);
                    //m.DebugDraw(Color.red, 20f);
                }            
            }





            //meshCollection.DebugDisplayEnabledBridges(Color.blue, 50);

            int[][] nodeValues = meshCollection.GetConnectionMetadata();

            //populate connection data
            for (int i = 0; i < parentLayer.NodeMetadata.Length; i++)
            {
                var nodeValue = nodeValues[i];

                if (nodeValue.Length == 0)
                {
                    parentLayer.NodeMetadata[i].RoomCode = 0;
                    parentLayer.NodeMetadata[i].SmoothColor = Color.black;
                }
                else
                {
                    parentLayer.NodeMetadata[i].RoomCode = i;
                }                
            
                parentLayer.NodeMetadata[i].RoomConnections = nodeValue;
            }

            var cleverMeshes = new CleverMesh[meshCollection.Meshes.Length];

            for (int i = 0; i < cleverMeshes.Length; i++)
            //for (int i = 0; i < 1; i++)
            {
                cleverMeshes[i] = new CleverMesh(parentLayer, meshCollection.Meshes[i].Nodes);
            }

            return cleverMeshes;
        }

        private CleverMesh[] Layer3(CleverMesh parentLayer)
        {
            var meshCollection = new MeshCollection(parentLayer);

            var cleverMeshes = new CleverMesh[1];//meshCollection.Meshes.Length];

            //for (int i = 0; i < cleverMeshes.Length; i++)
            for (int i = 0; i < 1; i++)
            {
                cleverMeshes[i] = new CleverMesh(parentLayer, meshCollection.Meshes[i].Nodes);
            }

            return cleverMeshes;


        }
    }
}
