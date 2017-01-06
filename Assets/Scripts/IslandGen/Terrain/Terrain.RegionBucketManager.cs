using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Map;

namespace Terrain {

    class RegionBucketManager {

        RegionBucket _bucket;
        Vector2 _testPos = new Vector2(0, 0);

        Chunk[,] _chunkData;

        GameObject _prefab;


        public void CreateBucketSystem(Chunk[,] chunkData, Rect rect)
        {
            _chunkData = chunkData;

            var count = 0;

            var heightTest = _chunkData.GetLength(0);


            while (heightTest > 1 && count < 20)
            {
                heightTest = heightTest / 2;
                count++;
            }

            if (heightTest != 1)
            {
                throw new System.Exception("Not a number that we can use for buckets");
            }
            else
            {
                //Debug.Log(count);
                _bucket = new RegionBucket(chunkData, count, rect);
            }
        }

        public void InstantiateDummyRegions(Transform transform, Material material)
        {
            _bucket.GenerateLODTree(material, transform);
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

        class RegionBucket {

            Rect _rect;
            Vector2 _lowerBounds;
            Vector2 _upperBounds;

            public bool CurrentIteration = false;
            public bool PreviousIteration = false;

            public Chunk Chunk = null;

            RegionBucket[,] _children;


            bool _filled = false;
            bool _currentlyVisible = false;

            int _layer;
            int _maxLayer;
           
            public RegionBucket(Chunk[,] chunks, int maxLayer, Rect rect)
            {
                _maxLayer = maxLayer;
                Instantiate(0, _maxLayer, rect);
                DistributeCellsDownward(chunks);
            }

            RegionBucket(int layer, int maxLayer, Rect rect)
            {
                Instantiate(layer, maxLayer, rect);
            }

            void Instantiate(int layer, int maxLayer, Rect rect)
            {
                _rect = rect;
                _layer = layer;
                _maxLayer = maxLayer;

                var Colour = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f));

                _lowerBounds = _rect.min;
                _upperBounds = _rect.max;



                if (_layer != _maxLayer)
                {
                    _filled = false;

                    _children = new RegionBucket[2, 2];

                    //Debug.Log("Inheriting Rect of Size: " + _rect);

                    var cellSize = new Vector2(_rect.size.x*0.5f, _rect.size.y*0.5f);

                    for (int x = 0; x < 2; x++)
                    {
                        for (int y = 0; y < 2; y++)
                        {
                            var pos = new Vector2(_rect.position.x + (x * cellSize.x), (_rect.position.y + (y * cellSize.y)));

                            //Debug.Log("Creating Smaller Rect: " + new Rect(pos, cellSize));
                            _children[x, y] = new RegionBucket(_layer + 1,maxLayer, new Rect(pos, cellSize));

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

            public void DistributeCellsDownward(Chunk[,] chunks)
            {
                //We aren't linking up the right data here

                var size = chunks.GetLength(0);
                var split = size / 2;

                //Debug.Log("Size: "+ size);
                //Debug.Log("Split: " + split);

                if (_layer == _maxLayer)
                {
                    Chunk = chunks[0, 0]; 
                }
                else
                {
                    for (int sectionX = 0; sectionX < 2; sectionX++)
                    {
                        for (int sectionY = 0; sectionY < 2; sectionY++)
                        {
                            var chunk = new Chunk[split, split];

                            for (int x = 0; x < split; x++)
                            {
                                for (int y = 0; y < split; y++)
                                {
                                    chunk[x, y] = chunks[x + (sectionX * split), y + (sectionY * split)];
                                }
                            }

                                _children[sectionX, sectionY].DistributeCellsDownward(chunk);

                        }
                    }
                }
            }

            public void GenerateLODTree(Material material, Transform transform)
            {
                if(_layer == _maxLayer)
                {
                    return;
                }

                var data = new HeightmapData[2, 2];

                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        if(_children[x,y].Chunk == null)
                        {
                            _children[x, y].GenerateLODTree(material, transform);
                        }

                        data[x,y] = _children[x, y].Chunk.GetHeightmapData();
                    }
                }
                Chunk = new Chunk(HeightmapData.DummyMap(data,_rect));
                Chunk.InstantiateDummy(transform, material);
            }

            public void UpdateGraphics(Vector2 testPoint, float testDistance)
            {
                var distance = DistancePointToRectangle(testPoint, _rect);

                if (_layer == _maxLayer)
                {
                    Chunk.Show();
                    return;
                }

                if (distance < testDistance)
                {
                    Chunk.Hide();

                    for (int x = 0; x < 2; x++)
                    {
                        for (int y = 0; y < 2; y++)
                        {
                            _children[x, y].UpdateGraphics(testPoint, testDistance *0.75f);
                        }
                    }

                }
                else if (_currentlyVisible == true)
                {
                    //DrawRect();
                    //don't need to update anything
                }
                else
                {
                    //DrawRect();
                    Chunk.Show();
                    HideChildren();
                }
            }

            void DrawRect()
            {
                Debug.DrawLine(new Vector3(_rect.min.x, 0, _rect.min.y), new Vector3(_rect.min.x, 0, _rect.max.y), Color.white);
                Debug.DrawLine(new Vector3(_rect.min.x, 0, _rect.max.y), new Vector3(_rect.max.x, 0, _rect.max.y), Color.white);
                Debug.DrawLine(new Vector3(_rect.max.x, 0, _rect.max.y), new Vector3(_rect.max.x, 0, _rect.min.y), Color.white);
                Debug.DrawLine(new Vector3(_rect.max.x, 0, _rect.min.y), new Vector3(_rect.min.x, 0, _rect.min.y), Color.white);
            }

            void HideChildren()
            {
                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        _children[x, y].HideEverything();
                    }
                }
            }

            void HideEverything()
            {
                Chunk.Hide();

                if (_layer != _maxLayer)
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
    }


}


