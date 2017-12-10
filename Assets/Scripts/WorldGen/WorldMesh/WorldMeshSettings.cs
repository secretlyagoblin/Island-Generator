using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGen {

    [CreateAssetMenu(fileName = "World Mesh Settings", menuName = "WorldGen/World Mesh Settings", order = 2)]
    public class WorldMeshSettings : ScriptableObject {

        public int MinRoomSize;
    }
}
