using UnityEngine;
using System.Collections.Generic;
using Terrain;
using System.Linq;
using U3D.Threading.Tasks;

public class DetailObjectPool : MonoBehaviour {

	public GameObject TestPoint;
    public GameObject GrassObject;
    public int PropCount;
    public int DetailDivisions;
    public float RegionSize;
    public float TestDistance;
    public float NoiseScale;

    public Gradient ColourGradient;

    DetailObjectBucketManager<DetailObjectData> _detailObjectManager;

	Queue<GameObject> _freeObjects = new Queue<GameObject>();
	Dictionary<DetailObjectData,GameObject> _dict = new Dictionary<DetailObjectData, GameObject>();
    MaterialPropertyBlock _block;

    // Use this for initialization
    void Start () {

        RNG.DateTimeInit();
        _block = new MaterialPropertyBlock();
        _detailObjectManager = new DetailObjectBucketManager<DetailObjectData> (DetailDivisions, Vector2.zero, new Vector2 (RegionSize, RegionSize));

        //Add a bunch of DetailObjects to the manager

	}

    Terrain.TerrainData _terrainData;

    public void SetPhysicalMap(Terrain.TerrainData data)
    {
        _terrainData = data;
    }

    public void InitPositions()
    {
        for (int i = 0; i < PropCount; i++)
        {
            var vec2 = new Vector2(RNG.NextFloat(0, 1), RNG.NextFloat(0, 1));

            if (_terrainData.WalkableMap.BilinearSampleFromNormalisedVector2(vec2) != 0)
            {
                continue;
            }

            var vec = new Vector3(vec2.x*RegionSize, _terrainData.HeightMap.BilinearSampleFromNormalisedVector2(vec2), vec2.y* RegionSize);

            if (Mathf.PerlinNoise(vec.x * NoiseScale, vec.z * NoiseScale) < 0.5f & (Mathf.PerlinNoise(vec.x * (NoiseScale * 4), vec.z * (NoiseScale * 4)) < 0.5f | RNG.Next(0, 100) < 1f))
            {
                var color = _terrainData.ColorSampleAtPoint(vec2);

                AddPosition(vec,color.grayscale);
                //Debug.DrawRay(vec, Vector3.right, Color.green, 100f);
            }
        }
    }

    public void AddPosition(Vector3 position, float gradientPosition)
    {
        _detailObjectManager.AddElement(CreateGrassData(position, gradientPosition), new Vector2(position.x, position.z));
    }

    bool _needsUpdate = true;
    Vector2 _testVector;
    Vector2 _previousTestVector = Vector2.zero;

    // Update is called once per frame
    void Update () {

        //Update the detail object manager

        _testVector = new Vector2(TestPoint.transform.position.x, TestPoint.transform.position.z);

        if (_needsUpdate && _testVector != _previousTestVector )
        {
            _needsUpdate = false;
            _previousTestVector = _testVector;
            
            var task = Task.Run(UpdateDetailObjectManager).ContinueInMainThreadWith(ApplyObjectManagerChanges);
        }
	}

    void UpdateDetailObjectManager()
    {
        _detailObjectManager.Update(_testVector, TestDistance);
    }

    void ApplyObjectManagerChanges(Task t) // Modify to happen in batches
    {
        StartCoroutine(ApplyObjectManagerChangesCoroutine(1000,150));
    }

    System.Collections.IEnumerator ApplyObjectManagerChangesCoroutine(int iterationSizeRemove, int iterationSizeAdd)
    {
        var listComplete = false;
        var startIndex = 0;

        while (!listComplete)
        {
            listComplete = RemoveOldObjects(startIndex, iterationSizeRemove);
            startIndex += iterationSizeRemove;
            yield return null;
        }

        listComplete = false;
        startIndex = 0;

        while (!listComplete)
        {
            listComplete = AddNewObjects(startIndex, iterationSizeAdd);
            startIndex += iterationSizeAdd;
            yield return null;
        }

        //Debug.Log("Created all grass, retriggering update...");

        _needsUpdate = true;
    }

    bool RemoveOldObjects(int startIndex, int iterations)
    {
        var maxCount = startIndex + iterations;
        var exitPoolCount = _detailObjectManager.ObjectsExitingPool.Count;

        var isWholeListIteratedOver = false;

        if (startIndex + iterations >= exitPoolCount)
        {
            maxCount = exitPoolCount;
            isWholeListIteratedOver = true;
        }

        for (int i = startIndex; i < maxCount; i++)
        {
            var objectData = _detailObjectManager.ObjectsExitingPool[i];
            var obj = _dict[objectData];
            obj.SetActive(false);
            _freeObjects.Enqueue(obj);
            _dict.Remove(objectData);
        }

        return isWholeListIteratedOver;
    }

    bool AddNewObjects(int startIndex, int iterations)
    {
        var maxCount = startIndex + iterations;
        var entryPoolCount = _detailObjectManager.ObjectsEnteringPool.Count;

        var isWholeListIteratedOver = false;

        if (startIndex + iterations >= entryPoolCount)
        {
            maxCount = entryPoolCount;
            isWholeListIteratedOver = true;
        }

        for (int i = startIndex; i < maxCount; i++)
        {
            var objectData = _detailObjectManager.ObjectsEnteringPool[i];

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
                var obj = objectData.Instantiate(GrassObject, transform);
                objectData.ApplyColorData(obj.transform, _block);

                _dict.Add(objectData, obj);
            }
        }

        return isWholeListIteratedOver;
    }

    DetailObjectData CreateGrassData(Vector3 position, float gradientPosition)
    {
        var pos = position;
        var rot = RNG.NextFloat(0, 360);

        var scale = (RNG.NextFloat(0.6f, 1.4f));
        scale = scale * ((Mathf.PerlinNoise(pos.x * 0.1f, pos.z * 0.1f)) + 0.5f);
        scale = scale * 0.32f;

        if (RNG.Next(0, 1000) < 1)
        {
            scale = scale * 3f;
        }

        var tint = ColourGradient.Evaluate(gradientPosition);
        //tint = (tint * 0.5f) + 0.5f;

        return new DetailObjectData(pos, Quaternion.AngleAxis(rot, Vector3.up), new Vector3(scale, scale, scale), tint);

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

        public GameObject Instantiate(GameObject baseObject, Transform parent)
        {
            var obj = Object.Instantiate(baseObject, _positon, _rotation, parent);
            obj.transform.localScale = _scale;

            return obj;
        }

    }
}

