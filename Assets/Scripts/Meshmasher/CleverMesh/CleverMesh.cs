using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshMasher;

public class CleverMesh {

    public SmartMesh Mesh { get { return _sMesh; } }
    public NodeMetadata[] CellMetadata;

    private NestedMesh _nMesh;
    private SmartMesh _sMesh;

    //Create from scratch

    public CleverMesh(Vector2Int seedTile, string meshTileJSON):this(new List<Vector2Int>() { seedTile }, meshTileJSON){}

    public CleverMesh(List<Vector2Int> seedTiles,  string meshTileJSON)
    {
        Init(seedTiles, meshTileJSON);
    }

    //Create nested later

    public CleverMesh(CleverMesh parent, int accessIndex, NestedMeshAccessType type = NestedMeshAccessType.Vertex):this(parent,new int[] {accessIndex},type){}

    public CleverMesh(CleverMesh parent, int[] accessIndexes, NestedMeshAccessType type = NestedMeshAccessType.Vertex)
    {
        _nMesh = new NestedMesh(parent._nMesh, accessIndexes, type);
        CellMetadata = _nMesh.BlerpParentValues(parent.CellMetadata, parent._nMesh);
        _sMesh = new SmartMesh(_nMesh.CreateMesh());
    }

    private void Init(List<Vector2Int> seedTiles, string meshTileJSON)
    {
        _nMesh = new NestedMesh(seedTiles.ToArray(), meshTileJSON);
        _sMesh = new SmartMesh(_nMesh.CreateMesh());

        CellMetadata = new NodeMetadata[_nMesh.Verts.Length];

        for (int i = 0; i < CellMetadata.Length; i++)
        {
            CellMetadata[i] = new NodeMetadata(0, Color.black, new int[] { 0 });
        }
    }

    public Mesh GetBarycenterDebugMesh()
    {
        return _nMesh.CreateBaryDebugMesh();
    }

}