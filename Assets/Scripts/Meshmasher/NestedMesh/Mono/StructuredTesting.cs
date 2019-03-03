using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshMasher;
using System.Linq;

public class StructuredTesting : MonoBehaviour {

    public GameObject InstantiationBase;
    public AnimationCurve FalloffCurve;
    public Gradient Gradient;
    public TextAsset MeshTileData;

	// Use this for initialization
	void Start () {

        RNG.DateTimeInit();

        var layer1 = new CleverMesh(new List<Vector2Int>() {Vector2Int.zero}, new MeshTile(MeshTileData.text));
        layer1.Mesh.DrawMesh(transform,Color.clear,Color.grey);

        var cellIndex = 126;

        //populate metadata
        var colors = new Color[] { Color.red, Color.green, Color.blue };

        for (int i = 0; i < layer1.Mesh.Cells[cellIndex].Nodes.Count; i++)
        {
            var n = layer1.Mesh.Cells[cellIndex].Nodes[i];
            layer1.NodeMetadata[n.Index] = new NodeMetadata(i + 1, colors[i],new int[] { } ,RNG.NextFloat(5));
        }

        var layer2 = new CleverMesh(layer1, layer1.Mesh.Cells[cellIndex].GetNeighbourhood());

        var border = layer2.Mesh.GetBorderNodes();
        //var state = layer2.Mesh.GenerateSemiConnectedMesh(5, border);
        
        //for (int i = 0; i < layer2.Mesh.Nodes.Count; i++)
        //{
        //    var n = layer2.Mesh.Nodes[i];
        //
        //    var color = layer2.CellMetadata[n.Index].MeshDual;
        //
        //    layer2.CellMetadata[n.Index].SmoothColor = new Color(color, color, color);
        //
        //    //if(border.Nodes[n.Index] == 1)
        //    //{
        //    //    layer2.CellMetadata[n.Index].SmoothColor = Color.black;
        //    //    layer2.CellMetadata[n.Index].Code = 0;
        //    //}
        //    //
        //    //else
        //    //{
        //    //    layer2.CellMetadata[n.Index].Code = i + 1;
        //    //    layer2.CellMetadata[n.Index].SmoothColor = RNG.GetRandomColor();
        //    //    layer2.CellMetadata[n.Index].Height += RNG.NextFloat(-0.5f,0.5f);
        //    //    layer2.CellMetadata[n.Index].Connections = n.Lines.Where(x => state.Lines[x.Index] == 1).Select(x => x.GetOtherNode(n).Index+1).Union(new List<int>() {i+1}).ToArray();
        //    //}
        //}        

        var layer3 = new CleverMesh(layer2, layer2.Mesh.Cells.Select(x => x.Index).ToArray());

        //layer2.Mesh.DrawMesh(transform, Color.blue, Color.gray);



        //for (int i = 0; i < layer2.Mesh.Lines.Count; i++)
        //{
        //    if(stuff.Lines[i] == 0)
        //    {
        //        layer2.Mesh.Lines[i].DebugDraw(Color.green, 100f);
        //    }
        //}


        for (int i = 0; i < layer3.Mesh.Nodes.Count; i++)
        {
            var n = layer3.Mesh.Nodes[i];
        
            var color = layer3.NodeMetadata[n.Index].MeshDual;
        
            color = Mathf.Clamp01((color * 1.5f) - 0.25f);
        
            layer3.NodeMetadata[n.Index].SmoothColor = new Color(color, color, color);
        
            //if (layer3.CellMetadata[n.Index].Code == 0)
            //{
            //    layer3.CellMetadata[n.Index].SmoothColor = Color.black;
            //}
            //else
            //{
            //    layer3.CellMetadata[n.Index].Code = i + 1;
            //    layer3.CellMetadata[n.Index].SmoothColor = layer3.CellMetadata[n.Index].Distance< 0.5f? Color.black:Color.white;
            //    layer3.CellMetadata[n.Index].Height += RNG.NextFloat(-0.1f, 0.1f);
            //}
        }

        var layer4 = new CleverMesh(layer3, layer3.Mesh.Cells.Select(x => x.Index).ToArray());
        //layer2.Mesh.DrawMesh(transform, Color.black, Color.white);

        var funLayer = layer2;
        //var stuff = layer2.Mesh.GetBorderNodes();
        //
        //var state = funLayer.Mesh.MinimumSpanningTree(stuff);

        //        for (int i = 0; i < funLayer.Mesh.Lines.Count; i++)
        //        {
        //            //if (stuff.Lines[funLayer.Mesh.Lines[i].Index] == 1)
        //            //    funLayer.Mesh.Lines[i].DrawLine(Color.green, 100f, 0f);
        //            //else 
        //if (state.Lines[funLayer.Mesh.Lines[i].Index] == 1)
        //                funLayer.Mesh.Lines[i].DrawLine(Color.white, 100f, 0f);
        //        }
        //        
        //        for (int i = 0; i < funLayer.Mesh.Cells.Count; i++)
        //        {
        //            var c = funLayer.Mesh.Cells[i];
        //        
        //            for (int u = 0; u < c.Lines.Count; u++)
        //            {
        //                    if (state.Lines[c.Lines[u].Index] == 0)
        //                    {
        //                        var other = c.Lines[u].GetCellPartner(c);
        //                    if (other == null)
        //                        continue;
        //        
        //                        Debug.DrawLine(c.Center, other.Center, Color.red, 100f);
        //                    }
        //            }
        //        }

        //layer2.Mesh.DrawMesh(transform,Color.white,Color.blue);

        var pts = layer4.Mesh.Nodes;

        var matA = new Material(Shader.Find("Standard"));
        var matB = new Material(Shader.Find("Standard"));

        matA.color = Color.red;
        matB.color = Color.green;

        for (int i = 0; i < pts.Count; i++)
        {
           //if (layer4.CellMetadata[i].Code == 0 )//|| layer4.CellMetadata[i].Distance < 0.5)
           //    continue;

            var jitter = RNG.NextFloat(0.2f);

            var smoothVal = FalloffCurve.Evaluate(layer4.NodeMetadata[i].SmoothColor.r);

            var color = Gradient.Evaluate(smoothVal);


            var obj = Instantiate(InstantiationBase);
            //obj.GetComponent<MeshRenderer>().sharedMaterial = layer4.CellMetadata[i].Distance < 0.8 | layer4.CellMetadata[i].Code == 0 ? matA : matB;
            obj.GetComponent<MeshRenderer>().material.color = color;
            obj.transform.position = pts[i].Vert+ Vector3.forward* layer4.NodeMetadata[i].Height;
            //obj.transform.position = new Vector3(obj.transform.position.x, -obj.transform.position.z, obj.transform.position.y);
            obj.transform.localScale = Vector3.one * 0.06f;
            obj.name = "Room " + layer4.NodeMetadata[i].Code;
        }    

    }

}




