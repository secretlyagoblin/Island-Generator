using UnityEngine;
using System.Collections.Generic;

namespace Maps {

    public partial class Map {
        //Contains constructiors, accessors, static functions and math functions

        public int SizeX
        { get; private set; }

        public int SizeY
        { get; private set; }

        float[,] _map;

        public float[,] FloatArray
        { get { return Clone(this)._map; } }

        public Map(Map mapTemplate)
        {
            SizeX = mapTemplate.SizeX;
            SizeY = mapTemplate.SizeY;
            _map = new float[SizeX, SizeY];
        }

        public Map(int sizeX, int sizeY)
        {
            SizeX = sizeX;
            SizeY = sizeY;
            _map = new float[SizeX, SizeY];
        }

        public Map(Map mapTemplate, float defaultValue)
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

        public Map(int sizeX, int sizeY, float defaultValue)
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

        public static void SetGlobalDisplayStack(MeshDebugStack stack)
        {
            _stack = stack;
        }

        public static MeshDebugStack SetGlobalDisplayStack()
        {
            _stack = new MeshDebugStack(new Material(Shader.Find("Standard")));
            return _stack;
        }

        

        public Map Display()
        {
            if (_stack != null)
            {
                _stack.RecordMapStateToStack(this);
            }

            return this;
        }

        public static Map Clone(Map map)
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
                    averageX += coords[i][u].x;
                    averageY += coords[i][u].y;
                }

                averageX /= coords[i].Count;
                averageY /= coords[i].Count;

