using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ObjectPooling : MonoBehaviour {

	public GameObject TestPoint;
    public GameObject GrassObject;
    public float RegionSize;
    public int VectorCount;
    public float TestDistance;
    public float NoiseScale;

	Bucket<GrassData> _bucket;
	Bucket<GrassData>[] _previousBuckets;
	Queue<GameObject> _freeObjects = new Queue<GameObject>();
	Dictionary<GrassData,GameObject> _dict = new Dictionary<GrassData, GameObject>();
    MaterialPropertyBlock _block;

    // Use this for initialization
    void Start () {

        RNG.DateTimeInit();

        _block = new MaterialPropertyBlock();

        _bucket = new Bucket<GrassData> (0, Vector2.zero, new Vector2 (RegionSize, RegionSize));

		for (int i = 0; i < VectorCount; i++) {
            var vec = new Vector3(RNG.NextFloat(0, RegionSize), RNG.NextFloat(0, RegionSize), RNG.NextFloat(0, RegionSize));

            if (Mathf.PerlinNoise( vec.x* NoiseScale, vec.z * NoiseScale)<0.5f)
            {
                if (Mathf.PerlinNoise(vec.x * (NoiseScale*4), vec.z * (NoiseScale * 4)) < 0.5f)
                {
                    _bucket.AddElement(CreateGrassData(vec), new Vector2(vec.x,vec.z));
                }
                else if(RNG.Next(0,100)<1){
                    _bucket.AddElement(CreateGrassData(vec), new Vector2(vec.x, vec.z));
                }
            }            
		}

		_previousBuckets = new Bucket<GrassData>[0];
	}

    GrassData CreateGrassData(Vector3 position)
    {
        var pos = new Vector3(position.x, 0, position.z);
        var rot = RNG.NextFloat(0, 360);

        var scale = (RNG.NextFloat(0.6f, 1.4f));
        scale = scale * ((Mathf.PerlinNoise(pos.x * 0.1f, pos.z * 0.1f)) + 0.5f);
        scale = scale * 0.32f;

        if (RNG.Next(0, 1000) < 1)
        {
            scale = scale * 2f;
        }

        var tint = Mathf.PerlinNoise(pos.x * 0.4f, pos.z * 0.4f);
        var color = new Color(tint*0.5f, tint, -tint+1f);

        return new GrassData(pos, Quaternion.AngleAxis(rot, Vector3.up), new Vector3(scale, scale, scale), color);

    }
	
	// Update is called once per frame
	void Update () {

		//_bucket.AddPoint (new Vector3 (Random.Range (0, 30f), Random.Range (0, 30f), Random.Range (0, 0f)));

		var currentBuckets = _bucket.GetBuckets (new Vector2 (TestPoint.transform.position.x,TestPoint.transform.position.z), TestDistance);
		var storedObjects = 0;

		for (int i = 0; i < _previousBuckets.Length; i++) {
			if (_previousBuckets [i].PreviousIteration && _previousBuckets [i].CurrentIteration) {
			} else {
				for (int u = 0; u < _previousBuckets [i].Elements.Count; u++) {
					var obj = _dict [_previousBuckets [i].Elements[u]];
					obj.SetActive (false);
					_freeObjects.Enqueue (obj);
					_dict.Remove (_previousBuckets[i].Elements[u]);
					storedObjects++;
				}				
				_previousBuckets [i].PreviousIteration = false;
			}
		}

		var newObjects = 0;
		var retrievedObjects = 0;

		for (int i = 0; i < currentBuckets.Length; i++) {
			var bucket = currentBuckets [i];

			if (bucket.PreviousIteration) {
                				
			} else {
				for (int u = 0; u < bucket.Elements.Count; u++) {
					var grassData = bucket.Elements[u];

					if (_freeObjects.Count > 0) {

						var obj = _freeObjects.Dequeue ();

                        obj.SetActive(true);
                        grassData.ApplyTransformData(obj.transform);
                        grassData.ApplyColorData(obj.transform, _block);      
                          
                        _dict.Add(grassData,obj);
						retrievedObjects++;					

					} else {
                        var obj = grassData.Instantiate(GrassObject);
                        grassData.ApplyColorData(obj.transform, _block);
                        
                        _dict.Add(grassData,obj);
						newObjects++;
					}
				}
			}

			bucket.PreviousIteration = true;
			bucket.CurrentIteration = false;
		}

        /*

        if (retrievedObjects > 0)
        {
            //Debug.Log ("Reused " + retrievedObjects + " Objects");
        }
        if (newObjects > 0)
        {
            //Debug.Log ("Created " + newObjects + " new Objects");
        }

    */

		_previousBuckets = currentBuckets;	
	}

    class GrassData {

        //public Transform Transform;

        Vector3 _positon;
        Quaternion _rotation;
        Vector3 _scale;
        Color _color;

        public GrassData(Vector3 position, Quaternion rotation, Vector3 scale, Color color)
        {
            _positon = position;
            _rotation = rotation;
            _scale = scale;
            _color = color;
        }

        public void ApplyTransformData(Transform transform)
        {
            transform.localPosition = _positon;
            transform.localRotation = _rotation;
            transform.localScale = _scale;
        }

        public void ApplyColorData(Transform transform, MaterialPropertyBlock block)
        {
            block.SetColor("_Color", _color);
            transform.GetComponentInChildren<MeshRenderer>().SetPropertyBlock(block);
        }

        public GameObject Instantiate(GameObject baseObject)
        {
            var obj = Object.Instantiate(baseObject, _positon,_rotation);
            obj.transform.localScale = _scale;

            return obj;
        }

    }
}

