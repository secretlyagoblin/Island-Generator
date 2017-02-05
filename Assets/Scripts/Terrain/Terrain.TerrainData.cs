using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Map;

namespace Terrain {

    public partial class TerrainData {

        public Rect Rect
        {
            get; private set;
        }

        Map.Stack _stack;

        private TerrainData()
        {

        }

        public float[,] GetFloatArray(MapType type)
        {
            return _stack.GetMap(type).FloatArray;
        }

        public Layer GetHeightmapLayer(MapType type)
        {
            return _stack.GetMap(type);
        }

    }
}
