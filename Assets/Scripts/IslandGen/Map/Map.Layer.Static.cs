using UnityEngine;
using System.Collections.Generic;

namespace Map {

    public partial class Layer {
        //Contains constructiors, accessors, static functions and math functions

        public int SizeX
        { get; private set; }

        public int SizeY
        { get; private set; }

        float[,] _map;

        public float[,] FloatArray
        { get { return Clone(this).Normalise()._map; } }

        public Layer(Layer mapTemplate)
        {
            SizeX = mapTemplate.SizeX;
            SizeY = mapTemplate.SizeY;
            _map = new float[SizeX, SizeY];
        }

        public Layer(int sizeX, int sizeY)
        {
            SizeX = sizeX;
            SizeY = sizeY;
            _map = new float[SizeX, SizeY];
        }

        public Layer(Layer mapTemplate, float defaultValue)
        {
            SizeX = mapTemplate.SizeX;
            SizeY = mapTemplate.SizeY;
            _map = new float[SizeX, SizeY];

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    _map[x, y] = defaultValue;
                }
            }
        }

        public Layer(int sizeX, int sizeY, float defaultValue)
        {
            SizeX = sizeX;
            SizeY = sizeY;
            _map = new float[SizeX, SizeY];

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    _map[x, y] = defaultValue;
                }
            }
        }

        // Accessors

        public float this[int indexA, int indexB]
        {
            get { return _map[indexA, indexB]; }
            set { _map[indexA, indexB] = value; }
        }

        // Static Functions

        static MeshDebugStack _stack = null;

        public static void SetGlobalStack(MeshDebugStack stack)
        {
            _stack = stack;
        }

        public Layer AddToGlobalStack()
        {
            if (_stack != null)
            {
                _stack.RecordMapStateToStack(this);
            }

            return this;
        }

        public static Layer Clone(Layer map)
        {
            return BlankMap(map).OverwriteMapWith(map);
        }

        public static List<Coord> GetCenters(List<List<Coord>> coords)
        {
            var returnCoords = new List<Coord>();

            for (int i = 0; i < coords.Count; i++)
            {
                var averageX = 0;
                var averageY = 0;

                for (int u = 0; u < coords[i].Count; u++)
                {
                    averageX += coords[i][u].TileX;
                    averageY += coords[i][u].TileY;
                }

                averageX /= coords[i].Count;
                averageY /= coords[i].Count;

                returnCoords.Add(new Coord(averageX, averageY));
            }

            return returnCoords;
        }

        public static Layer BlankMap(Layer template)
        {
            return new Layer(template.SizeX, template.SizeY);
        }

        public static Layer CreateHeightMap(Layer[] heightData)
        {
            return Clone(heightData[0]).AddHeightmapLayers(heightData, 0);
        }

        public static Layer BlankMap(int sizeX, int sizeY)
        {
            return new Layer(sizeX, sizeY);
        }

        public static Layer ApplyMask(Layer mapA, Layer mapB, Layer mask)
        {
            return Clone(mapA).ApplyMask(mask, mapB);
        }

        public static bool MapsAreSameDimensions(Layer mapA, Layer mapB)
        {
            return mapA.SizeX == mapB.SizeX && mapA.SizeY == mapB.SizeY;

        }

        public static Layer BooleanUnion(Layer mapA, Layer mapB)
        {
            //Need a check here to avoid failure

            if (!MapsAreSameDimensions(mapA, mapB))
            {
                Debug.Log("Maps are not the same size!");
                return null;
            }

            var outputMap = new Layer(mapA);

            for (int x = 0; x < mapA.SizeX; x++)
            {
                for (int y = 0; y < mapA.SizeY; y++)
                {
                    outputMap[x, y] = (mapA[x, y] == 0 | mapB[x, y] == 0) ? 0 : 1;
                }
            }
            return outputMap;
        }

        public static Layer BooleanIntersection(Layer mapA, Layer mapB)
        {
            //Need a check here to avoid failure

            if (!MapsAreSameDimensions(mapA, mapB))
            {
                Debug.Log("Maps are not the same size!");
                return null;
            }

            var outputMap = new Layer(mapA);

            for (int x = 0; x < mapA.SizeX; x++)
            {
                for (int y = 0; y < mapA.SizeY; y++)
                {
                    outputMap[x, y] = (mapA[x, y] == 0 && mapB[x, y] == 0) ? 0 : 1;
                }
            }
            return outputMap;
        }

        public static Layer BooleanDifference(Layer mapA, Layer mapB)
        {
            //Need a check here to avoid failure

            if (!MapsAreSameDimensions(mapA, mapB))
            {
                Debug.Log("Maps are not the same size!");
                return null;
            }

            var outputMap = new Layer(mapA);

            for (int x = 0; x < mapA.SizeX; x++)
            {
                for (int y = 0; y < mapA.SizeY; y++)
                {
                    outputMap[x, y] = (mapA[x, y] == 0) ? 1 : mapB[x, y];
                }
            }
            return outputMap;
        }

        public Layer BooleanMapFromThreshold(float threshold)
        {
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    _map[x, y] = _map[x, y] <= threshold ? 0 : 1;
                }
            }

            return this;
        }

        public static Layer GetInvertedMap(Layer map)
        {
            return Clone(map).Invert();
        }

        public static Layer HigherResult(Layer mapA, Layer mapB)
        {
            var map = Clone(mapA);

            for (int x = 0; x < mapA.SizeX; x++)
            {
                for (int y = 0; y < mapA.SizeY; y++)
                {
                    map[x, y] = mapA[x, y] > mapB[x, y] ? mapA[x, y] : mapB[x, y];
                }
            }

            return map;
        }

        // Math Functions

        public static Layer operator +(Layer a, Layer b)
        {
            var outputMap = new Layer(a);

            for (int x = 0; x < a.SizeX; x++)
            {
                for (int y = 0; y < a.SizeY; y++)
                {
                    outputMap[x, y] = a[x, y] + b[x, y];
                }
            }

            return outputMap;
        }

        public static Layer operator -(Layer a, Layer b)
        {
            var outputMap = new Layer(a);

            for (int x = 0; x < a.SizeX; x++)
            {
                for (int y = 0; y < a.SizeY; y++)
                {
                    outputMap[x, y] = a[x, y] - b[x, y];
                }
            }

            return outputMap;
        }

        public static Layer operator *(Layer a, Layer b)
        {
            var outputMap = new Layer(a);

            for (int x = 0; x < a.SizeX; x++)
            {
                for (int y = 0; y < a.SizeY; y++)
                {
                    outputMap[x, y] = a[x, y] * b[x, y];
                }
            }

            return outputMap;
        }

        public static Layer operator /(Layer a, Layer b)
        {
            var outputMap = new Layer(a);

            for (int x = 0; x < a.SizeX; x++)
            {
                for (int y = 0; y < a.SizeY; y++)
                {
                    outputMap[x, y] = a[x, y] / b[x, y];
                }
            }

            return outputMap;
        }


        public static Layer Blend(Layer mapA, Layer mapB, Layer blendMap)
        {
            var outputMap = Layer.BlankMap(mapA);

            for (int x = 0; x < mapA.SizeX; x++)
            {
                for (int y = 0; y < mapA.SizeY; y++)
                {
                    outputMap[x, y] = (mapA[x, y] * blendMap[x, y]) + (mapB[x, y] * (1 - blendMap[x, y]));
                }
            }

            return outputMap;
        }

        public static Layer CreateMapFromSubMapsAssumingOnePixelOverlap(Layer[,] mapArray)
        {
            var finalMap = new Layer(mapArray[0, 0]);

            var trueLength = finalMap.SizeX - 1;

            for (int arrayX = 0; arrayX < 2; arrayX++)
            {
                for (int arrayY = 0; arrayY < 2; arrayY++)
                {
                    var currentMap = mapArray[arrayX, arrayY];

                    var trueX = arrayX * trueLength;
                    var trueY = arrayY * trueLength;

                    for (int x = 0; x < trueLength; x += 2)
                    {
                        for (int y = 0; y < trueLength; y += 2)
                        {
                            finalMap[trueX, trueY] = currentMap[x, y];
                            trueY++;
                        }
                        trueX++;
                    }
                }
            }

            //Handle Edges Here

            return finalMap;

        }


        // Map Photoshop Functions

        public Layer Lighten(Layer other)
        {
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    _map[x, y] = _map[x, y] > other[x, y] ? _map[x, y] : other[x, y];
                }
            }

            return this;
        }
    }
}