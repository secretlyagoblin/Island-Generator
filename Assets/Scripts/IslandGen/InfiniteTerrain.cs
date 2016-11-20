using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;

public class InfiniteTerrain : MonoBehaviour {

    public const float MaxViewDistance = 2000;
    public Transform Viewer;
    public Material TerrainMaterial;

    public static Vector2 _viewerPosition;
    public static HeightmeshGenerator _heightmeshGenerator = new HeightmeshGenerator();
    int _chunkSize;
    int _chunksVisibleInViewDistance;

    static Queue<ThreadData<MapCreationData>> _mapThread = new Queue<ThreadData<MapCreationData>>();

    Dictionary<Coord, TerrainChunk> terrainChunkDictionary = new Dictionary<Coord, TerrainChunk>();

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
        DequeueNewLODS();
        updateVisibleChunks();
    }

    void DequeueNewLODS()
    {
        if (_mapThread.Count == 0)
            return;

        for (int i = 0; i < Math.Min(_mapThread.Count,2); i++)
        {
            var data = _mapThread.Dequeue();
            data.Callback(data.Parameter);
            //Debug.Log("I worked");
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
                var viewedChunkCoord = new Coord(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();

                    var cellAbove = new Coord(viewedChunkCoord.TileX, viewedChunkCoord.TileY + 1);
                    var cellCount = 0;

                    if (terrainChunkDictionary.ContainsKey(cellAbove))
                    {
                        terrainChunkDictionary[viewedChunkCoord].UpdateTopSeam(terrainChunkDictionary[cellAbove].CurrentMap, terrainChunkDictionary[cellAbove].CurrentLod);
                        cellCount++;
                    }

                    var cellBeside = new Coord(viewedChunkCoord.TileX + 1, viewedChunkCoord.TileY);

                    if (terrainChunkDictionary.ContainsKey(cellBeside))
                    {
                        terrainChunkDictionary[viewedChunkCoord].UpdateRightSeam(terrainChunkDictionary[cellBeside].CurrentMap, terrainChunkDictionary[cellBeside].CurrentLod);
                        cellCount++;
                    }

                    var cellDiagonal = new Coord(viewedChunkCoord.TileX + 1, viewedChunkCoord.TileY+1);

                    if (terrainChunkDictionary.ContainsKey(cellDiagonal) && cellCount == 2)
                    {
                        terrainChunkDictionary[viewedChunkCoord].UpdateCorner(terrainChunkDictionary[cellAbove], terrainChunkDictionary[cellBeside], terrainChunkDictionary[cellDiagonal]);
                    }




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

        Coord _coord;

        int _size;

        public Map CurrentMap{
            get
            {
                if (CurrentLod == -1)
                    return null;
                if (_lodStatus[CurrentLod] == LODstatus.Created)
                    return _maps[CurrentLod]; 
                return null;

            }
        }

        MeshSeam _topSeam;
        MeshSeam _rightSeam;


        int _currentLodCount = 6;
        public int CurrentLod = -1;

        int _mapSize = 240;
        int _mapMinSize = 10;

        Map[] _maps;

        Mesh[] _lods;
        LODstatus[] _lodStatus;


        MeshFilter _filter;
        //MeshCollider _collider;

        public TerrainChunk(Coord coord, int size, Transform transform, Material terrainMaterial)
        {
            CurrentLod = -1;
            _coord = coord;

            _size = size;
            _lods = new Mesh[_currentLodCount];
            _lodStatus = new LODstatus[_currentLodCount];
            _maps = new Map[_currentLodCount];

            for (int i = 0; i < _currentLodCount; i++)
            {
                _lodStatus[i] = LODstatus.NotCreated;
            }

            _position = new Vector2(coord.TileX * size, coord.TileY * size);

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
            //_collider = _meshObject.AddComponent<MeshCollider>();

            //UpdateTerrainChunk();

            //CreateLOD(_currentLodCount-1);

            _topSeam = new MeshSeam(-1, -1, _meshObject.transform, terrainMaterial);
            _rightSeam = new MeshSeam(-1, -1, _meshObject.transform, terrainMaterial);


        }

        
        public void SetVisible(bool visible)
        {
            _meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return _meshObject.activeSelf;
        }

        // Mesh Chunk Data

        public void UpdateTerrainChunk()
        {
            var viewerDistanceFromNearestEdge = Mathf.Sqrt(_bounds.SqrDistance(_viewerPosition));

            var part = Mathf.InverseLerp(0, MaxViewDistance * 0.8f, viewerDistanceFromNearestEdge);

            if (part < 1f)
            {
                var lod = (int)(part * _currentLodCount);

                if (CurrentLod != lod)
                {

                    if (_lodStatus[lod] == LODstatus.NotCreated)
                    {
                        CreateLOD(lod);
                    }
                    else if (_lodStatus[lod] == LODstatus.Created)
                    {
                        _filter.mesh = _lods[lod];
                        //_collider.sharedMesh = _lods[lod];

                        CurrentLod = lod;
                    }
                }
            }




            var visible = viewerDistanceFromNearestEdge <= MaxViewDistance;
            SetVisible(visible);
        }

        void CreateLOD(int LOD)
        {
            var t = Mathf.InverseLerp(0, _currentLodCount-1, LOD);
            var mapSize = -((int)Mathf.Lerp(-_mapSize, -_mapMinSize, t));
            _lodStatus[LOD] = LODstatus.InProgress;
            //Debug.Log("LOD: " + LOD + ", Map Size: " + mapSize);
            RequestMap(ImplimentLOD, mapSize, LOD);
            
        }

        void ImplimentLOD(MapCreationData mapCreationData)
        {
            
            _maps[mapCreationData.LOD] = mapCreationData.Map;
            _lods[mapCreationData.LOD] = mapCreationData.MeshPatch.CreateMesh();

            //_collider.sharedMesh = _lods[mapCreationData.LOD];
            _filter.mesh = _lods[mapCreationData.LOD];
            CurrentLod = mapCreationData.LOD;
            _lodStatus[mapCreationData.LOD] = LODstatus.Created;

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
            var heightMeshGenerator = new HeightmeshGenerator();
            var meshPatch = heightMeshGenerator.GenerateHeightmeshPatch(map, new MeshLens(new Vector3(_size, _size, _size)));
            //var mesh = new HeightmeshGenerator()
            //Should generate mesh in here
            lock (_mapThread)
            {
                _mapThread.Enqueue(new ThreadData<MapCreationData>(callback, new MapCreationData(lod,map, meshPatch)));
            }
        }

        // Mesh Seam Data

        public void UpdateTopSeam(Map mapB, int LOD)
        {
            if (CurrentMap == null | mapB == null)
                return;

            if (_topSeam.NeedsUpdate(CurrentLod, LOD))
            {
                UpdateSeam(mapB, new Coord(_coord.TileX, _coord.TileY + 1),_topSeam);
            }

        }

        public void UpdateRightSeam(Map mapB, int LOD)
        {
            if (CurrentMap == null | mapB == null)
                return;

            if (_rightSeam.NeedsUpdate(CurrentLod, LOD))
            {
                UpdateSeam(mapB, new Coord(_coord.TileX+1, _coord.TileY), _rightSeam);
            }
        }

        void UpdateSeam(Map mapB, Coord coordB, MeshSeam seam)
        {
            //Debug.Log("Seam needs updating");

            var mesh = _heightmeshGenerator.GenerateMeshSeam(_maps[CurrentLod], _coord, mapB, coordB, new MeshLens(new Vector3(_size, _size, _size)));
            seam.ApplyMesh(mesh.CreateMesh());
        }

        // Corner

        public void UpdateCorner(TerrainChunk mapA, TerrainChunk mapB, TerrainChunk mapC)
        {

        }

        struct MeshSeam {
            int homeTileLOD;
            int awayTileLOD;

            GameObject _gameObject;
            MeshFilter _filter;

            public MeshSeam(int homeLOD, int awayLOD, Transform parent, Material terrianMaterial)
            {
                homeTileLOD = homeLOD;
                awayTileLOD = awayLOD;

                _gameObject = new GameObject();

                _gameObject.transform.parent = parent;
                _gameObject.transform.localPosition = Vector3.zero;
                //_meshObject.transform.localScale = Vector3.one * size * 0.1f;

                var renderer = _gameObject.AddComponent<MeshRenderer>();
                var material = terrianMaterial;
                //material.mainTexture = texture;
                //material.color = new Color(material.color.r + (RNG.Next(-20, 20) * 0.01f), material.color.g, material.color.b + (RNG.Next(-20, 20) * 0.01f));
                renderer.sharedMaterial = material;
                _filter = _gameObject.AddComponent<MeshFilter>();
            }

            public bool NeedsUpdate(int homeLOD, int awayLOD)
            {
                var returnValue = false;
                if(homeLOD != homeTileLOD)
                {
                    homeTileLOD = homeLOD;
                    returnValue = true;
                }

                if (awayLOD != awayTileLOD)
                {
                    awayTileLOD = awayLOD;
                    returnValue = true;
                }

                return returnValue;
            }

            public void ApplyMesh(Mesh mesh)
            {
                _filter.mesh = mesh;
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
        public readonly HeightmeshPatch MeshPatch;
        public readonly Map Map;

        public MapCreationData(int lod, Map map, HeightmeshPatch meshPatch)
        {
            LOD = lod;
            Map = map;
            MeshPatch = meshPatch;
        }
    }

    enum LODstatus {
        NotCreated,InProgress,Created 
    };

}