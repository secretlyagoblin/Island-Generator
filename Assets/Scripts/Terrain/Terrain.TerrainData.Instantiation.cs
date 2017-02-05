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
            R = Layer.BlankMap(baseMap);
            G = Layer.BlankMap(baseMap);
            B = Layer.BlankMap(baseMap);
        }

        public Color[,] GetColorArray()
        {
            return new Color[1,1];
        } 
    }
}
