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

        public override void DebugDraw(Color color)
        {
            throw new System.NotImplementedException();
        }

        public override HexPayload[] Finalise()
        {
            throw new System.NotImplementedException();
        }
    }
}
