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
    }
}
