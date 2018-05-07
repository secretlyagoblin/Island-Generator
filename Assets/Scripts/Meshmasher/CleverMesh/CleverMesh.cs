﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshMasher;

public class CleverMesh {

    public SmartMesh Mesh { get { return _sMesh; } }
    public NodeMetadata[] CellMetadata;

    NestedMesh _nMesh;
    SmartMesh _sMesh;

    public CleverMesh()
    {
        var tiles = new List<Vector2Int>() { Vector2Int.zero, new Vector2Int(0, 1), new Vector2Int(-1, 0) };
        _nMesh = new NestedMesh(tiles.ToArray());
        _sMesh = new SmartMesh(_nMesh.CreateMesh());

        CellMetadata = new NodeMetadata[_nMesh.Verts.Length];

        for (int i = 0; i < CellMetadata.Length; i++)
        {
            CellMetadata[i] = new NodeMetadata(0, Color.black, new int[] { 0 });
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