using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Terrain {

    public class Chunk {

        bool _walkableTerrain;
        HeightmapData _data;
        Coord _coord;

        GameObject _object;
        MeshCollider _collider;
        Mesh _collisionMesh;

        public HeightmapData GetHeightmapData() {
            return _data;        
        }

        public Chunk(HeightmapData data)
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
            var collisionData = HeightmapData.CreateCollisionData(_data, decimationFactor, rect);
            _collisionMesh = HeightmeshGenerator.GenerateAndFinaliseHeightMesh(collisionData);
        }

        public void EnableCollision()
        {
            _collider = _object.AddComponent<MeshCollider>();
            _collider.sharedMesh = _collisionMesh;
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
        }

        public void Show()
        {
            _object.SetActive(true);
        }
    }
}

