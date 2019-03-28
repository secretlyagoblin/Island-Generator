using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshMasher;
using System.Linq;
using MeshMasher.NodeData;

public class CleverMesh {

    public SmartMesh Mesh { get { return _sMesh; } }
    public SmartMesh RingMesh;
    public NodeMetadata[] NodeMetadata;
    //public NodeMetadata[] CellMetadata; // this is readonly and can cause issues if it is set directly

    public NodeMetadata[] RingNodeMetadata;

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

        NodeMetadata = _nMesh.BlerpParentNodeValues(parent.NodeMetadata, parent.RingNodeMetadata, parent._nMesh);

        if (type == NestedMeshAccessType.Vertex)
        {
            RingNodeMetadata = _nMesh.BlerpRingNodeValues(parent.NodeMetadata, parent.RingNodeMetadata, parent._nMesh);

            RingMesh = new SmartMesh(_nMesh.RingVerts, _nMesh.RingTris);
        }

        _sMesh = new SmartMesh(_nMesh.Verts,_nMesh.Tris);
        
    }

    public CleverMesh(CleverMesh parent) : this(parent, parent.Mesh.Nodes.ConvertAll(x => x.Index).ToArray()) { }

    private void Init(List<Vector2Int> seedTiles, MeshTile meshTileJSON)
    {
        _nMesh = new NestedMesh(seedTiles.ToArray(), meshTileJSON);
        _sMesh = new SmartMesh(_nMesh.Verts, _nMesh.Tris);

        NodeMetadata = new NodeMetadata[_nMesh.Verts.Length];
        RingNodeMetadata = new NodeMetadata[0];

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