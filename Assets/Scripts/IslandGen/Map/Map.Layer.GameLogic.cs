using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Map {

    public partial class Layer {
        // GameLogic

        public Coord GetValidStartLocation()
        {

            var samplePoint = new Coord((int)(SizeX * 0.5f), (int)(SizeY * 0.5f));

            if (_map[samplePoint.TileX, samplePoint.TileY] < 0.001f)
            {
                return new Coord(samplePoint.TileX, samplePoint.TileY);
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
                        samplePoint.TileX--;
                    }
                    else
                    {
                        samplePoint.TileX++;
                    }

                    if (_map[samplePoint.TileX, samplePoint.TileY] < 0.001f)
                    {
                        return samplePoint;
                    }
                }

                for (int y = 0; y < yStep; y++)
                {
                    if (yNeg)
                    {
                        samplePoint.TileY--;
                    }
                    else
                    {
                        samplePoint.TileY++;
                    }

                    if (_map[samplePoint.TileX, samplePoint.TileY] < 0.001f)
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
    }
}
