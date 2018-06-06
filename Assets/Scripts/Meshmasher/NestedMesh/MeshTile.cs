using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MeshMasher.MeshTiling {

    public class MeshTile {

        public Vector3[] Positions;
        public SimpleVector2Int[] Offsets;

        public int[] Triangles;
        public Vector3[] Centers;
        public SimpleVector2Int[] CenterOffsets;

        public int[][] ScaledVerts;
        public SimpleVector2Int[][] ScaledOffsets;

        //public int[][] ScaledTrianglesOwnedByPoints;

        public Vector3[][] Barycenters;
        public int[][] NestedTriangleIndexes;
        public SimpleVector2Int[][] NestedTriangleOffsets;

        public MeshTile(string meshTileJSON)
        {
            PopulateFromString(meshTileJSON);
        }

        private void PopulateFromString(string str)
        {
            var importObject = JsonUtility.FromJson<ImportJsonObject>(str);

            //Positions
            Positions = new Vector3[importObject.positions.Length / 2];

            for (int i = 0; i < importObject.positions.Length; i += 2)
            {
                Positions[i / 2] = new Vector3(importObject.positions[i], importObject.positions[i + 1], 0f);
            }

            //Triangles
            Triangles = importObject.triangles.Clone() as int[];

            //Offsets
            Offsets = new SimpleVector2Int[importObject.offsets.Length / 2];

            for (int i = 0; i < importObject.offsets.Length; i += 2)
            {
                Offsets[i / 2] = new SimpleVector2Int(importObject.offsets[i], importObject.offsets[i + 1]);
            }

            //TriangleCenters
            Centers = new Vector3[Triangles.Length / 3];
            CenterOffsets = new SimpleVector2Int[Triangles.Length / 3];

            for (int i = 0; i < Centers.Length; i++)
            {
                var centerListOffset = i * 2;
                Centers[i] = new Vector3(importObject.triangleCenters[centerListOffset], importObject.triangleCenters[centerListOffset + 1]);
                CenterOffsets[i] = new SimpleVector2Int(importObject.triangleCenterOffsets[centerListOffset], importObject.triangleCenterOffsets[centerListOffset + 1]);
            }

            //InnerIndexList

            ScaledVerts = new int[importObject.innerIndexList.Length][];
            ScaledOffsets = new SimpleVector2Int[importObject.innerIndexList.Length][];

            //ScaledTrianglesOwnedByPoints= public int[][]

            Barycenters = new Vector3[importObject.innerIndexList.Length][];
            NestedTriangleIndexes = new int[importObject.innerIndexList.Length][];
            NestedTriangleOffsets = new SimpleVector2Int[importObject.innerIndexList.Length][];

            var count = 0;

            for (int u = 0; u < importObject.innerIndexList.Length; u++)
            {
                var innerCount = importObject.innerIndexList[u];

                ScaledVerts[u] = new int[innerCount];
                ScaledOffsets[u] = new SimpleVector2Int[innerCount];

                Barycenters[u] = new Vector3[innerCount];
                NestedTriangleIndexes[u] = new int[innerCount];
                NestedTriangleOffsets[u] = new SimpleVector2Int[innerCount];

                for (int v = 0; v < innerCount; v++)
                {
                    ScaledVerts[u][v] = importObject.culledPointIndexes[count];
                    ScaledOffsets[u][v] = new SimpleVector2Int(importObject.culled2dPointOffsets[count * 2], importObject.culled2dPointOffsets[(count * 2) + 1]);

                    Barycenters[u][v] = new Vector3(importObject.barycentricCoordinates[count * 3], importObject.barycentricCoordinates[(count * 3) + 1], importObject.barycentricCoordinates[(count * 3) + 2]);
                    NestedTriangleIndexes[u][v] = importObject.culledTriangleIndexes[count];
                    NestedTriangleOffsets[u][v] = new SimpleVector2Int(importObject.culled2dTriangleOffsets[count * 2], importObject.culled2dTriangleOffsets[(count * 2) + 1]);

                    count++;
                }
            }
        }

        private class ImportJsonObject {
            public int[] triangles;
            public int[] offsets;
            public float[] positions;
            public float[] triangleCenters;
            public int[] triangleCenterOffsets;
            public int[] innerIndexList;
            public int[] culled2dTriangleOffsets;
            public int[] culledTriangleIndexes;
            public int[] culled2dPointOffsets;
            public int[] culledPointIndexes;
            public float[] barycentricCoordinates;
            public float[] innerBarycentricCoordinates;
            public int[] innerBarycentricIndices;
        }
    }
}