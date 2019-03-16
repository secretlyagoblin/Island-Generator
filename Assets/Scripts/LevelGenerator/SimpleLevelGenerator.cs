using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using U3D.Threading.Tasks;
using MeshMasher;

namespace LevelGenerator {
    public class SimpleLevelGenerator: LevelGenerator {

        public SimpleLevelGenerator(int startIndex, LevelGeneratorSettings settings) : base(settings)
        {
            _cellIndex = startIndex;
        }

        private int _cellIndex;

        public override void Generate()
        {
            var colors = new Color[] { Color.red, Color.green, Color.blue };

            Debug.Log("Layer 1: ");

            var layer1 = new CleverMesh(new List<Vector2Int>() { Vector2Int.zero }, _meshTile);

            //CreateObject(layer1);
            //CreateRing(layer1);



            var neighbourhood = layer1.Mesh.Nodes[_cellIndex].Nodes.ToList().ConvertAll(x => x.Index);
            var widerNeighbourhood = neighbourhood.SelectMany(x => layer1.Mesh.Nodes[x].Nodes).Distinct().ToList().ConvertAll(x => x.Index);
            neighbourhood.Add(_cellIndex);

            for (int i = 0; i < neighbourhood.Count; i++)
            {
                var n = layer1.Mesh.Nodes[neighbourhood[i]];
                layer1.NodeMetadata[n.Index] = new NodeMetadata(i + 1, RNG.NextColor(), new int[] { }, RNG.NextFloat(5));
            }

            Debug.Log("Layer 2: ");

            var layer2 = new CleverMesh(layer1,
                widerNeighbourhood.ToArray(),
                //cellIndex,
                MeshMasher.NestedMeshAccessType.Vertex);

            /*

            Debug.Log("Layer 2: ");

            var layer2 = new CleverMesh(layer1,
                cellIndex,
                //cellIndex,
                MeshMasher.NestedMeshAccessType.Vertex);

            CreateObject(layer2);
            CreateRing(layer2);

            */

            //return;

            var layer2obj = CreateObjectXY(layer2);
            var layer2ring = CreateRing(layer2);
            layer2ring.transform.parent = layer2obj.transform;
            layer2ring.name = "Layer2ring";
            layer2obj.transform.Translate(Vector3.back);

            //Debug.Log("Layer 3: ");
            //
            //var layer3 = new CleverMesh(layer2,
            //    layer2.Mesh.Nodes[0].Index,
            //    //cellIndex,
            //    MeshMasher.NestedMeshAccessType.Vertex);
            //
            //var layer4 = new CleverMesh(layer3,
            //    layer3.Mesh.Nodes.ConvertAll(x => x.Index).ToArray(),
            //    //cellIndex,
            //    MeshMasher.NestedMeshAccessType.Vertex);


            Debug.Log("Layer 4: ");
            //GameObject.StartCoroutine(CreateSimple(layer2, MeshMasher.NestedMeshAccessType.Vertex));

            CreateSimpleJobAsync(layer2, CreateSimpleMeshTile);



        }



    }
}
