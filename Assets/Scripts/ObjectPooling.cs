using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ObjectPooling : MonoBehaviour {

	public GameObject TestPoint;

	Bucket<Vector3> _bucket;

	Bucket<Vector3>[] _previousBuckets;

	Queue<GameObject> _freeObjects = new Queue<GameObject>();

	Dictionary<Vector3,GameObject> _dict = new Dictionary<Vector3,GameObject>();

	// Use this for initialization
	void Start () {

		_bucket = new Bucket<Vector3> (0, Vector2.zero, new Vector2 (30, 30));

		for (int i = 0; i < 5000; i++) {
            var vec = new Vector3(Random.Range(0, 30f), Random.Range(0, 30f), Random.Range(0, 0f));
            _bucket.AddElement (vec, vec);
		}

		_previousBuckets = new Bucket<Vector3>[0];
	}
	
	// Update is called once per frame
	void Update () {

		//_bucket.AddPoint (new Vector3 (Random.Range (0, 30f), Random.Range (0, 30f), Random.Range (0, 0f)));

		var currentBuckets = _bucket.GetBuckets (new Vector2 (TestPoint.transform.position.x,TestPoint.transform.position.y), 3f);

		var storedObjects = 0;

		for (int i = 0; i < _previousBuckets.Length; i++) {
			if (_previousBuckets [i].PreviousIteration && _previousBuckets [i].CurrentIteration) {
			} else {
				for (int u = 0; u < _previousBuckets [i].Points.Count; u++) {
					var obj = _dict [_previousBuckets [i].Points [u]];
					obj.SetActive (false);
					_freeObjects.Enqueue (obj);
					_dict.Remove (_previousBuckets[i].Points[u]);
					storedObjects++;
				}				
				_previousBuckets [i].PreviousIteration = false;
			}
		}

		if (storedObjects > 0)
			Debug.Log ("Stored " + storedObjects + " Objects");

		var newObjects = 0;
		var retrievedObjects = 0;

		for (int i = 0; i < currentBuckets.Length; i++) {
			var bucket = currentBuckets [i];

			if (bucket.PreviousIteration) {
				
			} else {

				for (int u = 0; u < bucket.Points.Count; u++) {
					var point = bucket.Points [u];

					if (_freeObjects.Count > 0) {
						var obj = _freeObjects.Dequeue ();
						obj.SetActive(true);
						obj.transform.position = point;
						_dict.Add(point,obj);
						retrievedObjects++;
					
					} else {
						var obj = GameObject.CreatePrimitive (PrimitiveType.Sphere);
						obj.SetActive(true);
						obj.transform.position = point;
						_dict.Add(point,obj);
						newObjects++;
					}
				}
			}

			bucket.PreviousIteration = true;
			bucket.CurrentIteration = false;
		}

		if (retrievedObjects > 0)
			Debug.Log ("Reused " + retrievedObjects + " Objects");
		if (newObjects > 0)
			Debug.Log ("Created " + newObjects + " new Objects");

		_previousBuckets = currentBuckets;	
	}
}

