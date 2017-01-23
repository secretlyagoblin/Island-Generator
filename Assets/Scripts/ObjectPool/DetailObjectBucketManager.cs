using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buckets;

public class DetailObjectBucketManager<T> {

    Bucket<T> _bucket;
    Bucket<T>[] _previousBuckets;
    Bucket<T>[] _currentBuckets;

    public List<T> ObjectsExitingPool;
    public List<T> ObjectsEnteringPool;

    public DetailObjectBucketManager(Vector2 lowerBounds, Vector2 upperBounds)
    {

        _bucket = new Bucket<T>(0, lowerBounds, upperBounds);
        _previousBuckets = new Bucket<T>[0];

        ObjectsEnteringPool = new List<T>();
        ObjectsExitingPool = new List<T>();
    }

    public void AddElement(T element, Vector2 position)
    {
        _bucket.AddElement(element, position);
    }

    public void Update(Vector2 testPosition, float testDistance)
    {
        var currentBuckets = _bucket.GetBuckets(testPosition, testDistance);

        GetObjectsExitingPool();
        GetObjectsEnteringPool();
    }

    void GetObjectsExitingPool()
    {
        ObjectsExitingPool.Clear();

        for (int i = 0; i < _previousBuckets.Length; i++)
        {
            if (_previousBuckets[i].PreviousIteration && _previousBuckets[i].CurrentIteration)
            {
            }
            else
            {
                for (int u = 0; u < _previousBuckets[i].Elements.Count; u++)
                {
                    ObjectsExitingPool.Add(_previousBuckets[i].Elements[u]);
                }

                _previousBuckets[i].PreviousIteration = false;
            }
        }
    }

    void GetObjectsEnteringPool()
    {
        ObjectsEnteringPool.Clear();

        for (int i = 0; i < _currentBuckets.Length; i++)
        {
            var bucket = _currentBuckets[i];

            if (bucket.PreviousIteration)
            {

            }
            else
            {
                for (int u = 0; u < bucket.Elements.Count; u++)
                {
                    ObjectsEnteringPool.Add(bucket.Elements[u]);
                }
            }

            bucket.PreviousIteration = true;
            bucket.CurrentIteration = false;
        }


    }

}