                returnCoords.Add(new Coord(averageX, averageY));
            }

            return returnCoords;
        }

        public static Map BlankMap(Map template)
        {
            return new Map(template.SizeX, template.SizeY);
        }

        public static Map CreateHeightMap(Map[] heightData)
        {
            return Clone(heightData[0]).AddHeightmapLayers(heightData, 0);
        }

        public static Map BlankMap(int sizeX, int sizeY)
        {
            return new Map(sizeX, sizeY);
        }

        public static Map ApplyMask(Map mapA, Map mapB, Map mask)
        {
            return Clone(mapA).ApplyMask(mask, mapB);
        }

        public static bool MapsAreSameDimensions(Map mapA, Map mapB)
        {
            return mapA.SizeX == mapB.SizeX && mapA.SizeY == mapB.SizeY;

        }

        public static Map BooleanUnion(Map mapA, Map mapB)
        {
            //Need a check here to avoid failure

            if (!MapsAreSameDimensions(mapA, mapB))
            {
                Debug.Log("Maps are not the same size!");
                return null;
            }

            var outputMap = new Map(mapA);

            for (int x = 0; x < mapA.SizeX; x++)
            {
                for (int y = 0; y < mapA.SizeY; y++)
                {
                    outputMap[x, y] = (mapA[x, y] == 0 | mapB[x, y] == 0) ? 0 : 1;
                }
            }
            return outputMap;
        }

        public static Map BooleanIntersection(Map mapA, Map mapB)
        {
            //Need a check here to avoid failure

            if (!MapsAreSameDimensions(mapA, mapB))
            {
                Debug.Log("Maps are not the same size!");
                return null;
            }

            var outputMap = new Map(mapA);

            for (int x = 0; x < mapA.SizeX; x++)
            {
                for (int y = 0; y < mapA.SizeY; y++)
                {
                    outputMap[x, y] = (mapA[x, y] == 0 && mapB[x, y] == 0) ? 0 : 1;
                }
            }
            return outputMap;
        }

        public static Map BooleanDifference(Map mapA, Map mapB)
        {
            //Need a check here to avoid failure

            if (!MapsAreSameDimensions(mapA, mapB))
            {
                Debug.Log("Maps are not the same size!");
                return null;
            }

            var outputMap = new Map(mapA);

            for (int x = 0; x < mapA.SizeX; x++)
            {
                for (int y = 0; y < mapA.SizeY; y++)
                {
                    outputMap[x, y] = (mapA[x, y] == 0) ? 1 : mapB[x, y];
                }
            }
            return outputMap;
        }

        public Map BooleanMapFromThreshold(float threshold)
        {
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    _map[x, y] = _map[x, y] < threshold ? 0 : 1;
                }
            }

            return this;
        }

        public static Map GetInvertedMap(Map map)
        {
            return Clone(map).Invert();
        }

        public static Map HigherResult(Map mapA, Map mapB)
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

        public static Map operator +(Map a, Map b)
        {
            var outputMap = new Map(a);

            for (int x = 0; x < a.SizeX; x++)
            {
                for (int y = 0; y < a.SizeY; y++)
                {
                    outputMap[x, y] = a[x, y] + b[x, y];
                }
            }

            return outputMap;
        }

        public static Map operator -(Map a, Map b)
        {
            var outputMap = new Map(a);

            for (int x = 0; x < a.SizeX; x++)
            {
                for (int y = 0; y < a.SizeY; y++)
                {
                    outputMap[x, y] = a[x, y] - b[x, y];
                }
            }

            return outputMap;
        }

        public static Map operator *(Map a, Map b)
        {
            var outputMap = new Map(a);

            for (int x = 0; x < a.SizeX; x++)
            {
                for (int y = 0; y < a.SizeY; y++)
                {
                    outputMap[x, y] = a[x, y] * b[x, y];
                }
            }

            return outputMap;
        }

        public static Map operator /(Map a, Map b)
        {
            var outputMap = new Map(a);

            for (int x = 0; x < a.SizeX; x++)
            {
                for (int y = 0; y < a.SizeY; y++)
                {
                    outputMap[x, y] = a[x, y] / b[x, y];
                }
            }

            return outputMap;
        }

        public static Map Blend(Map mapA, Map mapB, Map blendMap)
        {
            var outputMap = Map.BlankMap(mapA);

            for (int x = 0; x < mapA.SizeX; x++)
            {
                for (int y = 0; y < mapA.SizeY; y++)
                {
                    outputMap[x, y] = (mapA[x, y] * blendMap[x, y]) + (mapB[x, y] * (1 - blendMap[x, y]));
                }
            }

            return outputMap;
        }

        public static Map CreateMapFromSubMapsWithoutResizing(Map[,] mapArray)
        {
            var finalX = mapArray[0, 0].SizeX * mapArray.GetLength(0);
            var finalY = mapArray[0, 0].SizeY * mapArray.GetLength(1);

            var finalMap = new Map(finalX, finalY);

            var trueLength = finalMap.SizeX - 1;

            for (int arrayX = 0; arrayX < mapArray.GetLength(0); arrayX++)
            {
                for (int arrayY = 0; arrayY < mapArray.GetLength(1); arrayY++)
                {
                    var currentMap = mapArray[arrayX, arrayY];

                    var startX = arrayX * currentMap.SizeX;
                    var startY = arrayY * currentMap.SizeY;

                    for (int x = 0; x < currentMap.SizeX; x++)
                    {
                        for (int y = 0; y < currentMap.SizeY; y++)
                        {
                            finalMap[startX + x, startY + y] = currentMap[x, y];
                        }
                    }                    
                }
            }

            //Handle Edges Here

            return finalMap;

        }

        public static Map CreateMapFromSubMapsAssumingOnePixelOverlap(Map[,] mapArray)
        {
            var finalMap = new Map(mapArray[0, 0]);

            var trueLength = finalMap.SizeX - 1;

            for (int arrayX = 0; arrayX < 2; arrayX++)
            {
                for (int arrayY = 0; arrayY < 2; arrayY++)
                {
                    var currentMap = mapArray[arrayX, arrayY];

                    var trueX = arrayX * trueLength / 2;                   

                    for (int x = 0; x < trueLength; x += 2)
                    {
                        var trueY = arrayY * trueLength / 2;
                        for (int y = 0; y < trueLength; y += 2)
                        {
                            finalMap[trueX, trueY] = currentMap[x, y];
                            trueY++;
                        }
                        trueX++;
                    }
                }
            }

            for (int arrayX = 1; arrayX < 2; arrayX++)
            {
                for (int arrayY = 0; arrayY < 2; arrayY++)
                {
                    var currentMap = mapArray[arrayX, arrayY];
                    var trueY = arrayY * trueLength / 2;

                    for (int y = 0; y < trueLength; y += 2)
                    {
                        finalMap[trueLength, trueY] = currentMap[trueLength, y];
                        trueY++;
                    }
                }
            }

            for (int arrayX = 0; arrayX < 2; arrayX++)
            {
                for (int arrayY = 1; arrayY < 2; arrayY++)
                {
                    var currentMap = mapArray[arrayX, arrayY];
                    var trueX = arrayX * trueLength / 2;

                    for (int x = 0; x < trueLength; x += 2)
                    {
                        finalMap[trueX, trueLength] = currentMap[x, trueLength];
                        trueX++;
                    }
                }
            }

            finalMap[trueLength, trueLength] = mapArray[1, 1][trueLength, trueLength];

            //Handle Edges Here

            return finalMap;

        }

        public static Map DecimateMap(Map map, int decimationFactor)
        {
            var length = map.SizeX;

            var trueLength = length - 1;
            var arraySize = (trueLength / decimationFactor) + 1;

            var finalMap = new Map(arraySize, arraySize);

            var trueX = 0;
            var trueY = 0;

            for (int x = 0; x < trueLength; x += decimationFactor)
            {
                trueY = 0;
                for (int y = 0; y < trueLength; y += decimationFactor)
                {
                    finalMap[trueX, trueY] = map[x, y];
                    trueY++;
                }
                trueX++;
            }

            trueY = 0;

            for (int y = 0; y < trueLength; y += decimationFactor)
            {
                finalMap[arraySize-1, trueY] = map[trueLength, y];
                trueY++;
            }

            trueX = 0;

            for (int x = 0; x < trueLength; x += decimationFactor)
            {
                finalMap[trueX, arraySize-1] = map[x, trueLength];
                trueX++;
            }

            finalMap[arraySize-1, arraySize-1] = map[trueLength, trueLength];

            //Handle Edges Here

            return finalMap;

        }



        // Map Photoshop Functions

        public Map Lighten(Map other)
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

        // Texture Parsing Functions

        public static Map MapFromGrayscaleTexture(Texture2D grayscaleTexture)
        {
            var sizeX = grayscaleTexture.width;
            var sizeY = grayscaleTexture.height;

            var map = new Map(sizeX, sizeY);

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    var pixel = grayscaleTexture.GetPixel(x, y);
                    map[x, y] = pixel.grayscale < 0.5f? 0:1;
                }
            }
            
            //Hacky lining up with texture
            map.Rotate90().Rotate90().Rotate90().MirrorY().MirrorX();

            return map;
        }
    }
}