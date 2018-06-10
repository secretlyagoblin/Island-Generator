using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StructureV2 : MonoBehaviour {


    public GameObject InstantiationBase;
    public AnimationCurve FalloffCurve;
    public Gradient Gradient;
    public TextAsset meshTileData;

    public bool EnablePreview = true;

    // Use this for initialization
    void Start()
    {
        RNG.DateTimeInit();

        var colors = new Color[] { Color.red, Color.green, Color.yellow };

        ///Assumptions:
        /// Currently not considering height differences
        /// Pathfinding could be better
        /// Biome just means colours at this point

        ///Below we:
        /// 1: Create a single triangle
        /// 2: Give each triangle a different biome (3 zones)

        #region layer one

        /// 1: Create a single triangle
        var layer1 = new CleverMesh(new List<Vector2Int>() {Vector2Int.zero}, meshTileData.text);
        var cellIndex = 2;

        /// 2: Give each triangle a different biome (3 zones)
        for (int i = 0; i < layer1.Mesh.Cells[cellIndex].Nodes.Count; i++)
        {
            var n = layer1.Mesh.Cells[cellIndex].Nodes[i];
            layer1.CellMetadata[n.Index] = new NodeMetadata(i + 1, colors[i], new int[] { }, RNG.NextFloat(5));
        }

        if (EnablePreview)
        {
            //layer1.Mesh.DrawMesh(transform, Color.clear, Color.grey);
        }

        layer1.Mesh.DrawMesh(transform, Color.grey, Color.clear,100f);

        #endregion

        //var bubb = 43;

        //var slayer2 = new CleverMesh(layer1, layer1.Mesh.Cells[cellIndex].Index);
        //var slayer2Triangle = new CleverMesh(layer1, layer1.Mesh.Cells[cellIndex].Index, MeshMasher.NestedMeshAccessType.Triangles);

        //slayer2.Mesh.DrawMesh(transform);

        //var slayer2 = new CleverMesh(layer1, layer1.Mesh.Cells[cellIndex].GetNeighbourhood());

        var slayer2 = new CleverMesh(layer1, layer1.Mesh.Cells.Select(x => x.Index).ToArray(),MeshMasher.NestedMeshAccessType.Triangles);
        //var slayer3 = new CleverMesh(slayer2, slayer2.Mesh.Cells.Select(x => x.Index).ToArray());


        //var slayer4 = new CleverMesh(slayer3, slayer3.Mesh.Cells.Select(x => x.Index).ToArray());
        //slayer2.Mesh.DrawMesh(transform, RNG.GetRandomColor(), Color.clear);
        //slayer3.Mesh.DrawMesh(transform, RNG.GetRandomColor(), Color.clear);
        //slayer4.Mesh.DrawMesh(transform, Color.red, Color.clear);
        //var layer5 = new CleverMesh(layer4, layer4.Mesh.Cells.Select(x => x.Index).ToArray(),1);
        //layer5.Mesh.DrawMesh(transform, RNG.GetRandomColor(), Color.clear);

        StartCoroutine(CreateBit(slayer2, 100));


        //var mate = new Material(Shader.Find("Standard"));
        //
        //var gameObjecte = new GameObject();
        //var fe = gameObjecte.AddComponent<MeshFilter>();
        //var re = gameObjecte.AddComponent<MeshRenderer>();
        //re.sharedMaterial = mate;
        ////f.mesh = layer5.Mesh.ToXYMesh();
        //fe.mesh = slayer3.Mesh.ToXYMesh();
        //fe.name = "Cell " + 1;

        return;

        ///Below we:
        /// 3: Create a boundary area that is a no-go zone.
        /// 2: Calculate a connectivity graph between regions (TODO: fix distance to be based on distance from node center based on layer 1)
        /// 3: Give each walkable node a special room code
        /// 4: TODO: Give each walkable node a connectivity map
        /// 5: TODO: Create mini-valleys using voronoi falloff where connectivity should be broken.
        /// 6: TODO: Define higher level biomes based on parent colour

        #region layer two

        var layer2 = new CleverMesh(layer1, layer1.Mesh.Cells[cellIndex].GetNeighbourhood());

        var layer2Border = layer2.Mesh.GetBorderNodes();
        var layer2State = layer2.Mesh.GenerateSemiConnectedMesh(5, layer2Border);

        var roomNumber = 1;

        //layer2.Mesh.DrawMesh(transform, Color.clear, Color.green);

        if (EnablePreview)
        {
            for (int i = 0; i < layer2Border.Nodes.Length; i++)
            {
                if (layer2Border.Nodes[i] == 1)
                    layer2.CellMetadata[i].Code = 0;
            }

            for (int i = 0; i < layer2Border.Lines.Length; i++)
            {
                if (layer2Border.Lines[i] == 1)
                    layer2.Mesh.Lines[i].DebugDraw(Color.green, 100f);
                else if (layer2State.Lines[i] == 1 &&
                    layer2.CellMetadata[layer2.Mesh.Lines[i].Nodes[0].Index].Code != 0 &&
                    layer2.CellMetadata[layer2.Mesh.Lines[i].Nodes[1].Index].Code != 0)
                {

                    layer2.Mesh.Lines[i].DebugDraw(Color.red, 100f);
                }
                else
                {
                    layer2.Mesh.Lines[i].DebugDraw(Color.white, 100f);
                }
            }

            for (int i = 0; i < layer2.CellMetadata.Length; i++)
            {
                if (layer2.CellMetadata[i].Code != 0)
                {
                    layer2.CellMetadata[i].Code = roomNumber;
                    roomNumber++;
                    //layer2.CellMetadata[i].SmoothColor = Color.white;
                }
                else
                {
                    //layer2.CellMetadata[i].Code = 0;
                    layer2.CellMetadata[i].SmoothColor = Color.black;
                }
            }
        }

        for (int i = 0; i < layer2.Mesh.Nodes.Count; i++)
        {
            var n = layer2.Mesh.Nodes[i];

            if (layer2Border.Nodes[n.Index] == 1 | layer2.CellMetadata[i].Code == 0)
            {                                   
                layer2.CellMetadata[n.Index].SmoothColor = Color.black;
                layer2.CellMetadata[n.Index].Code = 0;
            }

            else
            {
                //var colour = layer2.CellMetadata[n.Index].MeshDual;


                //layer2.CellMetadata[n.Index].SmoothColor = new Color(colour,colour,colour);
                layer2.CellMetadata[n.Index].Height += RNG.NextFloat(-0.5f, 0.5f);
                layer2.CellMetadata[n.Index].Connections = n
                    .Lines
                    .Where(x => layer2State.Lines[x.Index] == 1 )
                   // &&
                       // layer2.CellMetadata[layer2.Mesh.Lines[i].Nodes[0].Index].Code != 0 &&
                       // layer2.CellMetadata[layer2.Mesh.Lines[i].Nodes[1].Index].Code != 0)
                    .Select(x => x.GetOtherNode(n).Index + 1)
                    .Union(new List<int>() { i + 1 })
                    .ToArray();
                layer2.CellMetadata[n.Index].Code = i + 1;
            }
        }


        #endregion

        ///NO NEED TO GO BELOW HERE BUDDY, JUST FOCUS ON THE BIG PICTURE!
        ///NO NEED TO GO BELOW HERE BUDDY, JUST FOCUS ON THE BIG PICTURE!
        ///NO NEED TO GO BELOW HERE BUDDY, JUST FOCUS ON THE BIG PICTURE!
        ///NO NEED TO GO BELOW HERE BUDDY, JUST FOCUS ON THE BIG PICTURE!
        ///NO NEED TO GO BELOW HERE BUDDY, JUST FOCUS ON THE BIG PICTURE!

        ///Below we:
        /// 6: TODO: Identify key paths in and out of layer 3 regions
        /// 7: TODO: Define walkable paths, areas of interest
        /// 8: TODO: Define walkable/non walkable area for layer 4
        /// 9: TODO: Define tighter biomes, based more on creating variety at microscale

        #region layer three

        var layer3 = new CleverMesh(layer2, layer2.Mesh.Cells.Select(x => x.Index).ToArray());

        for (int i = 0; i < layer3.Mesh.Nodes.Count; i++)
        {
            var n = layer3.Mesh.Nodes[i];

            if (layer3.CellMetadata[n.Index].Code == 0)
            {
                //layer3.CellMetadata[n.Index].SmoothColor = Color.black;
            }
            else
            {
                var colour = layer3.CellMetadata[n.Index].MeshDual;
                colour = Mathf.Max(layer3.CellMetadata[n.Index].Distance, colour);// < 0.5f ? 0f : (colour);//*0.5f)+0.5f ;
                //colour = layer3.CellMetadata[n.Index].Distance < 0.5f ? 0f : 1f;//*0.5f)+0.5f ;

                layer3.CellMetadata[n.Index].Code = i + 1;
                //layer3.CellMetadata[n.Index].SmoothColor = layer3.CellMetadata[n.Index].Distance < 0.5f ? Color.black : Color.white;
                layer3.CellMetadata[n.Index].SmoothColor = new Color(colour, colour, colour);
                layer3.CellMetadata[n.Index].Height += RNG.NextFloat(-0.1f, 0.1f);
            }
        }

        //for (int i = 0; i < layer3.Mesh.Nodes.Count; i++)
        //{
        //   
        //}

        if (EnablePreview)
        {
            //layer3.Mesh.DrawMesh(transform, Color.clear, Color.grey);
        }

        #endregion

        ///Below we:
        /// 10: TODO: Actually start only generating this stuff based on distance
        /// 11: TODO: Build an actual heightmesh
        /// 11.5: TODO: Create Triangle-focused nestedmesh that creates seamless terrains
        /// 12: TODO: Instantiate large props

        #region layer four

        var mat = new Material(Shader.Find("Standard"));

        layer3.Mesh.DrawMesh(transform);

        for (int i = 0; i < layer3.Mesh.Cells.Count; i++)
        {
            var layer4 = new CleverMesh(layer3,new int[] { layer3.Mesh.Cells[i].Index });
            layer4.Mesh.DrawMesh(transform, RNG.GetRandomColor(), Color.clear);
            //var layer5 = new CleverMesh(layer4, layer4.Mesh.Cells.Select(x => x.Index).ToArray(),1);
            //layer5.Mesh.DrawMesh(transform, RNG.GetRandomColor(), Color.clear);

            var gameObject = new GameObject();
            var f = gameObject.AddComponent<MeshFilter>();
            var r = gameObject.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            //f.mesh = layer5.Mesh.ToXYMesh();
            f.mesh = layer4.Mesh.ToXYMesh();
            f.name = "Cell " + i;
        }

        return;

        //var layer4 = 
        //layer4.Mesh.DrawMesh(transform, Color.green, Color.red);

        #endregion

        ///Below we:
        /// 13: TODO: Instantiate smaller props

        #region create final mesh

        //var finalLayer = layer4;
        //var pts = finalLayer.Mesh.Nodes;
        //
        //for (int i = 0; i < pts.Count; i++)
        //{
        //    //if (layer4.CellMetadata[i].Code == 0 )//|| layer4.CellMetadata[i].Distance < 0.5)
        //    //    continue;
        //
        //    //var jitter = RNG.NextFloat(0.1f);
        //    //
        //    //var smoothVal = FalloffCurve.Evaluate(layer4.CellMetadata[i].SmoothColor.r+jitter);
        //    //
        //    //var color = Gradient.Evaluate(smoothVal);
        //    var n = finalLayer.Mesh.Nodes[i];
        //   // var colour = finalLayer.CellMetadata[n.Index].
        //
        //    var obj = Instantiate(InstantiationBase);
        //    //obj.GetComponent<MeshRenderer>().sharedMaterial = layer4.CellMetadata[i].Distance < 0.8 | layer4.CellMetadata[i].Code == 0 ? matA : matB;
        //    obj.GetComponent<MeshRenderer>().material.color = finalLayer.CellMetadata[i].SmoothColor;
        //    obj.transform.position = pts[i].Vert + Vector3.forward * finalLayer.CellMetadata[i].Height;
        //    //obj.transform.position = new Vector3(obj.transform.position.x, -obj.transform.position.z, obj.transform.position.y);
        //    obj.transform.localScale = Vector3.one * 0.06f;
        //    obj.name = "Room " + finalLayer.CellMetadata[i].Code;
        //}

        #endregion

    }

    IEnumerator CreateBit(CleverMesh mesh, int chunkSize)
    {
        var mate = new Material(Shader.Find("Standard"));

        var waitTime = new WaitForSeconds(0.1f);

        var current = 0;

        for (int i = 0; i < mesh.Mesh.Cells.Count; i++)
        {
            var slayer4 = new CleverMesh(mesh, mesh.Mesh.Cells[i].Index, MeshMasher.NestedMeshAccessType.Vertex);

            var gameObjecte = new GameObject();
            var fe = gameObjecte.AddComponent<MeshFilter>();
            var re = gameObjecte.AddComponent<MeshRenderer>();
            re.sharedMaterial = mate;
            //f.mesh = layer5.Mesh.ToXYMesh();
            fe.mesh = slayer4.Mesh.ToXYMesh();
            fe.name = "Cell " + i;

            current++;

            if (current >= chunkSize)
            {
                current = 0;
                yield return null;
            }

        }
    }
}