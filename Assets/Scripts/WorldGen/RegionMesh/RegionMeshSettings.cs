using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGen {

    [CreateAssetMenu(fileName = "Region Mesh Settings", menuName = "WorldGen/Region Mesh Settings", order = 3)]
    public class RegionMeshSettings : ScriptableObject {

        //public int MinRoomSize;
        //public float BoundsOffsetPercentage;// I don't think this does anything
        //public float DelaunayBoundsRatio = 0.015f;
        //public int MinRoomDiffusion;
        //public int MaxRoomDiffusion;
        //
        //public float DebugDisplayTime = 0.25f;

        [TextArea]
        public string Comments;

    }
}
