﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshMasher;

public struct NodeMetadata : IBlerpable<NodeMetadata> {

    public int Code { get { return _roomCode.Value; } set { _roomCode.Value = value; _zoneBoundary.RoomCode = value; } }
    public Color SmoothColor { get { return _roomColor.Value; } set { _roomColor.Value = value; } }
    public float Height { get { return _height.Value; } set { _height.Value = value; } }
    public float Distance { get { return _zoneBoundary.Distance; } }
    public int[] Connections { get { return _zoneBoundary.Links; } set { _zoneBoundary.Links = value; } }
    public float MeshDual { get { return _meshDual.Value; } set { _meshDual.Value = value; } }
    public bool Walkable { get { return _cliffData.Walkable.Value; } set { _cliffData.Walkable.Value = value; } }
    public float CliffDistance { get { return _cliffData.Distance.Value; } set { _cliffData.Distance.Value = value; } }


    MeshMasher.NodeDataTypes.RoomInt _roomCode;
    MeshMasher.NodeDataTypes.RoomColor _roomColor;
    MeshMasher.NodeDataTypes.RoomFloat _height;
    MeshMasher.NodeDataTypes.ZoneBoundary _zoneBoundary;
    MeshMasher.NodeDataTypes.MeshDual _meshDual;
    MeshMasher.NodeDataTypes.CliffData _cliffData;

    public NodeMetadata(int roomCode, Color roomColor, int[] links, float height = 0f)
    {
        _roomCode = new MeshMasher.NodeDataTypes.RoomInt(roomCode);
        _roomColor = new MeshMasher.NodeDataTypes.RoomColor(roomColor);
        _height = new MeshMasher.NodeDataTypes.RoomFloat(height);
        _zoneBoundary = new MeshMasher.NodeDataTypes.ZoneBoundary(roomCode, 1, links);
        _meshDual = new MeshMasher.NodeDataTypes.MeshDual(0);
        _cliffData = new MeshMasher.NodeDataTypes.CliffData(0, true);
    }

    public NodeMetadata Blerp(NodeMetadata a, NodeMetadata b, NodeMetadata c, Barycenter weight)
    {
        return new NodeMetadata()
        {
            _roomCode = _roomCode.Blerp(a._roomCode, b._roomCode, c._roomCode, weight),
            _roomColor = _roomColor.Blerp(a._roomColor, b._roomColor, c._roomColor, weight),
            _height = _height.Blerp(a._height, b._height, c._height, weight),
            _zoneBoundary = _zoneBoundary.Blerp(a._zoneBoundary, b._zoneBoundary, c._zoneBoundary, weight),
            _meshDual = _meshDual.Blerp(a._meshDual, b._meshDual, c._meshDual, weight)
        };
    }
}