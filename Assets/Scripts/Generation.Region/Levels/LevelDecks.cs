using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderingRoad.Procgen.RecursiveHex;
using WanderingRoad.Procgen.Topology;
using WanderingRoad.Random;

namespace WanderingRoad.Procgen.Levelgen
{
    public class LevelDeck : LoopingDeck<System.Func<SubMesh<HexPayload>, MeshState<Connection>>>
    {

    }
}
