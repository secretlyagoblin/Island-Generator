using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using U3D.Threading.Tasks;
using MeshMasher;

namespace LevelGenerator {
    public class MainLevelGenerator: LevelGenerator {

        public MainLevelGenerator(int startIndex, LevelGeneratorSettings settings) : base(settings)
        {
            _cellIndex = startIndex;
        }

        private int _cellIndex;

        public override void Generate()
        {
            var layer2 = CreateWalkableTerrain();
            CreateObject(layer2);
            return;
            var layer3 = CreateLayer3(layer2);
            CreateObject(layer3);
            
        }

        private CleverMesh CreateWalkableTerrain()
        {
            ///Assumptions:
            /// Currently not considering height differences
            /// Pathfinding could be better
            /// Biome just means colours at this point

            ///Below we:
            /// 1: Create a series of regions

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
                layer1.NodeMetadata[n] = new NodeMetadata(i + 1, RNG.NextColor(), new int[] { }, RNG.NextFloat(5));
            }

            /// 3: Generate some basic connectivity
            var level1Border1State = layer1.Mesh.CreateMeshState<int>();
            layer1WiderNeighbourhood
                .SelectMany(x => layer1.Mesh.Nodes[x].Lines)
                .Distinct()
                .Where(x => layer1WiderNeighbourhood.Contains(x.Nodes[0].Index) && layer1WiderNeighbourhood.Contains(x.Nodes[1].Index))
                .ToList()
                .ForEach(x => {
                    if (RNG.SmallerThan(0.7f))
                    { level1Border1State.Lines[x.Index] = 1; }
                    else
                    { level1Border1State.Lines[x.Index] = 0; }
                });

            var layer1RegionConnections = new List<SimpleVector2Int>();

            layer1.Mesh.Lines.ForEach(x =>
            {
                var nodeA = x.Nodes[0].Index;
                var nodeB = x.Nodes[1].Index;

                if (level1Border1State.Lines[x.Index] == 1)
                {
                    level1Border1State.Lines[x.Index] = 1;

                    var codeA = layer1.NodeMetadata[nodeA].Code;
                    var codeB = layer1.NodeMetadata[nodeB].Code;

                    layer1RegionConnections.Add(codeA < codeB ? new SimpleVector2Int(codeA, codeB) : new SimpleVector2Int(codeB, codeA));
                }
            });

            layer1RegionConnections = layer1RegionConnections.Distinct().ToList();

            ///Below we:
            /// 1: Create a boundary area that is a no-go zone.
            /// 2: Calculate a connectivity graph between regions (TODO: fix distance to be based on distance from node center based on layer 1)
            /// 3: Give each walkable node a special room code
            /// 4: Give each walkable node a connectivity map
            /// 5: TODO: Create mini-valleys using voronoi falloff where connectivity should be broken.
            /// 6: TODO: Define higher level biomes based on parent colour

            var layer2 = new CleverMesh(layer1, layer1WiderNeighbourhood.ToArray(), MeshMasher.NestedMeshAccessType.Vertex);

            // 1: Create a boundary area that is a no-go zone.
            var layer2IsExteriorBorder = layer2.Mesh.GetBorderNodes();

            for (int i = 0; i < layer2IsExteriorBorder.Nodes.Length; i++)
            {
                if (layer2IsExteriorBorder.Nodes[i] == true)
                {
                    layer2.NodeMetadata[i].Code = 0;
                    layer2.NodeMetadata[i].SmoothColor = Color.white;
                }
            }            

            for (int i = 0; i < layer2.RingNodeMetadata.Length; i++)
            {
                layer2.RingNodeMetadata[i].Code = 0;
                layer2.RingNodeMetadata[i].SmoothColor = Color.white;
            }

            var layer2IsBorder = layer2IsExteriorBorder.Clone();

