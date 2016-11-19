using UnityEngine;
using System.Collections.Generic;
using System;

public class InfiniteTerrain : MonoBehaviour {

    public const float MaxViewDistance = 400;
    public Transform Viewer;
    public Material TerrainMaterial;

    public static Vector2 _viewerPosition;
    int _chunkSize;
    int _chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    // Use this for initialization
    void Start()
    {
        RNG.Init(DateTime.Now.ToString());

        _chunkSize = 241 - 1;
        _chunksVisibleInViewDistance = Mathf.RoundToInt(MaxViewDistance / _chunkSize);

    }

    void Update()
    {
        _viewerPosition = new Vector2(Viewer.position.x, Viewer.position.z);
        updateVisibleChunks();
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


        int _currentLodCount = 4;
         int _currentLod = 0;

        Mesh[] _lods;

        MeshFilter _filter;

        public TerrainChunk(Vector2 coord, int size, Transform transform, Material terrainMaterial)
        {
            _currentLodCount = 4;
            _currentLod = 0;


            _lods = new Mesh[4];
            var mapSize = 240;


            _position = coord * size;

            for (int i = 0; i < _currentLodCount; i++)
            {
                var map = new Map(mapSize, mapSize).PerlinFillMap(3, new Domain(0.3f, 1.8f), new Coord((int)coord.x, (int)coord.y), new Vector2(0.5f, 0.5f), new Vector2(0, 0), 7, 0.5f, 1.87f).Clamp(1, 2f);

                _lods[i] = HeightmeshGenerator.GenerateHeightmeshPatch(map, new MeshLens(new Vector3(size, size, size))).CreateMesh();

                mapSize = (int)(mapSize * 0.5f);
            }



            _bounds = new Bounds(_position, Vector2.one * size);
            var positionVector3 = new Vector3(_position.x, 0, _position.y);



            _meshObject = new GameObject();

            _meshObject.transform.parent = transform;
            _meshObject.transform.position = positionVector3;
            //_meshObject.transform.localScale = Vector3.one * size * 0.1f;

            var renderer = _meshObject.AddComponent<MeshRenderer>();
            var material = new Material(terrainMaterial);
            //material.mainTexture = texture;
            material.color = new Color(material.color.r + (RNG.Next(-20, 20) * 0.01f), material.color.g, material.color.b + (RNG.Next(-20, 20) * 0.01f));
            renderer.material = material;
            _filter = _meshObject.AddComponent<MeshFilter>();
            var collider = _meshObject.AddComponent<MeshCollider>();

            collider.sharedMesh = _lods[1];
            _filter.mesh = _lods[_currentLodCount-1];
            _currentLod = _currentLodCount - 1;


            SetVisible(false);
        }

        public void UpdateTerrainChunk()
        {
            var viewerDistanceFromNearestEdge = Mathf.Sqrt(_bounds.SqrDistance(_viewerPosition));

            var part = Mathf.InverseLerp(0, MaxViewDistance, viewerDistanceFromNearestEdge);

            if(part < 1f)
            {
                var lod = (int)(part * _currentLodCount);
                _filter.mesh = _lods[lod];
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


    }

}