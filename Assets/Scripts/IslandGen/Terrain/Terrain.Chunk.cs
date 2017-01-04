using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Terrain {

    public class Chunk {

        bool _walkableTerrain;
        HeightmapData _data;
        Coord _coord;

        GameObject _object;

        public HeightmapData GetHeightmapData() {
            return _data;        
        }

        public Chunk(HeightmapData data)
        {
            _data = data;
        }

        public void Instantiate(Transform parent, Material material)
        {
            var finalMesh = HeightmeshGenerator.GenerateMesh(_data);

            var gobject = new GameObject();
            gobject.transform.position = new Vector3(_data.Rect.position.x, 0, _data.Rect.position.y);
            gobject.AddComponent<MeshRenderer>().sharedMaterial = material;
            gobject.AddComponent<MeshFilter>().sharedMesh = finalMesh;
            var col = gobject.AddComponent<MeshCollider>();
            col.sharedMesh = finalMesh;
            _object = gobject;
        }

        public void InstantiateDummy(Transform parent, Material material)
        {
            var finalMesh = HeightmeshGenerator.GenerateMesh(_data);

            var gobject = new GameObject();
            gobject.transform.position = new Vector3(_data.Rect.position.x, 0, _data.Rect.position.y);
            gobject.AddComponent<MeshRenderer>().sharedMaterial = material;
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

