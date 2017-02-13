using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Maps {

    public partial class Map {
        // General Functions

        int _subMapIndexX = 0;
        int _subMapIndexY = 0;

        public Map ExtractMap(Coord position, Coord size)
        {
            return ExtractMap(position.x, position.y, size.x, size.y);
        }

        public Map ExtractMap(int posX, int posY, int sizeX, int sizeY)
        {
            var map = new Map(sizeX, sizeY);

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    map[x, y] = _map[posX+x, posY+y];
                }
            }
            return map;
        }

        public Map ApplyMap(Map map, Coord position)
        {
            var mapSizeX = Mathf.Min(map.SizeX + position.x, SizeX);
            var mapSizeY = Mathf.Min(map.SizeY + position.y, SizeY);

            var posX = -1;
            var posY = -1;

            for (int x = position.x; x < mapSizeX; x++)
            {
                posX++;
                for (int y = position.y; y < mapSizeY; y++)
                {
                    posY++;

                    _map[x, y] = map[posX, posY];
                }
            }
            return this;
        }

        public Map Resize(int newSizeX, int newSizeY)
        {
            var map = new Map(newSizeX, newSizeY);

            for (int x = 0; x < newSizeX; x++)
            {
                for (int y = 0; y < newSizeY; y++)
                {
                    var normalisedX = Mathf.InverseLerp(0, newSizeX - 1, x);
                    var normalisedY = Mathf.InverseLerp(0, newSizeY - 1, y);

                    map[x, y] = BilinearSampleFromNormalisedVector2(new Vector2(normalisedX, normalisedY));
                }
            }
            return map;
        }

    }
}