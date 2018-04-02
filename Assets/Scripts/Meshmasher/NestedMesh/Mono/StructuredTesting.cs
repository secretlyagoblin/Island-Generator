using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshMasher;
using System.Linq;

public class StructuredTesting : MonoBehaviour {

    public GameObject InstantiationBase;

	// Use this for initialization
	void Start () {

        RNG.DateTimeInit();

        var layer1 = new CleverMesh();
        //layer1.Mesh.DrawMesh(transform);

        var cellIndex = 126;

        //populate metadata
        var colors = new Color[] { Color.red, Color.green, Color.blue };

        for (int i = 0; i < layer1.Mesh.Cells[cellIndex].Nodes.Count; i++)
        {
            var n = layer1.Mesh.Cells[cellIndex].Nodes[i];
            layer1.CellMetadata[n.Index] = new NodeMetadata(i + 1, colors[i],new int[] { } ,RNG.NextFloat(5));
        }

        var layer2 = new CleverMesh(layer1, layer1.Mesh.Cells[cellIndex].GetNeighbourhood());

        var border = layer2.Mesh.GetBorderNodes();
        var state = layer2.Mesh.GenerateSemiConnectedMesh(5, border);
        
        for (int i = 0; i < layer2.Mesh.Nodes.Count; i++)
        {
            var n = layer2.Mesh.Nodes[i];

            if(border.Nodes[n.Index] == 1)
            {
                layer2.CellMetadata[n.Index].SmoothColor = Color.black;
                layer2.CellMetadata[n.Index].Code = 0;
            }
        
            else
            {
                layer2.CellMetadata[n.Index].Code = i + 1;
                layer2.CellMetadata[n.Index].SmoothColor = RNG.GetRandomColor();
                layer2.CellMetadata[n.Index].Height += RNG.NextFloat(-0.5f,0.5f);
                layer2.CellMetadata[n.Index].Connections = n.Lines.Where(x => state.Lines[x.Index] == 1).Select(x => x.GetOtherNode(n).Index+1).Union(new List<int>() {i+1}).ToArray();
            }
        }        

        var layer3 = new CleverMesh(layer2, layer2.Mesh.Cells.Select(x => x.Index).ToArray());

        layer2.Mesh.DrawMesh(transform, Color.blue, Color.white);

        var buff = layer3.Mesh.GetBorderNodes();

        for (int i = 0; i < layer3.Mesh.Lines.Count; i++)
        {
            if(buff.Lines[i] == 1)
            {
                layer3.Mesh.Lines[i].DebugDraw(Color.green, 100f);
            }
        }

        for (int i = 0; i < layer3.Mesh.Nodes.Count; i++)
        {
            var n = layer3.Mesh.Nodes[i];
        
            if (layer3.CellMetadata[n.Index].Code == 0)
            {
                layer3.CellMetadata[n.Index].SmoothColor = Color.black;
            }
            else
            {
                layer3.CellMetadata[n.Index].Code = i + 1;
                layer3.CellMetadata[n.Index].SmoothColor = layer3.CellMetadata[n.Index].Distance< 0.5f? Color.black:Color.white;
                layer3.CellMetadata[n.Index].Height += RNG.NextFloat(-0.1f, 0.1f);
            }
        }

        var layer4 = new CleverMesh(layer3, layer3.Mesh.Cells.Select(x => x.Index).ToArray());
        //layer2.Mesh.DrawMesh(transform, Color.black, Color.white);

        var funLayer = layer2;
        //var stuff = layer2.Mesh.GetBorderNodes();
        //
        //var state = funLayer.Mesh.MinimumSpanningTree(stuff);

                for (int i = 0; i < funLayer.Mesh.Lines.Count; i++)
                {
                    //if (stuff.Lines[funLayer.Mesh.Lines[i].Index] == 1)
                    //    funLayer.Mesh.Lines[i].DrawLine(Color.green, 100f, 0f);
                    //else 
        if (state.Lines[funLayer.Mesh.Lines[i].Index] == 1 && border.Lines[funLayer.Mesh.Lines[i].Index] == 1)
                        funLayer.Mesh.Lines[i].DrawLine(Color.white, 100f, 0f);
                }
                
                for (int i = 0; i < funLayer.Mesh.Cells.Count; i++)
                {
                    var c = funLayer.Mesh.Cells[i];
                
                    for (int u = 0; u < c.Lines.Count; u++)
                    {
                            if (state.Lines[c.Lines[u].Index] == 0)
                            {
                                var other = c.Lines[u].GetCellPartner(c);
                            if (other == null)
                                continue;
                
                                Debug.DrawLine(c.Center, other.Center, Color.red, 100f);
                            }
                    }
                }


        StateData wrapper = 6;
        StateData gupper = StateCode.excludedFromSet;

        var wrabbler = new StateData(StateCode.excludedFromSet);


            

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

            var obj = Instantiate(InstantiationBase);
            //obj.GetComponent<MeshRenderer>().sharedMaterial = layer4.CellMetadata[i].Distance < 0.5 | layer4.CellMetadata[i].Code == 0 ? matA : matB;
            obj.GetComponent<MeshRenderer>().material.color = layer4.CellMetadata[i].SmoothColor - new Color(jitter, jitter, jitter);
            obj.transform.position = pts[i].Vert+ Vector3.forward* layer4.CellMetadata[i].Height;
            //obj.transform.position = new Vector3(obj.transform.position.x, -obj.transform.position.z, obj.transform.position.y);
            obj.transform.localScale = Vector3.one * 0.06f;
            obj.name = "Room " + layer4.CellMetadata[i].Code;
        }    

    }
	
	// Update is called once per frame
	void Update () {
		
	}


}

