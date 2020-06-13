using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderingRoad.Core.Random;
using WanderingRoad.Procgen.RecursiveHex;

namespace WanderingRoad.Procgen.Levelgen.Levels
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
            var corridorDeck = new LevelDeck();
            //corridorDeck.DebugMode = true;
            corridorDeck.Add(Levelgen.Connectivity.MinimalCorridorTwo, 3, "Two Corridors");
            corridorDeck.Add(Levelgen.Connectivity.TubbyCorridors,2, "Thicc Corridors");
            //corridorDeck.Add(Levelgen.Connectivity.MinimalCorridor, 1, "One Corridor");

            var roomDeck = new LevelDeck();
            //corridorDeck.DebugMode = true;
            roomDeck.Add(Levelgen.Connectivity.MinimalCorridorTwo, 1, "Two Corridors");
            roomDeck.Add(Levelgen.Connectivity.TubbyCorridors, 3, "Thicc Corridors");

            var openAreaDeck = new LevelDeck();
            openAreaDeck.Add(Levelgen.Connectivity.TubbyCorridors, 10, "Thicc Corridors");

            //this._collection.DebugDraw(Color.blue, 100f,true);

            for (int i = 0; i < _collection.Meshes.Length; i++)
            {
                var mesh = _collection.Meshes[i];

                //Handle empty regions
                for (int u = 0; u < mesh.Connectivity.Nodes.Length; u++)
                {
                    if (mesh.Connectivity.Nodes[u] != Topology.Connection.NotPresent)
                        goto notEmpty;
                }

                continue;
            notEmpty:

                mesh.SetConnectivity(Levelgen.Connectivity.ConnectEverythingExceptEdges);
                //mesh.DebugDraw(Color.red, 100f);
                

                var props = mesh.Properties;

                if(props.Connections < 3 && props.MoreSingleInclusive)
                {
                    mesh.SetConnectivity(corridorDeck.Draw());
                    //mesh.DebugDraw(Color.red, 100f);
                    //Debug.Log("Corridor!");
                }
                else if (props.Connections < 4)
                {
                    mesh.SetConnectivity(roomDeck.Draw());
                    //mesh.DebugDraw(Color.blue, 100f);
                    //Debug.Log("Room!");
                }
                else
                {
                    mesh.SetConnectivity(openAreaDeck.Draw());
                    //mesh.DebugDraw(Color.green, 100f);
                    //Debug.Log("Open area!");
                }                

                mesh.SetConnectivity(Levelgen.Connectivity.ConnectEverythingExceptEdges);
            }

            for (int i = 0; i < _nodeMetadata.Length; i++)
            {
                _nodeMetadata[i].Code = i + 1;
            }

            //_collection.Bridges.LeaveSingleRandomConnection();
        }
    }

    public class PostprocessTerrain : HexGraph
    {
        public PostprocessTerrain(Vector3[] verts, int[] tris, HexPayload[] nodes, Func<HexPayload, int> identifier, Func<HexPayload, int[]> connector) : base(verts, tris, nodes, identifier, connector)
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
            }
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

                var storedConnectivity = mesh.Connectivity.Clone();

                mesh.SetConnectivity(Levelgen.Connectivity.ConnectOnlyEdges);

                this.ApplyValuesToNodeMetadata(
                    Levelgen.Distance.GetDistanceFromEdge(mesh, 12),
                    mesh,
                    (x, y) => new HexPayload(x) { EdgeDistance = y }
                 );

                mesh.SetConnectivity(storedConnectivity);


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
                mesh.SetConnectivity(Levelgen.Connectivity.AddOneLayerOfEdgeBufferAroundNeighbourSubMeshesAssumingHexGrid);
                //mesh.SetConnectivity(Levelgen.Connectivity.SummedDikstra);
                mesh.SetConnectivity(Levelgen.Connectivity.SummedDikstraRemoveDeadEnds);
            }

            for (int i = 0; i < _nodeMetadata.Length; i++)
            {
                _nodeMetadata[i].Code = i + 1;
                _nodeMetadata[i].Height = RNG.Next(0, 3);
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