            var splines = RNG.Shuffle(layer2.Mesh.Lines);
            for (int i = 0; i < layer2IsBorder.Lines.Length; i++)
            {
                var line = splines[i];
                if (layer2IsExteriorBorder.Lines[line.Index] | (layer2IsExteriorBorder.Nodes[line.Nodes[0].Index] && layer2IsExteriorBorder.Nodes[line.Nodes[1].Index]))
                {
                    line.DebugDraw(Color.white, 100f);
                    continue;
                }
                   

                var codeA = layer2.NodeMetadata[line.Nodes[0].Index].Code;

                var codeB = layer2.NodeMetadata[line.Nodes[1].Index].Code;
                var testCode = codeA < codeB ? new SimpleVector2Int(codeA, codeB) : new SimpleVector2Int(codeB, codeA);
                if (codeA == codeB )
                {
                    layer2IsBorder.Lines[line.Index] = false;
                    //line.DebugDraw(layer2.NodeMetadata[line.Nodes[0].Index].SmoothColor, 100f);
                    layer2IsBorder.Nodes[line.Nodes[0].Index] = false;
                    layer2IsBorder.Nodes[line.Nodes[1].Index] = false;
                    //line.DebugDraw(Color.green, 100f);

                }
                else if (layer1RegionConnections.Contains(testCode))
                {
                    layer1RegionConnections.Remove(testCode);
                    layer2IsBorder.Lines[line.Index] = false;
                    //line.DebugDraw(layer2.NodeMetadata[line.Nodes[0].Index].SmoothColor, 100f);
                    layer2IsBorder.Nodes[line.Nodes[0].Index] = false;
                    layer2IsBorder.Nodes[line.Nodes[1].Index] = false;
                    line.DebugDraw(Color.green, 100f);
                }
                else
                {
                    layer2IsBorder.Lines[line.Index] = true;
                    layer2IsBorder.Nodes[line.Nodes[0].Index] = true;
                    layer2IsBorder.Nodes[line.Nodes[1].Index] = true;
                    line.DebugDraw(Color.blue, 100f);
                }
            }        


            // 2: Calculate a connectivity graph between regions (TODO: fix distance to be based on distance from node center based on layer 1)

            var minimum = layer2.Mesh.MinimumSpanningTree(layer2IsBorder);

            //layer2.Mesh.DrawMesh(Root.transform);
            layer2.Mesh.DrawRoads(Root.transform, minimum);

           // return layer2;

            // 3: Give each walkable node a special room code and color

            //var roomNumber = 1;
            //
            //for (int i = 0; i < layer2.NodeMetadata.Length; i++)
            //{
            //    if (layer2.NodeMetadata[i].Code != 0)
            //    {
            //        layer2.NodeMetadata[i].Code = roomNumber;
            //        roomNumber++;
            //        //layer2.CellMetadata[i].SmoothColor = Color.white;
            //    }
            //    else
            //    {
            //        //layer2.CellMetadata[i].Code = 0;
            //        layer2.NodeMetadata[i].SmoothColor = Color.black;
            //    }
            //}

            //CreateObject(layer2);
            //return;


            // 4: TODO: Give each walkable node a connectivity map

            for (int i = 0; i < layer2.Mesh.Nodes.Count; i++)
            {
                var n = layer2.Mesh.Nodes[i];

                if (layer2IsBorder.Nodes[n.Index] | layer2.NodeMetadata[i].Code == 0)
                {
                    layer2.NodeMetadata[n.Index].SmoothColor = Color.magenta;
                    continue;
                }

                //layer2.NodeMetadata[n.Index].SmoothColor = layer2State.Nodes[n.Index] == 1 ? layer2.NodeMetadata[n.Index].SmoothColor : Color.black;





                layer2.NodeMetadata[n.Index].Height += RNG.NextFloat(-0.5f, 0.5f);
                layer2.NodeMetadata[n.Index].Connections = n
                    .Lines
                    .Where(x => layer2IsBorder.Lines[x.Index])
                    .Select(x => x.GetOtherNode(n).Index + 1)
                    .Union(new List<int>() { i + 1 })
                    .ToArray();
                layer2.NodeMetadata[n.Index].Code = i + 1;
            }

            //CreateObject(layer2);
            //return;

            return layer2;
        }

