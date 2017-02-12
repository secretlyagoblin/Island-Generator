using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Map;
using Buckets;

public class VoronoiGenerator {

    Layer _map;
    Layer _distanceMap;
    Layer _heightMap;
    Layer _falloffMap;
    List<VoronoiCell> _allElements;
    Bucket<VoronoiCell> _buckets;

    Coord _coord;

    float _seed;

    public static VoronoiGenerator Generator = null; 

    public VoronoiGenerator(Layer map, List<VoronoiCell> cells)
    {
        _map = map;
        _allElements = cells;

        var color = new Color(RNG.NextFloat(), RNG.NextFloat(), RNG.NextFloat());

        _buckets = new Bucket<VoronoiCell>(10, new Rect(new Vector2(-0.5f, -0.5f), new Vector2(2f, 2f)));

        for (int i = 0; i < _allElements.Count; i++)
        {
            var cell = _allElements[i];
            //Debug.DrawRay(new Vector3(cell.Position.x, cell.Height, cell.Position.y), Vector3.up * 100f, color, 100f);
            _buckets.AddElement(cell, cell.Position);
        }



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

        for (int i = 0; i < _allElements.Count; i++)
        {

            var cell = _allElements[i];

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

        for (int i = 0; i < _allElements.Count; i++)
        {

            var cell = _allElements[i];

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
    /*
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
                
            }
        }

        _cells = outputList;
    }
    */
    void LinkMapPointsToCells()
    {
        var distanceMap = Layer.Clone(_map);


        for (int x = 0; x < _map.SizeX; x++)
        {

            for (int y = 0; y < _map.SizeY; y++)
            {
                var currentPos = _map.GetNormalisedVector3FromIndex(x, y);

                //if (Vector2.Distance(currentPos,lastPos) < lastPosDistance)
                //{
                //    currentPos = lastPos;
                //    bucketList = lastCells;
                //}
                //else
                //{
                var    bucketList = _buckets.GetBucketsWithinRangeOfPoint(currentPos, 0.05f);
                //}

                var minDist = Mathf.Infinity;
                var closest = new VoronoiCell(new Vector2(float.MaxValue, float.MaxValue));

                for (int b = 0; b < bucketList.Count; b++)
                {
                    var bucket = bucketList[b];

                    for (var i = 0; i < bucket.Elements.Count; i++)
                    {
                        var cell = bucket.Elements[i];
                        var dist = Vector2.Distance(cell.Position, currentPos);
                        if (dist < minDist)
                        {
                            closest = cell;

                            minDist = dist;
                        }
                    }
                }


                //if (!closestDistanceAssigned)
                //{
                //    closestDistanceAssigned = true;
                //    closestDistanceAverage = minDist;
                //}
                //else
                //{
                //    closestDistanceAverage += minDist;
                //    closestDistanceCount++;
                //}

                closest.MapPoints.Add(new Coord(x, y));

                if (minDist == Mathf.Infinity)
                {
                    minDist = 0;
                    //Debug.Log("Warning: Voronoi Cells are too sparse");
                }

                distanceMap[x, y] = minDist;

            }
        }

        //Debug.Log("Average Distance " + closestDistanceAverage);

        _distanceMap = distanceMap;
    }

    public Layer GetHeightMap(Layer sampleMap)
    {
        _heightMap = new Layer(_map);


            for (var i = 0; i < _allElements.Count; i++)
            {

                var cell = _allElements[i];
                if (cell.MapPoints.Count == 0)
                continue;


            for (int p = 0; p < cell.MapPoints.Count; p++)
            {
                var point = cell.MapPoints[p];
                _heightMap[point.TileX, point.TileY] = cell.Height;
            }
        }

        return Layer.Clone(_heightMap);
    }

    public Layer GetFalloffMap(int searchDistance)
    {
        _falloffMap = new Layer(_map);

        for (int i = 0; i < _allElements.Count; i++)
        {

            var cell = _allElements[i];
            if (cell.MapPoints.Count == 0)
                continue;

            cell.SetFalloff(_falloffMap, searchDistance);
        }

        return Layer.Clone(_falloffMap);
    }

}

public class VoronoiCell {

    public Vector2 Position
    { get; private set; }
    public float Height
    { get; private set; }
    public List<Coord> MapPoints
    { get; private set; }
    public Coord Center
    { get; private set; }


    public VoronoiCell(Vector3 position)
    {
        Position = new Vector2(position.x, position.z);
        Height = position.y;
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

        var map = new Layer(maxX - minX + 1 + buffer, maxY - minY + 1 + buffer, 0);

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
