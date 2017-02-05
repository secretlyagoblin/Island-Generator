using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Map;

namespace Terrain {

    public partial class TerrainData {

        public Layer WalkableMap
        {
            get; private set;
        }

        public PhysicalMap WalkablePhysicalMap
        {
            get
            {
                return WalkableMap.ToPhysical(Rect);
            }
        }

        public Layer HeightMap
        {
            get; private set;
        }

        ColorLayer _colorLayer;

        public Color[,] ColorMap
        {
            get
            {
                return _colorLayer.GetColorArray();
            }
        }

        private TerrainData(Rect rect, Layer walkableMap, Layer heightMap, ColorLayer colorLayer)
        {
            Rect = rect;
            WalkableMap = walkableMap;
            HeightMap = heightMap;
            _colorLayer = colorLayer;
        }

        private TerrainData(Rect rect, Layer walkableMap, Layer heightMap)
        {
            Rect = rect;
            WalkableMap = walkableMap;
            HeightMap = heightMap;
            _colorLayer = new ColorLayer(heightMap);
        }
    }

    class ColorLayer {

        public Layer R
        {
            get; private set;
        }

        public Layer G
        {
            get; private set;
        }

        public Layer B
        {
            get; private set;
        }

        public ColorLayer(Layer baseMap)
        {
            R = Layer.Clone(baseMap);
            G = Layer.Clone(baseMap);
            B = Layer.Clone(baseMap);
        }

        public Color[,] GetColorArray()
        {
            var colors = new Color[R.SizeX, R.SizeY];

            for (int x = 0; x < R.SizeX; x++)
            {
                for (int y = 0; y < R.SizeY; y++)
                {
                    colors[x, y] = new Color(R[x, y], G[x, y], B[x, y]);
                }
            }


            return colors;
        } 
    }
}
