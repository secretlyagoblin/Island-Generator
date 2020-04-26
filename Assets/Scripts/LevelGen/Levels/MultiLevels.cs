using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
            var walkability = _hexGroup.ToGraph<Levels.TestBed>(
                x => x.Code,
                x => x.Connections.ToArray())
                .Finalise(StandardRemapper);

            _hexGroup.MassUpdateHexes(walkability);

            var cliffDistance = _hexGroup.ToGraph<Levels.ApplyBounds>(
                x => x.ConnectionStatus == Topology.Connection.NotPresent ? 1 : 2,
                x => x.Connections.ToArray())
                .Finalise(StandardRemapper);

            for (int i = 0; i < walkability.Length; i++)
            {
                walkability[i] = new HexPayload(walkability[i])
                {
                    Height = cliffDistance[i].Height
                };
            }

            return _hexGroup.MassUpdateHexes(walkability);
        }


    }

    
}
