using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshMasher;
using System.Linq;

public class CleverMesh {

    public SmartMesh Mesh { get { return _sMesh; } }
    public SmartMesh RingMesh;
    public NodeMetadata[] NodeMetadata;
    //public NodeMetadata[] CellMetadata; // this is readonly and can cause issues if it is set directly

    private NodeMetadata[] _ringNodeMetadata;

    private NestedMesh _nMesh;
    private SmartMesh _sMesh;

    private int[] _widerNeighbourhoodRemap;

    //Create from scratch

    public CleverMesh(List<Vector2Int> seedTiles, MeshTile meshTile)
    {
        Init(seedTiles, meshTile);
    }

    public CleverMesh(Vector2Int seedTile, MeshTile meshTile) :this(new List<Vector2Int>() { seedTile }, meshTile){}

    //Create nested later

    public CleverMesh(CleverMesh parent, int accessIndex, NestedMeshAccessType type = NestedMeshAccessType.Vertex):this(parent,new int[] {accessIndex},type){}

    public CleverMesh(CleverMesh parent, int[] accessIndexes, NestedMeshAccessType type = NestedMeshAccessType.Vertex)
    {

        _nMesh = new NestedMesh(parent._nMesh, accessIndexes, type);

        if (type == NestedMeshAccessType.Vertex)
        {

            NodeMetadata = _nMesh.BlerpParentNodeValues(parent.NodeMetadata, parent._ringNodeMetadata, parent._nMesh);

            //try
            //{
            _ringNodeMetadata = _nMesh.BlerpRingNodeValues(parent.NodeMetadata, parent._ringNodeMetadata, parent._nMesh);
            //}
            //catch
            //{
            //    throw new System.Exception("Failed to blerp ring metadata");
            //}


            //convinced at this point that it's safe to keep this commented out

            //switch (type)
            //{
            //    case NestedMeshAccessType.Vertex:
            //            CellMetadata = _nMesh.BlerpNodeToCellValuesUsingDerivedCenter(NodeMetadata);
            //
            //        break;
            //    case NestedMeshAccessType.Triangles:
            //
            //            CellMetadata = _nMesh.BlerpNodeToCellValuesUsingParentCenter(parent.NodeMetadata, parent._nMesh);
            //        break;
            //}
            RingMesh = new SmartMesh(_nMesh.RingVerts, _nMesh.RingTris);
        }

        _sMesh = new SmartMesh(_nMesh.Verts,_nMesh.Tris);
        
    }

    private void Init(List<Vector2Int> seedTiles, MeshTile meshTileJSON)
    {
        _nMesh = new NestedMesh(seedTiles.ToArray(), meshTileJSON);
        _sMesh = new SmartMesh(_nMesh.Verts, _nMesh.Tris);

        NodeMetadata = new NodeMetadata[_nMesh.Verts.Length];
        _ringNodeMetadata = new NodeMetadata[0];

        for (int i = 0; i < NodeMetadata.Length; i++)
        {
            NodeMetadata[i] = new NodeMetadata(0, Color.black, new int[] { 0 });
        }

        //CellMetadata = new NodeMetadata[_nMesh.Tris.Length];
        //
        //for (int i = 0; i < CellMetadata.Length; i++)
        //{
        //    CellMetadata[i] = new NodeMetadata(0, Color.black, new int[] { 0 });
        //}
    }

    public Mesh GetBarycenterDebugMesh()
    {
        return _nMesh.CreateBaryDebugMesh();
    }

    public float[] GetDataAboutVertex(int index)
    {
        return _nMesh.GetDataAtIndex(index);
    }
}