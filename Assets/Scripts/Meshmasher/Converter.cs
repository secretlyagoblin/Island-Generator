using UnityEngine;
using System.Collections.Generic;
using BuildingGenerator;

namespace MeshMasher
{

    public static class Converter
    {
        public static BlueprintBuilder MeshToBuildingBlueprint(Mesh mesh)
        {
            var verts = mesh.vertices;
            var tris = mesh.triangles;

            var plateIndexList = new List<IndexCollection>();

            for (var i = 0; i < tris.Length; i += 3){
                plateIndexList.Add(new IndexCollection(new int[] { tris[i], tris[i + 1], tris[i + 2] }));
            }

            var index = new IndexCollection[] {
                new IndexCollection(new int[] { 0,1 })
            };

            return new BlueprintBuilder(verts, plateIndexList.ToArray(),index);
        }        
    }
}
