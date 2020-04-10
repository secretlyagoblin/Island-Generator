using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderingRoad.Procgen.RecursiveHex;
using WanderingRoad.Procgen.Topology;

namespace WanderingRoad.Procgen.Levelgen
{
    public static class HexGroupGraphExtensions
    {
        public static HexGroup ApplyGraph<T>(this HexGroup hexgroup, Func<HexPayload, int> regionIndentifier, Func<HexPayload, int[]> regionConnector, bool debugDraw = false) where T : Graph<HexPayload>
        {
            var graph = hexgroup.ToGraph<T>(regionIndentifier, regionConnector);
            var payloads = graph.Finalise(StandardRemapper);

            if (debugDraw)
            {
                graph.DebugDraw();
            }

            return hexgroup.MassUpdateHexes(payloads);
        }



        private static HexPayload StandardRemapper(HexPayload hex, Connection nodeStatus, int[] connections)
        {
            var done = hex;

            done.ConnectionStatus = nodeStatus;
            done.Connections = new CodeConnections(connections);
            return done;
        }

    }
}
