using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGen {

    [CreateAssetMenu(fileName = "Region Network Settings", menuName = "WorldGen/Region Network Settings", order = 1)]
    public class RegionNetworkSettings : ScriptableObject {
        public int NodeMin = 2;
        public int NodeMax = 5;
    }
}
