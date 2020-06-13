using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderingRoad.Procgen.RecursiveHex;
using WanderingRoad.Procgen.Topology;

namespace WanderingRoad.Procgen.Levelgen
{
    public abstract class HexGraph : Graph<HexPayload>
    {
        public HexGraph(Vector3[] verts, int[] tris, HexPayload[] nodes, Func<HexPayload, int> identifier, Func<HexPayload, int[]> connector) : base(verts, tris, nodes, identifier, connector)
        {
        }
    }
}