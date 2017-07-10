using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Maps {

    public partial class Map {
        // GameLogic

        public Coord GetValidStartLocation()
        {

            var samplePoint = new Coord((int)(SizeX * 0.5f), (int)(SizeY * 0.5f));

            if (_map[samplePoint.x, samplePoint.y] < 0.001f)
            {
                return new Coord(samplePoint.x, samplePoint.y);
            }

            var iterationCount = 0;
            var maxIterations = (int)(SizeX * 0.4f);

            var xNeg = false;
            var yNeg = false;

            var xStep = 1;
            var yStep = 1;

            while (iterationCount < maxIterations)
            {
                for (int x = 0; x < xStep; x++)
                {
                    if (xNeg)
                    {
                        samplePoint.x--;
                    }
                    else
                    {
                        samplePoint.x++;
                    }

                    if (_map[samplePoint.x, samplePoint.y] < 0.001f)
                    {
                        return samplePoint;
                    }
                }

                for (int y = 0; y < yStep; y++)
                {
                    if (yNeg)
                    {
                        samplePoint.y--;
                    }
                    else
                    {
                        samplePoint.y++;
                    }

                    if (_map[samplePoint.x, samplePoint.y] < 0.001f)
                    {
                        return samplePoint;
                    }
                }

                xNeg = xNeg ? false : true;
                yNeg = yNeg ? false : true;

                xStep++;
                yStep++;
            }

            Debug.Log("FailedToFindPoint");

            return (new Coord((int)(SizeX * 0.5f), (int)(SizeY * 0.5f)));
        }

        public Map[,] CreateLevelSubMapsFromThisLevelMap(int subMapSize)
        {
            var subMaps = new Map[SizeX / 3, SizeY / 3];

            var mapCountX = -1;

            for (int x = 0; x < SizeX; x+=3)
            {
                mapCountX++;
                var mapCountY = -1;

                for (int y = 0; y < SizeY; y+=3)
                {
                    mapCountY++;

                    var map = new Map(subMapSize, subMapSize);
                    map.FillWith(_map[x + 1, y + 1])
                        .SetRow(0, _map[x, y + 1])
                        .SetRow(subMapSize - 1, _map[x + 2, y + 1])
                        .SetColumn(0, _map[x + 1, y])
                        .SetColumn(subMapSize - 1, _map[x + 1, y + 2])
                        .SetIndex(0, 0, _map[x, y])
                        .SetIndex(0, subMapSize - 1, _map[x, y + 2])
                        .SetIndex(subMapSize - 1, 0, _map[x + 2, y])
                        .SetIndex(subMapSize - 1, subMapSize - 1, _map[x + 2, y + 2]);

                    subMaps[mapCountX, mapCountY] = map;
                }
            }

            return subMaps;
        }

    }
}
