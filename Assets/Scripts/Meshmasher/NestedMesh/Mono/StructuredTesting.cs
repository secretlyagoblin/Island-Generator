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

        //populate metadata
        var colors = new Color[] { Color.red, Color.green, Color.blue };

        for (int i = 0; i < layer1.Mesh.Cells[44].Nodes.Count; i++)
        {
            var n = layer1.Mesh.Cells[44].Nodes[i];
            layer1.CellMetadata[n.Index] = new NodeMetadata(i + 1, colors[i], RNG.NextFloat(5));
        }

        var layer2 = new CleverMesh(layer1, layer1.Mesh.Cells[44].GetNeighbourhood());        

        for (int i = 0; i < layer2.Mesh.Nodes.Count; i++)
        {
            var n = layer2.Mesh.Nodes[i];

            if (layer2.CellMetadata[n.Index].Code == 0)
            {
                layer2.CellMetadata[n.Index].SmoothColor = Color.black;
            }
            else
            {
                layer2.CellMetadata[n.Index].Code = i + 1;
                layer2.CellMetadata[n.Index].SmoothColor = RNG.GetRandomColor();
                layer2.CellMetadata[n.Index].Height += RNG.NextFloat(-0.5f,0.5f);
            }
        }

        var layer3 = new CleverMesh(layer2, layer2.Mesh.Cells.Select(x => x.Index).ToArray());

        return;

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
                layer3.CellMetadata[n.Index].SmoothColor = RNG.GetRandomColor();
                layer3.CellMetadata[n.Index].Height += RNG.NextFloat(-0.1f, 0.1f);
            }
        }

        var layer4 = new CleverMesh(layer3, layer3.Mesh.Cells.Select(x => x.Index).ToArray());

        var pts = layer4.Mesh.Nodes;



        for (int i = 0; i < pts.Count; i++)
        {
            if (layer4.CellMetadata[i].Code == 0)
                continue;

            var obj = Instantiate(InstantiationBase);
            obj.GetComponent<MeshRenderer>().material.color = layer4.CellMetadata[i].SmoothColor;
            obj.transform.position = pts[i].Vert+ Vector3.forward*layer4.CellMetadata[i].Height;
            obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.z, obj.transform.position.y);
            obj.transform.localScale = Vector3.one * 0.05f;
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
        var tiles = new List<Vector2Int>() { Vector2Int.zero };
        _nMesh = new NestedMesh(tiles.ToArray());
        _sMesh = new SmartMesh(_nMesh.CreateMesh());

        CellMetadata = new NodeMetadata[_nMesh.Verts.Length];

        for (int i = 0; i < CellMetadata.Length; i++)
        {
            CellMetadata[i] = new NodeMetadata(0, Color.black);
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

    public int Code { get { return _roomCode.Value; } set { _roomCode.Value = value; } }
    public Color SmoothColor { get { return _roomColor.Value; } set { _roomColor.Value = value; } }
    public float Height { get { return _height.Value; } set { _height.Value = value; } }

    MeshMasher.NodeDataTypes.RoomCode _roomCode;
    MeshMasher.NodeDataTypes.RoomColor _roomColor;
    MeshMasher.NodeDataTypes.RoomFloat _height;

    public NodeMetadata(int roomCode, Color roomColor, float height = 0f)
    {
        _roomCode = new MeshMasher.NodeDataTypes.RoomCode(roomCode);
        _roomColor = new MeshMasher.NodeDataTypes.RoomColor(roomColor);
        _height = new MeshMasher.NodeDataTypes.RoomFloat(height);
    }

    public NodeMetadata Lerp(NodeMetadata a, NodeMetadata b, NodeMetadata c, Vector3 weight)
    {
        return new NodeMetadata()
        {
            _roomCode = _roomCode.Lerp(a._roomCode, b._roomCode, c._roomCode, weight),
            _roomColor = _roomColor.Lerp(a._roomColor, b._roomColor, c._roomColor, weight),
            _height = _height.Lerp(a._height, b._height, c._height, weight)
        };
    }
}
