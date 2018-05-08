using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StructureV2 : MonoBehaviour {


    public GameObject InstantiationBase;
    public AnimationCurve FalloffCurve;
    public Gradient Gradient;

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
        var layer1 = new CleverMesh();
        var cellIndex = 126;

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

        #endregion

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


        for (int i = 0; i < layer2Border.Nodes.Length; i++)
        {
            if (layer2Border.Nodes[i] == 1)
                layer2.CellMetadata[i].Code = 0;
        }

        for (int i = 0; i < layer2Border.Lines.Length; i++)
        {
            if(layer2Border.Lines[i] == 1)
                layer2.Mesh.Lines[i].DebugDraw(Color.green, 100f);
            else if (layer2State.Lines[i] == 1 &&
                layer2.CellMetadata[layer2.Mesh.Lines[i].Nodes[0].Index].Code!=0 && 
                layer2.CellMetadata[layer2.Mesh.Lines[i].Nodes[1].Index].Code !=0)                
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
            if(layer2.CellMetadata[i].Code != 0)
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

        //for (int i = 0; i < layer3.Mesh.Nodes.Count; i++)
        //{
        //   
        //}

        if (EnablePreview)
        {
            layer3.Mesh.DrawMesh(transform, Color.clear, Color.grey);
        }

        #endregion

        ///Below we:
        /// 10: TODO: Actually start only generating this stuff based on distance
        /// 11: TODO: Build an actual heightmesh
        /// 12: TODO: Instantiate large props

        #region layer four

        var layer4 = new CleverMesh(layer3, layer3.Mesh.Cells.Select(x => x.Index).ToArray());

        #endregion

        ///Below we:
        /// 13: TODO: Instantiate smaller props

        #region create final mesh

        var pts = layer4.Mesh.Nodes;

        for (int i = 0; i < pts.Count; i++)
        {
            //if (layer4.CellMetadata[i].Code == 0 )//|| layer4.CellMetadata[i].Distance < 0.5)
            //    continue;

            //var jitter = RNG.NextFloat(0.1f);
            //
            //var smoothVal = FalloffCurve.Evaluate(layer4.CellMetadata[i].SmoothColor.r+jitter);
            //
            //var color = Gradient.Evaluate(smoothVal);


            var obj = Instantiate(InstantiationBase);
            //obj.GetComponent<MeshRenderer>().sharedMaterial = layer4.CellMetadata[i].Distance < 0.8 | layer4.CellMetadata[i].Code == 0 ? matA : matB;
            obj.GetComponent<MeshRenderer>().material.color = layer4.CellMetadata[i].SmoothColor;
            obj.transform.position = pts[i].Vert + Vector3.forward * layer4.CellMetadata[i].Height;
            //obj.transform.position = new Vector3(obj.transform.position.x, -obj.transform.position.z, obj.transform.position.y);
            obj.transform.localScale = Vector3.one * 0.06f;
            obj.name = "Room " + layer4.CellMetadata[i].Code;
        }

        #endregion

    }
}