using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshMasher;

public struct NodeMetadata : IBarycentricLerpable<NodeMetadata> {

    public int Code { get { return _roomCode.Value; } set { _roomCode.Value = value; _zoneBoundary.RoomCode = value; } }
    public Color SmoothColor { get { return _roomColor.Value; } set { _roomColor.Value = value; } }
    public float Height { get { return _height.Value; } set { _height.Value = value; } }
    public float Distance { get { return _zoneBoundary.Distance; } }
    public int[] Connections { get { return _zoneBoundary.Links; } set { _zoneBoundary.Links = value; } }
    public float MeshDual { get { return _meshDual.Value; } set { _meshDual.Value = value; } }

    MeshMasher.NodeDataTypes.RoomCode _roomCode;
    MeshMasher.NodeDataTypes.RoomColor _roomColor;
    MeshMasher.NodeDataTypes.RoomFloat _height;
    MeshMasher.NodeDataTypes.ZoneBoundary _zoneBoundary;
    MeshMasher.NodeDataTypes.MeshDual _meshDual;

    public NodeMetadata(int roomCode, Color roomColor, int[] links, float height = 0f)
    {
        _roomCode = new MeshMasher.NodeDataTypes.RoomCode(roomCode);
        _roomColor = new MeshMasher.NodeDataTypes.RoomColor(roomColor);
        _height = new MeshMasher.NodeDataTypes.RoomFloat(height);
        _zoneBoundary = new MeshMasher.NodeDataTypes.ZoneBoundary(roomCode, 1, links);
        _meshDual = new MeshMasher.NodeDataTypes.MeshDual(0);
    }

    public NodeMetadata Lerp(NodeMetadata a, NodeMetadata b, NodeMetadata c, Vector3 weight)
    {
        return new NodeMetadata()
        {
            _roomCode = _roomCode.Lerp(a._roomCode, b._roomCode, c._roomCode, weight),
            _roomColor = _roomColor.Lerp(a._roomColor, b._roomColor, c._roomColor, weight),
            _height = _height.Lerp(a._height, b._height, c._height, weight),
            _zoneBoundary = _zoneBoundary.Lerp(a._zoneBoundary, b._zoneBoundary, c._zoneBoundary, weight),
            _meshDual = _meshDual.Lerp(a._meshDual, b._meshDual, c._meshDual, weight)
        };
    }
}