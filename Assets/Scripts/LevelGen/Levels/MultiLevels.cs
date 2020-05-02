using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderingRoad.Core.Random;
using WanderingRoad.Procgen.RecursiveHex;

namespace WanderingRoad.Procgen.Levelgen
{
    public class InterconnectionLogic : MultiHexGraph
    {
        public InterconnectionLogic(HexGroup hexGroup) : base(hexGroup)
        { 
        }

        public override HexGroup Finalise(bool debugDraw = false)
        {
            var walkabilityGraph = _hexGroup.ToGraph<Levels.TestBed>(
                x => x.Code,
                x => x.Connections.ToArray());

            var walkability = walkabilityGraph.Finalise(StandardRemapper);

            if(debugDraw) walkabilityGraph.DebugDrawSubmeshConnectivity(Color.blue);

            _hexGroup.MassUpdateHexes(walkability);

            var cliffDistance = _hexGroup.ToGraph<Levels.ApplyBounds>(
                x => x.ConnectionStatus == Topology.Connection.NotPresent ? 1 : 0,
                x => x.Connections.ToArray())
                .Finalise(StandardRemapper);

            for (int i = 0; i < walkability.Length; i++)
            {
                walkability[i] = new HexPayload(walkability[i])
                {
                    EdgeDistance = cliffDistance[i].EdgeDistance,
                    Height= walkability[i].Height+ (RNG.NextFloat(0.05f))
                };
            }

            return _hexGroup.MassUpdateHexes(walkability);
        }


    }

    
}
