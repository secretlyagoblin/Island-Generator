using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshMasher.NodeData.Types;

namespace MeshMasher {

    public struct NodeMetadata : IBlerpable<NodeMetadata> {

        public int Id;
        public int Code { get { return _roomCode; } set { _roomCode = value; _zoneBoundary.RoomCode = value; } }
        public Color SmoothColor { get { return _roomColor; } set { _roomColor = value; } }
        public float Height { get { return _height; } set { _height = value; } }
        public bool IsTrueBoundary { get { return _zoneBoundary.IsEdgeNode; } }
        public int[] Connections { get { return _zoneBoundary.Links; } set { _zoneBoundary.Links = value; } }
        public float MeshDual { get { return _meshDual.Value; } set { _meshDual.Value = value; } }
        public bool IsFuzzyBoundary { get { return _cliffData.FuzzyBoundary; } set { _cliffData.FuzzyBoundary = value; } }
        public float CliffDistance { get { return _cliffData.Distance; } set { _cliffData.Distance = value; } }


        int _roomCode;
        Color _roomColor;
        float _height;
        ZoneBoundary _zoneBoundary;
        MeshDual _meshDual;
        CliffData _cliffData;

        public NodeMetadata(int roomCode, Color roomColor, int[] links, float height = 0f)
        {
            Id = 0;
            _roomCode = roomCode;
            _roomColor = roomColor;
            _height = height;
            _zoneBoundary = new ZoneBoundary(roomCode, true, links);
            _meshDual = new MeshDual(0);
            _cliffData = new CliffData(0, true);
        }

        public NodeMetadata Blerp(NodeMetadata a, NodeMetadata b, NodeMetadata c, Barycenter weight)
        {
            return new NodeMetadata()
            {
                _roomCode = _roomCode.Blerp(a._roomCode, b._roomCode, c._roomCode, weight),
                _roomColor = _roomColor.Blerp(a._roomColor, b._roomColor, c._roomColor, weight),
                _height = _height.Blerp(a._height, b._height, c._height, weight),
                _zoneBoundary = _zoneBoundary.Blerp(a._zoneBoundary, b._zoneBoundary, c._zoneBoundary, weight),
                _meshDual = _meshDual.Blerp(a._meshDual, b._meshDual, c._meshDual, weight),
                _cliffData = _cliffData.Blerp(a._cliffData, b._cliffData, c._cliffData, weight),
                Id = Id.Blerp(a.Id, b.Id, c.Id, weight)
            };
        }
    }
}