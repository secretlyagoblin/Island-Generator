using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderingRoad.Procgen.RecursiveHex;

namespace WanderingRoad.Procgen.Levelgen
{
    public abstract class MultiHexGraph
    {
        protected HexGroup _sourceGroup;

        public MultiHexGraph(HexGroup hexGroup)
        {
            _sourceGroup = hexGroup;
        }

        public abstract void DebugDraw(Color color);

        public abstract HexPayload[] Finalise(); 

    }
}
