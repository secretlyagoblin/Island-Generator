using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderingRoad.Core.Random;
using WanderingRoad.Procgen.RecursiveHex;
using WanderingRoad.Procgen.Topology;

namespace WanderingRoad.Procgen.Levelgen
{
    public class LevelDeck : LoopingDeck<System.Func<SubMesh<HexPayload>, MeshState<Connection>>>
    {

    }
}
