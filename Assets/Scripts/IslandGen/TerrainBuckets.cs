using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class TerrainBucketManager {

    TerrainBucket _bucket;
    Vector2 _testPos = new Vector2(0, 0);


    public void CreateBucketSystem(Rect rect, FinalChunk[,] TotalData)
    {
        var count = 0;
        var heightTest = TotalData.GetLength(0);
        

        while(heightTest > 1 && count < 20)
        {
            heightTest = heightTest / 2;
            count++;
        }

        if(heightTest != 1)
        {
            throw new System.Exception("Not a number that we can use for buckets");
        }
        else
        {
            _bucket = new TerrainBucket(rect, count, TotalData);
        }        
    }

    public void Update(Vector3 testPos, float distance)
    {
        var myPos = new Vector2(testPos.x, testPos.z);

        if (myPos != _testPos)
        {
            _testPos = myPos;
            _bucket.UpdateGraphics(_testPos, distance);
        }
    }

    class TerrainBucket {

        Rect _rect;
        Vector2 _lowerBounds;
        Vector2 _upperBounds;

        public bool CurrentIteration = false;
        public bool PreviousIteration = false;

        public MyTerrainData _myTerrainData = null;

        TerrainBucket[,] _buckets;

        bool _filled = false;
        bool _currentlyVisible = false;

        int _layer;
        int _maxLayer;

        public TerrainBucket(Rect rect, int maxLayer, FinalChunk[,] chunks)
        {
            Instantiate(0, maxLayer, rect);
            DistributeCellsDownward(chunks);
        }

        TerrainBucket(int layer, int maxLayer, Rect rect)
        {
            Instantiate(layer, maxLayer, rect);
        }

        void Instantiate(int layer, int maxLayer, Rect rect)
        {
            _rect = rect;
            _layer = layer;
            _maxLayer = maxLayer;

            var Colour = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f));

            _lowerBounds = rect.min;
            _upperBounds = rect.max;

            

            if (_layer != _maxLayer)
            {
                _filled = false;

                _buckets = new TerrainBucket[2, 2];

                //Debug.Log("Inheriting Rect of Size: " + _rect);

                var cellSize = _rect.size;
                cellSize.x = cellSize.x * 0.5f;
                cellSize.y = cellSize.y * 0.5f;

                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        var pos = _rect.position;
                        pos.x += (x * cellSize.x);
                        pos.y += (y * cellSize.y);

                        //Debug.Log("Creating Smaller Rect: " + new Rect(pos, cellSize));
                        _buckets[x, y] = new TerrainBucket(_layer+1,_maxLayer, new Rect(pos, cellSize));

                    }
                }

            }
            else
            {
                _filled = true;
            }
        }

        bool IsIn(Vector3 test)
        {
            return _rect.Contains(test);
        }

        public void DistributeCellsDownward(FinalChunk[,] chunks)
        {
            var size = chunks.GetLength(0);
            var split = (int)(size * 0.25);

            if (size == 1)
            {
                _myTerrainData = new MyTerrainData(chunks[0, 0]);



            } else{

                for (int sectionX = 0; sectionX < 2; sectionX++)
                {
                    for (int sectionY = 0; sectionY < 2; sectionY++)
                    {
                        var chunk = new FinalChunk[split, split];

                        for (int x = 0; x < split; x++)
                        {
                            for (int y = 0; y < split; y++)
                            {
                                chunk[x, y] = chunks[x + (sectionX * split), y + (sectionY * split)];
                            }
                        }
                        try
                        {


                            _buckets[sectionX, sectionY].DistributeCellsDownward(chunk);
                        }
                        catch
                        {
                            //Debug.Log("Woo!");
                        }
                    }
                }
            }

        }

        public void UpdateGraphics(Vector2 testPoint, float testDistance)
        {
            var distance = DistancePointToRectangle(testPoint, _rect);

            if(_layer == _maxLayer)
            {
                DrawRect();
                return;
            }

            if (distance < testDistance)
            {
                HideData();

                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        _buckets[x, y].UpdateGraphics(testPoint,testDistance);
                    }
                }

            }
            else if (_currentlyVisible == true)
            {
                DrawRect();
                //don't need to update anything
            }
            else
            {
                //DrawRect();
                ShowData();
                HideChildren();
            }
        }

        void ShowData()
        {
            _currentlyVisible = true;

            if(_myTerrainData == null)
            {
                _myTerrainData = new MyTerrainData(_buckets);
            }

            //_myTerrainData.ShowGameObject();
        }

        void DrawRect()
        {
            Debug.DrawLine(new Vector3(_rect.min.x, 0, _rect.min.y),new Vector3( _rect.min.x, 0, _rect.max.y),Color.white);
            Debug.DrawLine(new Vector3(_rect.min.x, 0, _rect.max.y), new Vector3(_rect.max.x, 0, _rect.max.y), Color.white);
            Debug.DrawLine(new Vector3(_rect.max.x, 0, _rect.max.y), new Vector3(_rect.max.x, 0, _rect.min.y), Color.white);
            Debug.DrawLine(new Vector3(_rect.max.x, 0, _rect.min.y), new Vector3(_rect.min.x, 0, _rect.min.y), Color.white);
        }

        void HideData()
        {
            _currentlyVisible = false;

            if (_myTerrainData != null)
            {
                _myTerrainData.HideGameObject();
            }
        }

        void HideChildren()
        {
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    _buckets[x, y].HideEverything();
                }
            }
        }

        void HideEverything()
        {
            HideData();

            if(_layer != _maxLayer)
            {
                HideChildren();
            }            
        }

        static float DistancePointToRectangle(Vector2 point, Rect rect)
        {
            //  Calculate a distance between a point and a rectangle.
            //  The area around/in the rectangle is defined in terms of
            //  several regions:
            //
            //  O--x
            //  |
            //  y
            //
            //
            //        I   |    II    |  III
            //      ======+==========+======   --yMin
            //       VIII |  IX (in) |  IV
            //      ======+==========+======   --yMax
            //       VII  |    VI    |   V
            //
            //
            //  Note that the +y direction is down because of Unity's GUI coordinates.

            if (point.x < rect.xMin)
            { // Region I, VIII, or VII
                if (point.y < rect.yMin)
                { // I
                    Vector2 diff = point - new Vector2(rect.xMin, rect.yMin);
                    return diff.magnitude;
                }
                else if (point.y > rect.yMax)
                { // VII
                    Vector2 diff = point - new Vector2(rect.xMin, rect.yMax);
                    return diff.magnitude;
                }
                else
                { // VIII
                    return rect.xMin - point.x;
                }
            }
            else if (point.x > rect.xMax)
            { // Region III, IV, or V
                if (point.y < rect.yMin)
                { // III
                    Vector2 diff = point - new Vector2(rect.xMax, rect.yMin);
                    return diff.magnitude;
                }
                else if (point.y > rect.yMax)
                { // V
                    Vector2 diff = point - new Vector2(rect.xMax, rect.yMax);
                    return diff.magnitude;
                }
                else
                { // IV
                    return point.x - rect.xMax;
                }
            }
            else
            { // Region II, IX, or VI
                if (point.y < rect.yMin)
                { // II
                    return rect.yMin - point.y;
                }
                else if (point.y > rect.yMax)
                { // VI
                    return point.y - rect.yMax;
                }
                else
                { // IX
                    return 0f;
                }
            }
        }
    }

    class MyTerrainData {

        public bool IsDerived { get; private set; }
        public Map Map { get
            {
                if(_map == null && IsDerived)
                {
                    _map = DeriveMap();
                }
                return _map;
            }
        }

        Map _map = null;

        Mesh _mesh;
        GameObject gobject = null;

        TerrainBucket[,] childBuckets;


        public MyTerrainData(TerrainBucket[,] buckets)
        {
            IsDerived = true;
            childBuckets = buckets;

        }

        public MyTerrainData(FinalChunk terrainData)
        {
            IsDerived = false;
            childBuckets = null;
            _map = terrainData.Map;
        }

        /*
        public void ShowGameObject()
        {
            if(gobject != null)
            {
                gobject.SetActive(true);
            }
            else
            {
                var map = Map;

                var meshGen = new HeightmeshGenerator();
                meshGen.GenerateHeightmeshPatch(map,new MeshLens(buc)) 
            }
        }
        */

        public void HideGameObject()
        {

        }

        Map DeriveMap()
        {
            return Map.CreateMapFromSubMapsAssumingOnePixelOverlap(new Map[,]
            {
                {
                    childBuckets[0, 0]._myTerrainData.Map,
                    childBuckets[0, 1]._myTerrainData.Map
                },
                {
                    childBuckets[1, 0]._myTerrainData.Map,
                    childBuckets[1, 1]._myTerrainData.Map
                }
            });
        }

        void CreateGameObject()
        {

        }

    }
}

class FinalChunk {
    public Map Map;
}

