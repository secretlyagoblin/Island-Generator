using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Terrain {

    public class Region {

        HeightmapData _data;
        RegionBucketManager _bucks;
        Chunk[,] _chunks;

        public Region(HeightmapData data)
        {
            _data = data;

            var divisions = 8;
            var mapSize = 70;

            _chunks = CreateChunks(divisions, mapSize);

            _bucks = new RegionBucketManager();
            _bucks.CreateBucketSystem(_chunks, _data.Rect);
        }

        public void InstantiateRegionCells(Transform transform, Material material)
        {
            for (int x = 0; x < _chunks.GetLength(0); x++)
            {
                for (int y = 0; y < _chunks.GetLength(1); y++)
                {
                    _chunks[x, y].Instantiate(transform, material);
                }
            }
        }

        public void InstantiateDummyCells(Transform transform, Material material)
        {
            _bucks.InstantiateDummyRegions(transform, material);
        }

        public void Update(Vector3 testPosition, float distance)
        {
            _bucks.Update(testPosition, distance);
        }

        Chunk[,] CreateChunks(int divisions, int mapSize)
        {
            var chunks = new Chunk[divisions, divisions];

            var positionSize = _data.Rect.height / divisions;
            var offsetSize = positionSize;// + (positionSize * (1f / mapSize));
            var offsetVector = new Vector2(offsetSize, offsetSize);

            for (int x = 0; x < divisions; x++)
            {
                for (int y = 0; y < divisions; y++)
                {                    
                    var pos = new Vector2(x * positionSize, y * positionSize);
                    var rect = new Rect(pos, offsetVector);
                    var mapData = HeightmapData.ChunkVoronoi(_data, new Coord(x,y), mapSize, rect);

                    var chunk = new Chunk(mapData);
                    chunks[x, y] = chunk;
                }
            }

            return chunks;
        }
    }
}