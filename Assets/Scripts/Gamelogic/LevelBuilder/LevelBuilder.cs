using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Linq;
using WanderingRoad.Random;

namespace WanderingRoad.Procgen.Levelgen
{

    public class LevelBuilder
    {
        public static LevelInfo BuildLevel(string seed)
        {
            var info = HexMapBuilder.BuildHexMap(seed);
            TerrainGenerator.BuildTerrain(info);
            return info;
        }



    }
}