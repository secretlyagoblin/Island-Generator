using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldGen;
using System.Linq;

public class WorldMesh {

    Transform _transform;
    Bounds _bounds;
    List<Region> _regions;
    Dictionary<Region, int> _regionVertexIndex = new Dictionary<Region, int>();
    MeshMasher.SmartMesh _mesh;

    public WorldMesh(List<Region> regions, Transform root)
    {
        _regions = regions;
        _transform = root;
        ReorientToXZPlaneAndUpdateBounds();
    }

    public void Generate()
    {
        CreateSmartMesh();
        CalculateClosestNodes();
        CalculateConnectivity();
    }

    void ReorientToXZPlaneAndUpdateBounds()
    {
        _bounds = new Bounds(_regions[0].XZPos, Vector3.zero);

        for (int i = 0; i < _regions.Count; i++)
        {
                _bounds.Encapsulate(_regions[i].XZPos);            
        }

        _bounds.size = _bounds.size.x > _bounds.size.z ? new Vector3(_bounds.size.x, 0, _bounds.size.x) : new Vector3(_bounds.size.z, 0, _bounds.size.z);
        _bounds.size += (Vector3.one * _bounds.size.magnitude * 0.15f);
    }

    void CreateSmartMesh()
    {
        _mesh = MeshMasher.DelaunayGen.FromBounds(_bounds, 0.015f);
       // _mesh.DrawMesh(_transform);
        _mesh.SetCustomBuckets(10, 1, 10);
    }

    void CalculateClosestNodes()
    {
        for (int i = 0; i < _regions.Count; i++)
        {
            _regionVertexIndex.Add(_regions[i], _mesh.ClosestIndex(_regions[i].XZPos));
        }
    }

    void CalculateConnectivity() { 

        var connectivity = MapGraphToShortestWalk();

        var connected = new bool[_mesh.Nodes.Count];

        for (int i = 0; i < connectivity.Length; i++)
        {
            connected[connectivity[i]] = true;
        }

        for (int i = 0; i < _mesh.Lines.Count; i++)
        {
            var l = _mesh.Lines[i];
            if (connected[l.Nodes[0].Index] == true && connected[l.Nodes[1].Index] == true)
            {
                l.DrawLine(Color.white, 100f,0.1f);
            }
        }

        var rooms = ApplyWeightsToNodes();
    }

    int[] MapGraphToShortestWalk()
    {
        var nodes = new List<int>();

        for (int i = 0; i < _regions.Count; i++)
        {
            var r = _regions[i];
            var a = _regionVertexIndex[r];

            for (int u = 0; u < r.Regions.Count; u++)
            {
                var b = _regionVertexIndex[r.Regions[u]];
                var realNodes = _mesh.ShortestWalkNode(a, b);
                nodes.AddRange(realNodes);
            }
        }

        var distinct = nodes.Distinct().ToArray();

        //for (int u = 0; u < distinct.Length; u++)
        //{
        //    Debug.DrawRay(_mesh.Nodes[distinct[u]].Vert, Vector3.up, Color.blue, 100f);
        //}

        return distinct;

    }

    MeshMasher.MeshState ApplyWeightsToNodes()
    {
        var state = _mesh.GetMeshState();

        foreach (var pair in _regionVertexIndex)
        {
            state.Nodes[pair.Value] = RNG.Next(4, 7);
        }

        var returnState = _mesh.ApplyRoomsBasedOnWeights(state);

        for (int i = 0; i < _mesh.Lines.Count; i++)
        {
            var l = _mesh.Lines[i];

            if (returnState.Nodes[l.Nodes[0].Index] != -1 && returnState.Nodes[l.Nodes[1].Index] != -1)
            {

                //if (returnState.Nodes[l.Nodes[0].Index] == returnState.Nodes[l.Nodes[1].Index])
                //{/
                //if (returnState.Nodes[l.Nodes[0].Index] == -1)
                //continue;
                var nodeValue = returnState.Nodes[l.Nodes[0].Index] > returnState.Nodes[l.Nodes[1].Index] ? returnState.Nodes[l.Nodes[0].Index] : returnState.Nodes[l.Nodes[1].Index];

                var colourHue = Mathf.InverseLerp(0f, 50f, nodeValue);
                l.DrawLine(Color.HSVToRGB(colourHue, 1f, 1f), 100f);
            }

        }
        return returnState;
    }

}