        private CleverMesh CreateLayer3(CleverMesh layerAbove)
        {
            var layer = new CleverMesh(layerAbove, layerAbove.Mesh.Nodes.Select(x => x.Index).ToArray(), MeshMasher.NestedMeshAccessType.Vertex);

            var valuesToIterateOver = new List<int>();

            for (int i = 0; i < layer.Mesh.Nodes.Count; i++)
            {
                var n = layer.Mesh.Nodes[i];

                if (layer.NodeMetadata[n.Index].Code == 0)
                {
                    valuesToIterateOver.Add(i);
                    //layer3.NodeMetadata[n.Index].SmoothColor = Color.grey;
                    layer.NodeMetadata[n.Index].IsFuzzyBoundary = true;
                    layer.NodeMetadata[n.Index].CliffDistance = 0.2f;
                    layer.NodeMetadata[n.Index].SmoothColor = new Color(0.2f, 0.2f, 0.2f);
                    continue;
                }

                var colour = layer.NodeMetadata[n.Index].MeshDual;
                colour = layer.NodeMetadata[n.Index].IsTrueBoundary ? 1: colour;
                var dist = layer.NodeMetadata[n.Index].IsTrueBoundary;

                if (layer.NodeMetadata[n.Index].IsTrueBoundary == true)
                {
                    valuesToIterateOver.Add(i);
                    layer.NodeMetadata[n.Index].IsFuzzyBoundary = true;
                    layer.NodeMetadata[n.Index].CliffDistance = 0.2f;
                    layer.NodeMetadata[n.Index].SmoothColor = new Color(0.2f, 0.2f, 0.2f);

                    //layer3.NodeMetadata[n.Index].SmoothColor = Color.grey;
                }
                else
                {
                    //layer.NodeMetadata[n.Index].IsFuzzyBoundary = true;
                    //layer3.NodeMetadata[n.Index].SmoothColor = layer3.NodeMetadata[n.Index].SmoothColor;
                    //layer3.NodeMetadata[n.Index].SmoothColor = new Color(
                }
                layer.NodeMetadata[n.Index].IsFuzzyBoundary = false;



                //    Mathf.Min(layer3.NodeMetadata[n.Index].Distance, layer3.NodeMetadata[n.Index].SmoothColor.r),
                //    Mathf.Min(layer3.NodeMetadata[n.Index].Distance, layer3.NodeMetadata[n.Index].SmoothColor.g),
                //    Mathf.Min(layer3.NodeMetadata[n.Index].Distance, layer3.NodeMetadata[n.Index].SmoothColor.b));


                //layer3.NodeMetadata[n.Index].Distance < 0.5f ? Color.black : layer3.NodeMetadata[n.Index].SmoothColor;

                //layer3.CellMetadata[n.Index].SmoothColor = new Color(colour, colour, colour);
                layer.NodeMetadata[n.Index].Height += RNG.NextFloat(-0.1f, 0.1f);
            }

            var color = 0.2f;

            for (int v = 0; v < 4; v++)
            {
                var valuesToSplunge = new List<int>();

                for (int i = 0; i < valuesToIterateOver.Count; i++)
                {
                    var n = layer.Mesh.Nodes[valuesToIterateOver[i]];


                    for (int u = 0; u < n.Nodes.Count; u++)
                    {
                        var neigh = n.Nodes[u];
                        if (layer.NodeMetadata[neigh.Index].CliffDistance != color)
                        {
                            goto end;
                        }
                    }
                    valuesToSplunge.Add(n.Index);
                    end:
                    ;
                }

                color = color + 0.2f;

                for (int i = 0; i < valuesToSplunge.Count; i++)
                {
                    layer.NodeMetadata[valuesToSplunge[i]].SmoothColor = new Color(color, color, color);
                    layer.NodeMetadata[valuesToSplunge[i]].CliffDistance = color;
                }

            }

            //for (int i = 0; i < layer.Mesh.Nodes.Count; i++)
            //{
            //    var n = layer.Mesh.Nodes[i];
            //    layer.NodeMetadata[n.Index].SmoothColor = layer.NodeMetadata[n.Index].Walkable ? Color.white : Color.black;
            //
            //}



            //CreateObject(layer3);
            return layer;
        }

    }
}
