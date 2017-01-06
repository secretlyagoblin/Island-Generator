using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Map;

public class VoronoiGenerator {

    Layer _map;
    Layer _distanceMap;
    Layer _heightMap;
    Layer _falloffMap;
    List<VoronoiCell> _pointList;

    Coord _coord;

    float _seed;

    public static VoronoiGenerator Generator = null; 

    public VoronoiGenerator(Layer map, int mapCoordinateX, int mapCoordinateY, float relativeDensity, float seed)
    {
        _map = map;
        _seed = seed;
        _coord = new Coord(mapCoordinateX, mapCoordinateY);

        GetPointsIgnoringResolution(relativeDensity, 2, relativeDensity*0.75f);

        //RandomlyDistributeCells(relativeDensity);
        LinkMapPointsToCells();

    }

    public Layer GetDistanceMap()
    {
        return Layer.Clone(_distanceMap);
    }

    public Layer GetSmattering(int chance)
    {
        var map = new Layer(_map.SizeX, _map.SizeY, 1);

        _distanceMap.Normalise();

        for (int i = 0; i < _pointList.Count; i++)
        {

            var cell = _pointList[i];

            if(RNG.Next(0, chance) == 0)
            {
                for (int p = 0; p < cell.MapPoints.Count; p++)
                {
                    var point = cell.MapPoints[p];

                    map[point.TileX, point.TileY] = _distanceMap[point.TileX, point.TileY];
                }
            }



        }
        return map;
    }

    public Layer GetVoronoiBoolMap(Layer insideMap)
    {
        var map = new Layer(_map.SizeX, _map.SizeY, 1);

        for (int i = 0; i < _pointList.Count; i++)
        {

            var cell = _pointList[i];

            if (cell.MapPoints.Count == 0)
                continue;

            cell.Sort(_distanceMap);

            var center = cell.Center;

            if(Mathf.Abs(insideMap[center.TileX,center.TileY]) <0.00001)
            {
                for (int p = 0; p < cell.MapPoints.Count; p++)
                {
                    var point = cell.MapPoints[p];


                    map[point.TileX, point.TileY] = 0;
                }
            }
        }
        return map;
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

                var perlin = Mathf.PerlinNoise(x + _seed, y + _seed)*Mathf.PI*2;

                var moveX = Mathf.Sin(perlin)*distribution;
                var moveY = Mathf.Cos(perlin)*distribution;

                outputList.Add(new VoronoiCell(new Vector2(x + moveX, y + moveY)));

                Debug.Log(new Vector2(moveX, moveY));
                Debug.Log(new Vector2(moveX, moveY).magnitude);

                /*
                if (yOdd)
                {
                    if (evenY == true)
                    {
                        outputList.Add(new VoronoiCell(new Vector2(x + Mathf.Sin(Mathf.PerlinNoise(x+_seed,y+_seed)),y + Mathf.Cos(Mathf.PerlinNoise(x + _seed, y + _seed)))));
                    }
                }
                else
                {
                    if (evenX)
                    {
                        if (evenY)
                        {
                            //outputList.Add(new VoronoiCell(new Vector2(x + RNG.NextFloat(-distribution, distribution), y + RNG.NextFloat(-distribution, distribution))));
                            outputList.Add(new VoronoiCell(new Vector2(x + Mathf.Sin(Mathf.PerlinNoise(x + _seed, y + _seed)), y + Mathf.Cos(Mathf.PerlinNoise(x + _seed, y + _seed)))));
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
                */
            }
        }

        _pointList = outputList;
    }

    void LinkMapPointsToCells()
    {
        var distanceMap = Layer.Clone(_map);

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

    public Layer GetHeightMap(Layer sampleMap)
    {
        _heightMap = new Layer(_map);

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

        return Layer.Clone(_heightMap);
    }

    public Layer GetFalloffMap(int searchDistance)
    {
        _falloffMap = new Layer(_map);

        for (int i = 0; i < _pointList.Count; i++)
        {

            var cell = _pointList[i];
            if (cell.MapPoints.Count == 0)
                continue;

            cell.SetFalloff(_falloffMap, searchDistance);
        }

        return Layer.Clone(_falloffMap);
    }


    void GetPointsIgnoringResolution(float sizeRelativeTo, int buffer, float percentageWarped)
    {
        _pointList = new List<VoronoiCell>();
        var cellStep = 1f / sizeRelativeTo;

        var intStartX = Mathf.FloorToInt(cellStep * _coord.TileX) - buffer;
        var intStartY = Mathf.FloorToInt(cellStep * _coord.TileY) - buffer;

        var intEndX = Mathf.CeilToInt(cellStep * (_coord.TileX+1)) + buffer;
        var intEndY = Mathf.CeilToInt(cellStep * (_coord.TileY+1)) + buffer;

        for (int x = intStartX; x < intEndX; x++)
        {
            for (int y = intStartY; y < intEndY; y++)
            {
                var vector = new Vector2(x * sizeRelativeTo, y * sizeRelativeTo);

                var offset = Mathf.PerlinNoise(x + _seed, y + _seed);

                var offsetVector = Vector2.zero;

                offsetVector.x = Mathf.Cos(offset * Mathf.PI * 2);
                offsetVector.x = Mathf.Sin(offset * Mathf.PI * 2);

                vector += (offsetVector * percentageWarped);

                vector.x -= _coord.TileX;
                vector.y -= _coord.TileY;

                vector.x *= _map.SizeX;
                vector.y *= _map.SizeY;






                _pointList.Add(new VoronoiCell(vector));
            }
        }
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

        public void Sort(Layer _distanceMap)
        {
            var points = MapPoints.OrderBy(x => _distanceMap[x.TileX, x.TileY]).ToList();
            Center = points.First();
        }

        public void SetFalloff(Layer bigMap, int searchDistance)
        {
            var minX = int.MaxValue;
            var maxX = int.MinValue;

            var minY = int.MaxValue;
            var maxY = int.MinValue;

            var buffer = 3;

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

            var map = new Layer(maxX - minX+1 + buffer, maxY - minY+1 + buffer, 0);

            for (int i = 0; i < MapPoints.Count; i++)
            {
                var p = MapPoints[i];

                map[p.TileX - minX + buffer, p.TileY - minY + buffer] = 1;

            }

            map.SmoothMap(searchDistance);

            //map.GetDistanceMap(searchDistance);

            for (int i = 0; i < MapPoints.Count; i++)
            {
                var p = MapPoints[i];
                bigMap[p.TileX, p.TileY] = map[p.TileX - minX + buffer, p.TileY - minY + buffer];

            }



        }
    }



}
