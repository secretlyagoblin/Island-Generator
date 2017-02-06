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

        public static TerrainData Decimate(Rect rect, TerrainData terrainData, int decimationFactor)
        {
            var walkableMap = Layer.DecimateMap(terrainData.WalkableMap, decimationFactor);
            var heightMap = Layer.DecimateMap(terrainData.HeightMap, decimationFactor);
            var r = Layer.DecimateMap(terrainData._colorLayer.R, decimationFactor);
            var g = Layer.DecimateMap(terrainData._colorLayer.G, decimationFactor);
            var b = Layer.DecimateMap(terrainData._colorLayer.B, decimationFactor);

            return new TerrainData(rect, walkableMap, heightMap, new ColorLayer(r, g, b));
        }


        public Color ColorSampleAtPoint(Vector2 vec)
        {
            var r = _colorLayer.R.BilinearSampleFromNormalisedVector2(vec);
            var g = _colorLayer.G.BilinearSampleFromNormalisedVector2(vec);
            var b = _colorLayer.B.BilinearSampleFromNormalisedVector2(vec);

            return new Color(r, g, b);
        }

        public static TerrainData CreateSubMap(Rect rect, TerrainData[,] terrainData)
        {
            //WalkableMap
            var layers = new Layer[2, 2];
            for (int x = 0; x < 2; x++)
                for (int y = 0; y < 2; y++)
                    layers[x, y] = terrainData[x, y].WalkableMap; 
            var walkableMap = Layer.CreateMapFromSubMapsAssumingOnePixelOverlap(layers);

            //HeightMap
            layers = new Layer[2, 2];
            for (int x = 0; x < 2; x++)
                for (int y = 0; y < 2; y++)
                    layers[x, y] = terrainData[x, y].HeightMap;
            var heightMap = Layer.CreateMapFromSubMapsAssumingOnePixelOverlap(layers);

            //R
            layers = new Layer[2, 2];
            for (int x = 0; x < 2; x++)
                for (int y = 0; y < 2; y++)
                    layers[x, y] = terrainData[x, y]._colorLayer.R;
            var r = Layer.CreateMapFromSubMapsAssumingOnePixelOverlap(layers);

            //G
            layers = new Layer[2, 2];
            for (int x = 0; x < 2; x++)
                for (int y = 0; y < 2; y++)
                    layers[x, y] = terrainData[x, y]._colorLayer.G;
            var g = Layer.CreateMapFromSubMapsAssumingOnePixelOverlap(layers);

            //B
            layers = new Layer[2, 2];
            for (int x = 0; x < 2; x++)
                for (int y = 0; y < 2; y++)
                    layers[x, y] = terrainData[x, y]._colorLayer.B;
            var b = Layer.CreateMapFromSubMapsAssumingOnePixelOverlap(layers);

            return new TerrainData(rect, walkableMap, heightMap, new ColorLayer(r, g, b));
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
            R = Layer.Clone(baseMap);
            G = Layer.Clone(baseMap);
            B = Layer.Clone(baseMap);

            _gradient = new Gradient();
        }

        public ColorLayer(Layer r, Layer g, Layer b)
        {
            R = r;
            G = g;
            B = b;

            _gradient = new Gradient();
        }

        public ColorLayer SetGradient(Gradient gradient)
        {
            _gradient = gradient;
            return this;
        }

        public ColorLayer ApplyMap(Layer map)
        {
            

            for (int x = 0; x < map.SizeX; x++)
            {
                for (int y = 0; y < map.SizeY; y++)
                {
                    var color = _gradient.Evaluate(map[x,y]);
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
