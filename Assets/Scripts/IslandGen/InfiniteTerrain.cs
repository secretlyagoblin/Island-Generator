using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;

public class InfiniteTerrain : MonoBehaviour {

    public const float MaxViewDistance = 2000;
    public Transform Viewer;
    public Material TerrainMaterial;

    public static Vector2 _viewerPosition;
    int _chunkSize;
    int _chunksVisibleInViewDistance;

    static Queue<ThreadData<MapCreationData>> _mapThread = new Queue<ThreadData<MapCreationData>>();

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    // Use this for initialization
    void Start()
    {
        RNG.Init(DateTime.Now.ToString());

        _chunkSize = 500;
        _chunksVisibleInViewDistance = Mathf.RoundToInt(MaxViewDistance / _chunkSize);

    }

    void Update()
    {
        _viewerPosition = new Vector2(Viewer.position.x, Viewer.position.z);
        dequeueNewLODS();
        updateVisibleChunks();
    }

    void dequeueNewLODS()
    {
        if (_mapThread.Count == 0)
            return;

        for (int i = 0; i < _mapThread.Count; i++)
        {
            var data = _mapThread.Dequeue();
            data.Callback(data.Parameter);
            Debug.Log("I worked");
        }



    }

    void updateVisibleChunks()
    {

        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }

        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(_viewerPosition.x / _chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(_viewerPosition.y / _chunkSize);

        for (int yOffset = -_chunksVisibleInViewDistance; yOffset <= _chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -_chunksVisibleInViewDistance; xOffset <= _chunksVisibleInViewDistance; xOffset++)
            {
                var viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    if (terrainChunkDictionary[viewedChunkCoord].IsVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, _chunkSize, transform, TerrainMaterial));
                }
            }
        }
    }

    public class TerrainChunk {

        GameObject _meshObject;
        Vector2 _position;
        Bounds _bounds;

        bool _currentState;

        Coord _coord;

        int _size;


        int _currentLodCount = 4;
        int _currentLod = -1;

        int _mapSize = 240;

        Mesh[] _lods;
        LODstatus[] _lodStatus;


        MeshFilter _filter;
        MeshCollider _collider;

        public TerrainChunk(Vector2 coord, int size, Transform transform, Material terrainMaterial)
        {
            _currentLod = 0;
            _coord = new Coord((int)coord.x, (int)coord.y);

            _size = size;
            _lods = new Mesh[_currentLodCount];
            _lodStatus = new LODstatus[_currentLodCount];

            for (int i = 0; i < _currentLodCount; i++)
            {
                _lodStatus[i] = LODstatus.NotCreated;
            }

            _position = coord * size;

            _bounds = new Bounds(_position, Vector2.one * size);
            var positionVector3 = new Vector3(_position.x, 0, _position.y);

            _meshObject = new GameObject();

            _meshObject.transform.parent = transform;
            _meshObject.transform.position = positionVector3;
            //_meshObject.transform.localScale = Vector3.one * size * 0.1f;

            var renderer = _meshObject.AddComponent<MeshRenderer>();
            var material = terrainMaterial;
            //material.mainTexture = texture;
            //material.color = new Color(material.color.r + (RNG.Next(-20, 20) * 0.01f), material.color.g, material.color.b + (RNG.Next(-20, 20) * 0.01f));
            renderer.sharedMaterial = material;
            _filter = _meshObject.AddComponent<MeshFilter>();
            _collider = _meshObject.AddComponent<MeshCollider>();

            CreateLOD(_currentLodCount-1);


        }

        public void UpdateTerrainChunk()
        {
            var viewerDistanceFromNearestEdge = Mathf.Sqrt(_bounds.SqrDistance(_viewerPosition));

            var part = Mathf.InverseLerp(0, MaxViewDistance*0.6f, viewerDistanceFromNearestEdge);

            if(part < 1f)
            {
                var lod = (int)(part * _currentLodCount);

                if (_currentLod != lod)
                {

                    if (_lodStatus[lod] == LODstatus.NotCreated)
                    {
                        CreateLOD(lod);
                    }
                    else if (_lodStatus[lod] == LODstatus.Created)
                    {
                        _filter.mesh = _lods[lod];
                        _collider.sharedMesh = _lods[lod];

                        _currentLod = lod;
                    }
                }
            }




            var visible = viewerDistanceFromNearestEdge <= MaxViewDistance;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            _meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return _meshObject.activeSelf;
        }

        void CreateLOD(int LOD)
        {
            var t = Mathf.InverseLerp(0, _currentLodCount-1, LOD);
            var mapSize = -((int)Mathf.Lerp(-_mapSize, -15, t));
            _lodStatus[LOD] = LODstatus.InProgress;
            //Debug.Log("LOD: " + LOD + ", Map Size: " + mapSize);
            RequestMap(ImplimentLOD, mapSize, LOD);
            
        }

        void ImplimentLOD(MapCreationData mapCreationData)
        {
            var heightMeshGenerator = new HeightmeshGenerator();
            _lods[mapCreationData.LOD] = heightMeshGenerator.GenerateHeightmeshPatch(mapCreationData.Map, new MeshLens(new Vector3(_size, _size, _size))).CreateMesh();

            _collider.sharedMesh = _lods[mapCreationData.LOD];
            _filter.mesh = _lods[mapCreationData.LOD];
            _currentLod = mapCreationData.LOD;
            _lodStatus[mapCreationData.LOD] = LODstatus.Created;

            _currentState = false;
            SetVisible(false);
        }

        void RequestMap(Action<MapCreationData> callback, int mapSize, int lod)
        {
            ThreadStart threadStart = delegate
             {
                 MapThread(callback, mapSize, lod);
             };

            new Thread(threadStart).Start();
        }

        void MapThread(Action<MapCreationData> callback, int mapSize, int lod)
        {
            var map = new Map(mapSize, mapSize).PerlinFillMap(3, new Domain(0.3f, 1.8f), _coord, new Vector2(0.5f, 0.5f), new Vector2(0, 0), 7, 0.5f, 1.87f).Clamp(1, 2f);
            //var mesh = new HeightmeshGenerator()
            //Should generate mesh in here
            lock (_mapThread)
            {
                _mapThread.Enqueue(new ThreadData<MapCreationData>(callback, new MapCreationData(lod, map)));
            }
        }


    }

    struct ThreadData<T> {
        public readonly Action<T> Callback;
        public readonly T Parameter;

        public ThreadData(Action<T> callback, T parameter)
        {
            Callback = callback;
            Parameter = parameter;
        }

    }

    struct MapCreationData {
        public readonly int LOD;
        public readonly Map Map;

        public MapCreationData(int lod, Map map)
        {
            LOD = lod;
            Map = map;
        }
    }

    enum LODstatus {
        NotCreated,InProgress,Created 
    };

}