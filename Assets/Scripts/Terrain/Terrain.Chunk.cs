using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Terrain {

    public class Chunk {

        bool _walkableTerrain = false;
        TerrainData _data;
        Coord _coord;

        GameObject _object;
        GameObject _colliderObject;
        MeshCollider _collider;
        Mesh _collisionMesh;

        public TerrainData GetHeightmapData() {
            return _data;        
        }

        public Chunk(TerrainData data)
        {
            _data = data;
        }

        public void Instantiate(Transform parent, Material material)
        {
            var finalMesh = HeightmeshGenerator.GenerateAndFinaliseHeightMesh(_data);

            var gobject = new GameObject();
            gobject.transform.parent = parent;
            gobject.name = "TerrainChunk";
            gobject.transform.localPosition = new Vector3(_data.Rect.position.x, 0, _data.Rect.position.y);
            gobject.AddComponent<MeshRenderer>().sharedMaterial = material;
            gobject.AddComponent<MeshFilter>().sharedMesh = finalMesh;
            _object = gobject;
        }

        public void AddCollision(int decimationFactor, Rect rect)
        {
            var collisionData = TerrainData.CreateCollisionData(_data, decimationFactor, rect);
            _collisionMesh = HeightmeshGenerator.GenerateAndFinaliseHeightMesh(collisionData);
        }

        public void EnableCollision(Transform parent)
        {
            var gobject = new GameObject();
            gobject.transform.parent = parent;
            gobject.name = "TerrainCollision";
            gobject.transform.localPosition = new Vector3(_data.Rect.position.x, 0, _data.Rect.position.y);
            _colliderObject = gobject;

            _collider = _colliderObject.AddComponent<MeshCollider>();
            _collider.sharedMesh = _collisionMesh;

            _walkableTerrain = true;
        }

        public void InstantiateDummy(Transform parent, Material material)
        {
            var finalMesh = HeightmeshGenerator.GenerateAndFinaliseHeightMesh(_data);

            var gobject = new GameObject();
            gobject.transform.parent = parent;
            gobject.name = "DummyTerrain";
            gobject.transform.localPosition = new Vector3(_data.Rect.position.x, 0, _data.Rect.position.y);


            var meshRenderer = gobject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            meshRenderer.receiveShadows = false;
            gobject.AddComponent<MeshFilter>().sharedMesh = finalMesh;
            _object = gobject;
        }

        public void Hide()
        {
            _object.SetActive(false);

            if (_walkableTerrain)
            {
                _colliderObject.SetActive(false);
            }
        }

        public void Show()
        {
            _object.SetActive(true);

            if (_walkableTerrain)
            {
                _colliderObject.SetActive(true);
            }
        }
    }
}

