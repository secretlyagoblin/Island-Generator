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
            _colorLayer = new ColorLayer(heightMap.Clone().Remap(0.1f,1f));
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

        Gradient _gradient;

        public ColorLayer(Layer baseMap)
        {
            R = Layer.BlankMap(baseMap);
            G = Layer.BlankMap(baseMap);
            B = Layer.BlankMap(baseMap);

            _gradient = new Gradient();
        }

        public ColorLayer SetGradient(Gradient gradient)
        {
            _gradient = gradient;
            return this;
        }

        public ColorLayer ApplyMap(Layer Map)
        {
            for (int x = 0; x < Map.SizeX; x++)
            {
                for (int y = 0; y < Map.SizeY; y++)
                {
                    var color = _gradient.Evaluate(Map[x,y]);
                    R[x,y] = color.r;
                    G[x, y] = color.g;
                    B[x, y] = color.b;
                }
            }

            return this;
        }

        public ColorLayer ApplyMapWithMask(Layer Map, Layer Mask)
        {
            for (int x = 0; x < Map.SizeX; x++)
            {
                for (int y = 0; y < Map.SizeY; y++)
                {
                    var color = _gradient.Evaluate(Map[x, y]);
                    R[x, y] = Mathf.Lerp(R[x, y],color.r,Mask[x,y]);
                    G[x, y] = Mathf.Lerp(G[x, y], color.g, Mask[x, y]);
                    B[x, y] = Mathf.Lerp(B[x, y], color.b, Mask[x, y]);
                }
            }

            return this;
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
