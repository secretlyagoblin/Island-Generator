using System.Collections;
using System.Collections.Generic;
using U3D.Threading.Tasks;
using UnityEngine;

namespace Terrain {

    public class Region {

        TerrainData _terrainData;
        VoronoiPointBucketManager _voronoiData;
        RegionBucketManager _bucks;
        Chunk[,] _chunks;
        Rect[,] _rects;

        public Region(TerrainData terrainData, VoronoiPointBucketManager voronoiData)
        {
            _terrainData = terrainData;
            _voronoiData = voronoiData;
        }

        public void CreateChunks(int divisions, int mapSize)
        {
            _chunks = new Chunk[divisions, divisions];
            _rects = new Rect[divisions, divisions];

            var positionSize = _terrainData.Rect.height / divisions;
            var offsetSize = positionSize;// + (positionSize * (1f / mapSize));
            var offsetVector = new Vector2(offsetSize, offsetSize);

            for (int x = 0; x < divisions; x++)
            {
                for (int y = 0; y < divisions; y++)
                {
                    var pos = new Vector2(x * positionSize, y * positionSize);
                    var rect = new Rect(pos, offsetVector);

                    var voronoiOffset = new Vector2(positionSize / 4, positionSize / 4);

                    var voronoiRect = new Rect(pos - voronoiOffset, offsetVector + voronoiOffset + voronoiOffset);

                    var cellData = _voronoiData.GetSubChunk(voronoiRect);
                    var voronoiList = GetVoronoiCellsFromBuckets(cellData, voronoiRect);

                    var prebakeMapData = TerrainData.VoronoiPreBake(_terrainData, mapSize, rect);

                    var mapData = TerrainData.ChunkVoronoi(prebakeMapData, voronoiList, mapSize, rect);
                    //var mapData = TerrainData.PassThroughUntouched(_terrainData, new Coord(0, 0), mapSize, rect);

                    var chunk = new Chunk(mapData);
                    _chunks[x, y] = chunk;
                    _rects[x, y] = rect;
                }
            }
        }

        public void CreateMultithreadedChunks(int divisions, int mapSize, System.Action<Task> callback)
        {
            List <Task> tasks = new List<Task>();

            _chunks = new Chunk[divisions, divisions];
            _rects = new Rect[divisions, divisions];

            var positionSize = _terrainData.Rect.height / divisions;
            var offsetSize = positionSize;// + (positionSize * (1f / mapSize));
            var offsetVector = new Vector2(offsetSize, offsetSize);

            for (int x = 0; x < divisions; x++)
            {
                for (int y = 0; y < divisions; y++)
                {
                    var pos = new Vector2(x * positionSize, y * positionSize);
                    var rect = new Rect(pos, offsetVector);

                    var voronoiOffset = new Vector2(positionSize / 4, positionSize / 4);


                    var voronoiRect = new Rect(pos - voronoiOffset, offsetVector + voronoiOffset + voronoiOffset);

                    var cellData = _voronoiData.GetSubChunk(voronoiRect);

                    var prebakeMapData = TerrainData.VoronoiPreBake(_terrainData, mapSize, rect);

                    var localX = x;
                    var localY = y;

                    tasks.Add(Task.Run(() => {

                        var voronoiList = GetVoronoiCellsFromBuckets(cellData, voronoiRect);

                        var data = TerrainData.ChunkVoronoi(prebakeMapData, voronoiList, mapSize, rect);
                        var chunk = new Chunk(data);

                        lock (_chunks)
                        {
                            _chunks[localX, localY] = chunk;
                        }


                    }
                    
                    ));

                    _rects[x, y] = rect;
                }
            }

            Task.WhenAll(tasks.ToArray()).ContinueInMainThreadWith(callback);
        }

        List<VoronoiCell> GetVoronoiCellsFromBuckets(List<Buckets.Bucket<VoronoiCell>> points, Rect rect)
        {
            var outputCells = new List<VoronoiCell>();

            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];
            
                for (int u = 0; u < p.Elements.Count; u++)
                {
                    var point = p.Elements[u];
            
                    //Debug.DrawRay(point, Vector3.up*100f, color,100f);                
            
                    var x = Util.InverseLerpUnclamped(rect.position.x, rect.size.x, point.Position.x);
                    var z = Util.InverseLerpUnclamped(rect.position.y, rect.size.y, point.Position.y);

                    var cell = new VoronoiCell(new Vector3(x,point.Height, z));
                    cell.Inside = point.Inside;


                    outputCells.Add(cell);
                }
            }

            return outputCells;
        }



        public void CreateBucketSystem()
        {
            _bucks = new RegionBucketManager();
            _bucks.CreateBucketSystem(_chunks, _terrainData.Rect);
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

        public void InstantiateCollision(int decimationFactor)
        {
            for (int x = 0; x < _chunks.GetLength(0); x++)
            {
                for (int y = 0; y < _chunks.GetLength(1); y++)
                {
                    _chunks[x, y].AddCollision(decimationFactor, _rects[x,y]);
                }
            }
        }

        public void EnableCollision(Transform transform)
        {
            for (int x = 0; x < _chunks.GetLength(0); x++)
            {
                for (int y = 0; y < _chunks.GetLength(1); y++)
                {
                    _chunks[x, y].EnableCollision(transform);
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
    }
}