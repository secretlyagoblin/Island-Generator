using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DetailObjectPool : MonoBehaviour {

	public GameObject TestPoint;
    public GameObject GrassObject;
    public float RegionSize;
    public int VectorCount;
    public float TestDistance;
    public float NoiseScale;

    DetailObjectBucketManager<DetailObjectData> _detailObjectManager;


	Queue<GameObject> _freeObjects = new Queue<GameObject>();
	Dictionary<DetailObjectData,GameObject> _dict = new Dictionary<DetailObjectData, GameObject>();
    MaterialPropertyBlock _block;

    // Use this for initialization
    void Start () {

        RNG.DateTimeInit();
        _block = new MaterialPropertyBlock();
        _detailObjectManager = new DetailObjectBucketManager<DetailObjectData> (Vector2.zero, new Vector2 (RegionSize, RegionSize));

        //Add a bunch of DetailObjects to the manager

		for (int i = 0; i < VectorCount; i++) {
            var vec = new Vector3(RNG.NextFloat(0, RegionSize), RNG.NextFloat(0, RegionSize), RNG.NextFloat(0, RegionSize));

            if (Mathf.PerlinNoise(vec.x * NoiseScale, vec.z * NoiseScale) < 0.5f & (Mathf.PerlinNoise(vec.x * (NoiseScale * 4), vec.z * (NoiseScale * 4)) < 0.5f | RNG.Next(0, 100) < 0.5f))
            {
                _detailObjectManager.AddElement(CreateGrassData(vec), new Vector2(vec.x, vec.z));
            }       
		}
	}


	// Update is called once per frame
	void Update () {

        //Update the detail object manager

		_detailObjectManager.Update (new Vector2 (TestPoint.transform.position.x,TestPoint.transform.position.z), TestDistance);

        // Hide the objects exiting the pool and remove them from the dict

        for (int i = 0; i < _detailObjectManager.ObjectsExitingPool.Count; i++)
        {
            var objectData = _detailObjectManager.ObjectsExitingPool[i];
            var obj = _dict[objectData];
            obj.SetActive(false);
            _freeObjects.Enqueue(obj);
            _dict.Remove(objectData);
        }

        // Either show or instantiate new detail objects

        for (int i = 0; i < _detailObjectManager.ObjectsEnteringPool.Count; i++)
        {
            var objectData = _detailObjectManager.ObjectsExitingPool[i];

            if (_freeObjects.Count > 0)
            {
                var obj = _freeObjects.Dequeue();

                obj.SetActive(true);
                objectData.ApplyTransformData(obj.transform);
                objectData.ApplyColorData(obj.transform, _block);

                _dict.Add(objectData, obj);
            }
            else
            {
                var obj = objectData.Instantiate(GrassObject);
                objectData.ApplyColorData(obj.transform, _block);

                _dict.Add(objectData, obj);
            }
            
        }
	}

    DetailObjectData CreateGrassData(Vector3 position)
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
        var color = new Color(tint * 0.5f, tint, -tint + 1f);

        return new DetailObjectData(pos, Quaternion.AngleAxis(rot, Vector3.up), new Vector3(scale, scale, scale), color);

    }

    class DetailObjectData {

        //public Transform Transform;

        Vector3 _positon;
        Quaternion _rotation;
        Vector3 _scale;
        Color _color;

        public DetailObjectData(Vector3 position, Quaternion rotation, Vector3 scale, Color color)
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

