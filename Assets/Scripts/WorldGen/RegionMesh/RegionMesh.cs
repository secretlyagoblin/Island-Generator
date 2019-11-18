using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGen {

    public class RegionMesh {

        public List<int> Nodes;
        public List<int> Lines;
        public List<int> Cells;

        public int RoomCode;
        public List<int> RoomConnectivity; 

    }
}
