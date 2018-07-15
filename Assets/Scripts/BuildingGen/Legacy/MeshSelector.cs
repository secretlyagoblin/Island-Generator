using UnityEngine;
using System.Collections.Generic;
using MeshMasher;
using Nurbz;
using System.Linq;

public class MeshSelector : MonoBehaviour {

    public Camera Camera;
    SmartMesh _mesh;
    MeshState<int> _state;

    // Use this for initialization
    void Start () {

        var mesh = GetComponent<MeshFilter>().sharedMesh;

        _mesh = new SmartMesh(mesh.vertices,mesh.triangles);
        _state = _mesh.GetMeshState<int>();

        for (int i = 0; i < _state.Cells.Length; i++)
        {
            _state.Cells[i] = 1;
        }	
	}

    // Update is called once per frame

    int _triangleIndex;
    bool _toggleVis = true;

	void Update () {

        MouseOver();

        if (Input.GetMouseButtonDown(0))
            MouseClick();

        if (Input.GetMouseButtonDown(1))
        {
            MouseRightClick();
        }

        if (Input.GetMouseButtonDown(2))
            _toggleVis = _toggleVis ? false : true;

        if (_toggleVis)
            HighlightCells();






    }

    void MouseOver() {

        RaycastHit hit;
        if (!Physics.Raycast(Camera.ScreenPointToRay(Input.mousePosition), out hit))
            return;

        MeshCollider meshCollider = hit.collider as MeshCollider;
        if (meshCollider.transform != transform)
            return;

        //Debug.Log("returning");

        var tri = _mesh.Cells[hit.triangleIndex];

        Transform hitTransform = hit.collider.transform;
        var p0 = hitTransform.TransformPoint(tri.Nodes[0].Vert);
        var p1 = hitTransform.TransformPoint(tri.Nodes[1].Vert);
        var p2 = hitTransform.TransformPoint(tri.Nodes[2].Vert);
        Debug.DrawLine(p0, p1,Color.blue);
        Debug.DrawLine(p1, p2,Color.blue);
        Debug.DrawLine(p2, p0,Color.blue);

        _triangleIndex = hit.triangleIndex;
    }

    void MouseClick() {

        _state.Cells[_triangleIndex] = 2;

    }

    void MouseRightClick()
    {
        var state = _mesh.SetCellGroups(_state);

        var distinctRooms = state.Lines.Distinct().ToList();


        foreach (var room in distinctRooms)
        {
            var boundingLines = _mesh.GetCellBoundingPolyLines(state, room);

            Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

            //Debug, got a bit muddled here

            //for (int i = 0; i < distinctRooms.Count; i++)
            //{
            //    state.Cells.Where()
            //}      
            //
            //foreach (var bo in room)
            //    foreach (var neigh in bo.Neighbours)
            //        if (bo.Room == neigh.Room)
            //            Debug.DrawLine(bo.Center, neigh.Center, color, 4f);


            foreach (var line in boundingLines)
            {
                Polyline3 polyline = new Polyline3(SmartMesh.NodeArrayToVector3Array(line));

                if (polyline.GetNormal() != Vector3.up)
                {
                    polyline.Flip();
                }

                polyline = polyline.OffsetInPlane(0.1f);

                polyline.Debugdraw(color, 4f);
            }



        }
    }

    void HighlightCells()
    {
        foreach(var tri in _mesh.Cells)
        {
            if(_state.Cells[tri.Index] == 2)
            {
                Debug.DrawLine(tri.Nodes[0].Vert, tri.Nodes[1].Vert, Color.red);
                Debug.DrawLine(tri.Nodes[1].Vert, tri.Nodes[2].Vert, Color.red);
                Debug.DrawLine(tri.Nodes[2].Vert, tri.Nodes[0].Vert, Color.red);
            }
        }

    }
}
