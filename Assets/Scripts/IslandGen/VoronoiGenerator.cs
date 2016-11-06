using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class VoronoiGenerator {

    Map _map;
    Map _distanceMap;
    Map _heightMap;
    Map _falloffMap;
    List<VoronoiCell> _pointList;

    public VoronoiGenerator(Map map, int mapCoordinateX, int mapCoordinateY, float relativeDensity)
    {
        _map = map;

        RandomlyDistributeCells(relativeDensity);
        LinkMapPointsToCells();

    }

    public Map GetDistanceMap()
    {
        return Map.Clone(_distanceMap);
    }

    void RandomlyDistributeCells(float relativeDensity)
    {
        var outputList = new List<VoronoiCell>();

        var evenX = true;
        var evenY = true;

        var mapSizeX = _map.SizeX;
        var mapSizeY = _map.SizeY;

        var voronoiCountX = (int)((mapSizeX) * relativeDensity);
        var voronoiCountY = (int)((mapSizeY) * relativeDensity);

        var yOdd = (voronoiCountY % 2 != 0);

        var voronoiStepX = _map.SizeX / (float)voronoiCountX;
        var voronoiStepY = _map.SizeY / (float)voronoiCountY;

        var distribution = 1.5f;
        distribution = 0.5f * distribution;
        distribution = 0.5f * distribution * voronoiStepX;

        var buffer = 2f;

        for (float x = -(voronoiStepX* buffer); x < mapSizeX + (voronoiStepX * buffer); x += voronoiStepX)
        {
            evenX = evenX ? false : true;

            for (float y = -(voronoiStepY * buffer); y < mapSizeY + (voronoiStepY * buffer); y += voronoiStepY)
            {
                evenY = evenY ? false : true;

                if (yOdd)
                {
                    if (evenY == true)
                    {
                        outputList.Add(new VoronoiCell(new Vector2(x + RNG.NextFloat(-distribution, distribution), y + RNG.NextFloat(-distribution, distribution))));
                    }
                }
                else
                {
                    if (evenX)
                    {
                        if (evenY)
                        {
                            outputList.Add(new VoronoiCell(new Vector2(x + RNG.NextFloat(-distribution, distribution), y + RNG.NextFloat(-distribution, distribution))));
                        }
                    }
                    else
                    {
                        if (!evenY)
                        {
                            outputList.Add(new VoronoiCell(new Vector2(x + RNG.NextFloat(-distribution, distribution), y + RNG.NextFloat(-distribution, distribution))));
                        }
                    }
                }
            }
        }

        _pointList = outputList;
    }

    void LinkMapPointsToCells()
    {
        var distanceMap = Map.Clone(_map);

        for (int x = 0; x < _map.SizeX; x++)
        {

            for (int y = 0; y < _map.SizeY; y++)
            {

                var currentPos = new Vector2(x, y);



                var minDist = Mathf.Infinity;
                var closest = new VoronoiCell(new Vector2(float.MaxValue, float.MaxValue));

                for (var i = 0; i < _pointList.Count; i++)
                {
                    var dist = Vector2.Distance(_pointList[i].Position, currentPos);
                    if (dist < minDist)
                    {
                        closest = _pointList[i];
                        minDist = dist;
                    }
                }

                closest.MapPoints.Add(new Coord(x, y));

                distanceMap[x, y] = minDist;

            }
        }

        _distanceMap = distanceMap;
    }

    public Map GetHeightMap(Map sampleMap)
    {
        _heightMap = new Map(_map);

        for (int i = 0; i < _pointList.Count; i++)
        {

            var cell = _pointList[i];
            if (cell.MapPoints.Count == 0)
                continue;
            cell.Sort(_distanceMap);
            var center = cell.Center;
            var value = sampleMap[center.TileX, center.TileY];

            for (int p = 0; p < cell.MapPoints.Count; p++)
            {
                var point = cell.MapPoints[p];
                _heightMap[point.TileX, point.TileY] = value;
            }
        }

        return Map.Clone(_heightMap);
    }

    public Map GetFalloffMap()
    {
        _falloffMap = new Map(_map);

        for (int i = 0; i < _pointList.Count; i++)
        {

            var cell = _pointList[i];
            cell.SetFalloff(_falloffMap);
        }

        return Map.Clone(_falloffMap);
    }


    class VoronoiCell {

        public Vector2 Position
        { get; private set; }
        public List<Coord> MapPoints
        { get; private set; }
        public Coord Center
        { get; private set; }


        public VoronoiCell(Vector2 position)
        {
            Position = position;
            MapPoints = new List<Coord>();
        }

        public void Sort(Map _distanceMap)
        {
            var points = MapPoints.OrderBy(x => _distanceMap[x.TileX, x.TileY]).ToList();
            Center = points.First();
        }

        public void SetFalloff(Map bigMap)
        {
            var minX = int.MaxValue;
            var maxX = int.MinValue;

            var minY = int.MaxValue;
            var maxY = int.MinValue;

            for (int i = 0; i < MapPoints.Count; i++)
            {
                var p = MapPoints[i];

                if (p.TileX > maxX)
                    maxX = p.TileX;
                if (p.TileX < minX)
                    minX = p.TileX;

                if (p.TileY > maxY)
                    maxY = p.TileY;
                if (p.TileY < minY)
                    minY = p.TileY;
            }

            var map = new Map(maxX - minX, maxY - minY, 0);

            for (int i = 0; i < MapPoints.Count; i++)
            {
                var p = MapPoints[i];

                map[p.TileX - minX, p.TileY - minY] = 1;

            }

            map.GetDistanceMap(10);

            for (int i = 0; i < MapPoints.Count; i++)
            {
                var p = MapPoints[i];
                bigMap[p.TileX, p.TileY] = map[p.TileX - minX, p.TileY - minY];

            }



        }
    }



}
