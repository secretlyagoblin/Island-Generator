using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderingRoad.Procgen.RecursiveHex;
using WanderingRoad.Procgen.Topology;

namespace WanderingRoad.Procgen.Levelgen
{
    public abstract class MultiHexGraph
    {
        protected HexGroup _hexGroup;

        public MultiHexGraph(HexGroup hexGroup)
        {
            _hexGroup = hexGroup;
        }

        public abstract HexGroup Finalise(bool debugDraw = false);

        protected static HexPayload StandardRemapper(HexPayload hex, Connection nodeStatus, int[] connections)
        {
            var done = hex;

            done.ConnectionStatus = nodeStatus;
            done.Connections = new CodeConnections(connections);
            return done;
        }

    }
}
