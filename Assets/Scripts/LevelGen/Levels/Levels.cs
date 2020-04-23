using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderingRoad.Procgen.RecursiveHex;

namespace WanderingRoad.Procgen.Levelgen
{
    public class SingleConnectionGraph : HexGraph
    {
        public SingleConnectionGraph(Vector3[] verts, int[] tris, HexPayload[] nodes, Func<HexPayload, int> identifier, Func<HexPayload, int[]> connector) : base(verts, tris, nodes, identifier, connector)
        {
        }

        protected override void Generate()
        {
            for (int i = 0; i < _collection.Meshes.Length; i++)
            {
                var mesh = _collection.Meshes[i];

                if (mesh.Id < 0)
                {
                    continue;
                }

                mesh.SetConnectivity(Levelgen.Connectivity.ConnectEverything);
                mesh.SetConnectivity(Levelgen.Connectivity.RemoveUnnecessaryCriticalNodesAssumingHexGrid);
            }

            for (int i = 0; i < _nodeMetadata.Length; i++)
            {
                _nodeMetadata[i].Code = i + 1;
            }

            _collection.Bridges.LeaveSingleRandomConnection();
            _collection.MarkBridgeInterfacesAsCritical();

            for (int i = 0; i < _collection.Meshes.Length; i++)
            {
                var mesh = _collection.Meshes[i];

                if (mesh.Id < 0)
                {
                    continue;
                }

                mesh.SetConnectivity(Levelgen.Connectivity.AddOneLayerOfEdgeBufferAroundNeighbourSubMeshesAssumingHexGrid);
                mesh.SetConnectivity(Levelgen.Connectivity.RecoverOrphanedCriticalNodes);
            }
        }
    }

    public class ConnectEverything : HexGraph
    {
        public ConnectEverything(Vector3[] verts, int[] tris, HexPayload[] nodes, Func<HexPayload, int> identifier, Func<HexPayload, int[]> connector) : base(verts, tris, nodes, identifier, connector)
        {
        }

        protected override void Generate()
        {
            for (int i = 0; i < _collection.Meshes.Length; i++)
            {
                var mesh = _collection.Meshes[i];

                if (mesh.Id < 0)
                {
                    continue;
                }

                mesh.SetConnectivity(Levelgen.Connectivity.ConnectEverything);
                mesh.SetConnectivity(Levelgen.Connectivity.RemoveUnnecessaryCriticalNodesAssumingHexGrid);
            }
        }
    }

    public class TestBed : HexGraph
    {
        public TestBed(Vector3[] verts, int[] tris, HexPayload[] nodes, Func<HexPayload, int> identifier, Func<HexPayload, int[]> connector) : base(verts, tris, nodes, identifier, connector)
        {
        }

        protected override void Generate()
        {

            for (int i = 0; i < _collection.Meshes.Length; i++)
            {
                var mesh = _collection.Meshes[i];

                if (mesh.Id < 0)
                {
                    continue;
                }

                mesh.SetConnectivity(Levelgen.Connectivity.ConnectEverythingExceptEdges);
                mesh.SetConnectivity(Levelgen.Connectivity.TubbyCorridors);
                mesh.SetConnectivity(Levelgen.Connectivity.ConnectEverythingExceptEdges);
            }

            for (int i = 0; i < _nodeMetadata.Length; i++)
            {
                _nodeMetadata[i].Code = i + 1;
            }

            //_collection.Bridges.LeaveSingleRandomConnection();
        }
    }

    public class ApplyBounds : HexGraph
    {
        public ApplyBounds(Vector3[] verts, int[] tris, HexPayload[] nodes, Func<HexPayload, int> identifier, Func<HexPayload, int[]> connector) : base(verts, tris, nodes, identifier, connector)
        {
        }

        protected override void Generate()
        {

            for (int i = 0; i < _collection.Meshes.Length; i++)
            {
                var mesh = _collection.Meshes[i];

                if (mesh.Id < 0)
                {
                    continue;
                }

                mesh.SetConnectivity(Levelgen.Connectivity.ConnectOnlyEdges);

                //Levelgen.

                //mesh.SetConnectivity(Levelgen.States.TubbyCorridors);
                //mesh.SetConnectivity(Levelgen.States.ConnectEverythingExceptEdges);
            }

            //for (int i = 0; i < _nodeMetadata.Length; i++)
            //{
            //    _nodeMetadata[i].Code = i + 1;
            //}

            //_collection.Bridges.LeaveSingleRandomConnection();
        }
    }


    public class HighLevelConnectivity : HexGraph
    {
        public HighLevelConnectivity(
            Vector3[] verts,
            int[] tris,
            HexPayload[] nodes,
            Func<HexPayload, int> identifier,
            Func<HexPayload, int[]> connector) : base(verts, tris, nodes, identifier, connector) { }

        protected override void Generate()
        {

            for (int i = 0; i < _collection.Meshes.Length; i++)
            {
                var mesh = _collection.Meshes[i];

                if (mesh.Id < 0)
                {
                    continue;
                }

                mesh.SetConnectivity(Levelgen.Connectivity.ConnectEverything);
                mesh.SetConnectivity(Levelgen.Connectivity.SummedDikstra);
            }

            for (int i = 0; i < _nodeMetadata.Length; i++)
            {
                _nodeMetadata[i].Code = i + 1;
            }

            //_collection.Bridges.LeaveSingleRandomConnection();
        }
    }

    public class NoBehaviour : HexGraph
    {
        public NoBehaviour(Vector3[] verts,
            int[] tris,
            HexPayload[] nodes,
            Func<HexPayload, int> identifier,
            Func<HexPayload, int[]> connector) : base(verts, tris, nodes, identifier, connector) { }

        protected override void Generate()
        {

            throw new Exception("NoBehaviour HexGraph should never be finalised, it's for preview only.");
        }
    }

    public class MinimiseCriticalNodes : HexGraph
    {
        public MinimiseCriticalNodes(Vector3[] verts,
            int[] tris,
            HexPayload[] nodes,
            Func<HexPayload, int> identifier,
            Func<HexPayload, int[]> connector) : base(verts, tris, nodes, identifier, connector) { }

        protected override void Generate()
        {
            for (int i = 0; i < _collection.Meshes.Length; i++)
            {
                var mesh = _collection.Meshes[i];

                if (mesh.Id < 0)
                {
                    continue;
                }

                mesh.SetConnectivity(Levelgen.Connectivity.ConnectEverything);
                mesh.SetConnectivity(Levelgen.Connectivity.RemoveUnnecessaryCriticalNodesAssumingHexGrid);
            }

            for (int i = 0; i < _nodeMetadata.Length; i++)
            {
                _nodeMetadata[i].Code = i + 1;
            }

            _collection.Bridges.LeaveSingleRandomConnection();
        }
    }
}