public class CleverMesh {

    public SmartMesh Mesh { get { return _sMesh; } }
    public NodeMetadata[] CellMetadata;

    NestedMesh _nMesh;
    SmartMesh _sMesh;

    public CleverMesh()
    {
        var tiles = new List<Vector2Int>() { Vector2Int.zero,new Vector2Int(0,1), new Vector2Int(-1, 0) };
        _nMesh = new NestedMesh(tiles.ToArray());
        _sMesh = new SmartMesh(_nMesh.CreateMesh());

        CellMetadata = new NodeMetadata[_nMesh.Verts.Length];

        for (int i = 0; i < CellMetadata.Length; i++)
        {
            CellMetadata[i] = new NodeMetadata(0, Color.black, new int[] {0});
        } 
    }

    public CleverMesh(CleverMesh parent, int[] accessIndexes)
    {
        _nMesh = new NestedMesh(parent._nMesh, accessIndexes);
        CellMetadata = parent._nMesh.LerpBarycentricValues(parent.CellMetadata, accessIndexes);
        //Need to lerp in here somewhere
        _sMesh = new SmartMesh(_nMesh.CreateMesh());        
    }
}

public struct NodeMetadata : IBarycentricLerpable<NodeMetadata> {

    public int Code { get { return _roomCode.Value; } set { _roomCode.Value = value; _zoneBoundary.RoomCode = value; } }
    public Color SmoothColor { get { return _roomColor.Value; } set { _roomColor.Value = value; } }
    public float Height { get { return _height.Value; } set { _height.Value = value; } }
    public float Distance { get { return _zoneBoundary.Distance; } }
    public int[] Connections { get { return _zoneBoundary.Links; } set { _zoneBoundary.Links = value; } }

    MeshMasher.NodeDataTypes.RoomCode _roomCode;
    MeshMasher.NodeDataTypes.RoomColor _roomColor;
    MeshMasher.NodeDataTypes.RoomFloat _height;
    MeshMasher.NodeDataTypes.ZoneBoundary _zoneBoundary;

    public NodeMetadata(int roomCode, Color roomColor, int[] links, float height = 0f )
    {
        _roomCode = new MeshMasher.NodeDataTypes.RoomCode(roomCode);
        _roomColor = new MeshMasher.NodeDataTypes.RoomColor(roomColor);
        _height = new MeshMasher.NodeDataTypes.RoomFloat(height);
        _zoneBoundary = new MeshMasher.NodeDataTypes.ZoneBoundary(roomCode, 1, links);
    }

    public NodeMetadata Lerp(NodeMetadata a, NodeMetadata b, NodeMetadata c, Vector3 weight)
    {
        return new NodeMetadata()
        {
            _roomCode = _roomCode.Lerp(a._roomCode, b._roomCode, c._roomCode, weight),
            _roomColor = _roomColor.Lerp(a._roomColor, b._roomColor, c._roomColor, weight),
            _height = _height.Lerp(a._height, b._height, c._height, weight),
            _zoneBoundary = _zoneBoundary.Lerp(a._zoneBoundary,b._zoneBoundary,c._zoneBoundary,weight)
            
        };
    }
}

public struct StateData {

    private bool _stateIsInt;
    private StateCode _intHelper;
    private int _int;

    public StateData(StateCode value)
    {
        this = value;
    }

    public static implicit operator StateData(int input)
    {
        return new StateData() {
            _stateIsInt = true,
            _intHelper = StateCode.unassigned,
            _int = input
        };
    }

    public static implicit operator StateData(StateCode input)
    {
        return new StateData()
        {
            _stateIsInt = false,
            _intHelper = input,
            _int = -1
        };
    }

    public static bool operator ==(StateData a, StateData b) { return IsEqual(a,b); }
    public static bool operator !=(StateData a, StateData b) { return !IsEqual(a, b); }

    public static bool operator ==(StateData a, int b) { return IsEqual(a,b); }
    public static bool operator !=(StateData a, int b) { return !IsEqual(a, b); }

    public static bool operator ==(StateData a, StateCode b) { return IsEqual(a, b); }
    public static bool operator !=(StateData a, StateCode b) { return !IsEqual(a, b); }

    private static bool IsEqual(StateData a, StateData b)
    {
        if (a._stateIsInt != b._stateIsInt)
            return false;
        if(a._stateIsInt == true)
        {
            return a._int == b._int;
        }
        else
        {
            return a._intHelper == b._intHelper;
        }
    }

    private static bool IsEqual(StateData a, int b)
    {
        if (a._stateIsInt != true)
            return false;

        return a._int == b;        
    }

    private static bool IsEqual(StateData a, StateCode b)
    {
        if (a._stateIsInt == true)
            return false;

        return a._intHelper == b;
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

}
public enum StateCode {
    unassigned = 0, excludedFromSet
}
