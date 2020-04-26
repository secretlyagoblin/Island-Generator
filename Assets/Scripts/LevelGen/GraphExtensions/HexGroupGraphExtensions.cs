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
                graph.DebugDrawSubmeshConnectivity(Color.blue) ;
            }

            return hexgroup.MassUpdateHexes(payloads);
        }

        public static HexGroup ApplyGraph<T>(this HexGroup hexgroup, bool debugDraw = false) where T : MultiHexGraph
        {
            var multiGraph = Activator.CreateInstance(typeof(T), hexgroup) as T;

            //Note, this is modifying the hexgroup as a side effect, not returing a new instance
            var processedHexgroup = multiGraph.Finalise(debugDraw);

            return processedHexgroup;
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
