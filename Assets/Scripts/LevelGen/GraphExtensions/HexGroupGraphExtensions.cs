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
        public static HexGroup ApplyGraph<T>(this HexGroup hexgroup, GraphLevel level, bool debugDraw = false) where T : Graph<HexPayload>
        {
            Func<HexPayload, int> regionIdentifier;
            Func<HexPayload, int, HexPayload> regionEncoder;

            switch (level)
            {
                case GraphLevel.Region:
                    regionIdentifier = new Func<HexPayload, int>(x => x.Region);
                    regionEncoder = new Func<HexPayload, int, HexPayload>((x, i) => { var y = x; y.Region = i; return y; });       
                    break;
                case GraphLevel.Code:
                    regionIdentifier = new Func<HexPayload, int>(x => x.Code);
                    regionEncoder = new Func<HexPayload, int, HexPayload>((x, i) => { var y = x; y.Code = i; return y; });
                    break;
                default:
                    throw new Exception("Invalid GraphLevel");
            }


            var graph = hexgroup.ToGraph<T>(regionIdentifier, StandardConnector, regionEncoder);
            var payloads = graph.Finalise(StandardRemapper);

            if (debugDraw)
            {
                graph.DebugDrawSubmeshConnectivity(Color.blue) ;
            }

            return hexgroup.MassUpdateHexes(payloads);
        }

        private static int[] StandardConnector(HexPayload payload)
        {
            return payload.Connections.ToArray();
        }




        private static HexPayload StandardRemapper(HexPayload hex, Connection nodeStatus, int[] connections)
        {
            var done = hex;

            done.ConnectionStatus = nodeStatus;
            done.Connections = new CodeConnections(connections);
            return done;
        }

        public enum GraphLevel
        {
            Region = 0,
            Code = 1
        }
    }
}
