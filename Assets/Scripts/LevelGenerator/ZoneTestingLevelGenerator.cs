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

            /// 2: Initialise wider neighbourhood with different colours
            for (int i = 0; i < layer1WiderNeighbourhood.Count; i++)
            {
                var n = layer1.Mesh.Nodes[layer1WiderNeighbourhood[i]].Index;
                layer1.NodeMetadata[n] = new NodeMetadata(i + 1, RNG.NextColor(), new int[] { }, RNG.NextFloat(5)) { Height = RNG.NextFloat(-3, 3) };
            }

            return new CleverMesh(layer1, layer1WiderNeighbourhood.ToArray());
        }

        private CleverMesh Layer2(CleverMesh parentLayer)
        {
            //var layer = new CleverMesh(parentLayer);
            var neigh = SubMesh.FromMesh(parentLayer);
            neigh.ToList().ForEach(x => {
                x.ApplyState(Dikstra);
                x.DebugDraw(RNG.NextColor(), 100f);               
            });
            return parentLayer;
        }

        private MeshState<int> Dikstra(SmartMesh mesh, int[] Nodes, int[] Lines)
        {
            return null;
        }
    }
}
