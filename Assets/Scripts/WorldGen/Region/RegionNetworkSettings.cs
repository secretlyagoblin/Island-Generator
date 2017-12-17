using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGen {

    [CreateAssetMenu(fileName = "Region Network Settings", menuName = "WorldGen/Region Network Settings", order = 1)]
    public class RegionNetworkSettings : ScriptableObject {
        public int SpawnedNodeMin = 2;
        public int SpawnedNodeMax = 5;
        public float RadiusMin = 1f;
        public float RadiusMax = 2f;
    }
}
