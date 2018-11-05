using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MeshMasher;

public class MeshTile {

    public int Scale;
    public int NestingScale;

    public Vector3[] Positions;
    public SimpleVector2Int[] Offsets;

    public int[] Triangles;
    public Vector3[] Centers;
    public SimpleVector2Int[] CenterOffsets;

    public int[][] ScaledVerts;
    public SimpleVector2Int[][] SubTileOffsets;

    //public int[][] ScaledTrianglesOwnedByPoints;

    public Barycenter[][] Barycenters;
    public int[][] NestedTriangleIndexes;
    public Barycenter[][] TriangleCenterBarycenters;
    public SimpleVector2Int[][] TriangleSubTileOffsets;

    public Barycenter[][] TriangleBarycentricContainment;

    public MeshTile(string meshTileJSON)
    {
        PopulateFromString(meshTileJSON);
    }

    private void PopulateFromString(string str)
    {
        var importObject = JsonUtility.FromJson<ImportJsonObject>(str);

        //Metadata
        Scale = importObject.gridScale;
        NestingScale = importObject.nestingScale;

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

        ScaledVerts = new int[importObject.innerPointIndexList.Length][];
        SubTileOffsets = new SimpleVector2Int[importObject.innerPointIndexList.Length][];
        Barycenters = new Barycenter[importObject.innerPointIndexList.Length][];

        //ScaledTrianglesOwnedByPoints= public int[][]            

        var count = 0;

        for (int u = 0; u < importObject.innerPointIndexList.Length; u++)
        {
            var innerCount = importObject.innerPointIndexList[u];

            ScaledVerts[u] = new int[innerCount];
            SubTileOffsets[u] = new SimpleVector2Int[innerCount];

            Barycenters[u] = new Barycenter[innerCount];


            for (int v = 0; v < innerCount; v++)
            {
                ScaledVerts[u][v] = importObject.nestedPointIndexes[count];
                SubTileOffsets[u][v] = new SimpleVector2Int(importObject.nested2dPointOffsets[count * 2], importObject.nested2dPointOffsets[(count * 2) + 1]);

                Barycenters[u][v] = new Barycenter(importObject.barycentricCoordinates[count * 3], importObject.barycentricCoordinates[(count * 3) + 1], importObject.barycentricCoordinates[(count * 3) + 2], true, false);

                count++;
            }
        }

        count = 0;

        NestedTriangleIndexes = new int[importObject.innerTrianglePointIndexList.Length][];
        TriangleSubTileOffsets = new SimpleVector2Int[importObject.innerTrianglePointIndexList.Length][];
        TriangleCenterBarycenters = new Barycenter[importObject.innerTrianglePointIndexList.Length][];

        for (int u = 0; u < importObject.innerTrianglePointIndexList.Length; u++)
        {
            var innerCount = importObject.innerTrianglePointIndexList[u];

            NestedTriangleIndexes[u] = new int[innerCount];
            TriangleSubTileOffsets[u] = new SimpleVector2Int[innerCount];
            TriangleCenterBarycenters[u] = new Barycenter[innerCount * 3];

            for (int v = 0; v < innerCount; v++)
            {
                NestedTriangleIndexes[u][v] = importObject.nestedTriangleIndexes[count];
                TriangleSubTileOffsets[u][v] = new SimpleVector2Int(importObject.nested2dTriangleOffsets[count * 2], importObject.nested2dTriangleOffsets[(count * 2) + 1]);

                var bc = count * 3;

                TriangleCenterBarycenters[u][v] = new Barycenter(importObject.triangleCenterBarycenters[bc], importObject.triangleCenterBarycenters[bc + 1], importObject.triangleCenterBarycenters[bc + 2], true, false);
                count++;
            }
        }

        count = 0;

        TriangleBarycentricContainment = new Barycenter[importObject.triangleAccessBarcenticListLength.Length][];

        for (int u = 0; u < importObject.triangleAccessBarcenticListLength.Length; u++)
        {
            var innerCount = importObject.triangleAccessBarcenticListLength[u];
            TriangleBarycentricContainment[u] = new Barycenter[innerCount];

            for (int v = 0; v < innerCount; v++)
            {
                TriangleBarycentricContainment[u][v] = new Barycenter(
                        //u,
                        importObject.triangleAccessBarycenters[count * 3],
                        importObject.triangleAccessBarycenters[count * 3 + 1],
                        importObject.triangleAccessBarycenters[count * 3 + 2],
                        importObject.triangleAccessBarcenticInternalMask[count],
                        importObject.triangleAccessEdgeMask[count]
                    );

                count++;
            }
        }
    }

    private class ImportJsonObject {
        public int gridScale;
        public int nestingScale;
        public int[] triangles;
        public int[] offsets;
        public float[] positions;
        public float[] triangleCenters;
        public int[] triangleCenterOffsets;
        public int[] innerPointIndexList;
        public int[] innerTrianglePointIndexList;
        public int[] nested2dTriangleOffsets;
        public int[] nestedTriangleIndexes;
        public int[] nested2dPointOffsets;
        public int[] nestedPointIndexes;
        public float[] barycentricCoordinates;
        public float[] innerBarycentricCoordinates;
        public int[] innerBarycentricIndices;
        public int[] triangleAccessBarcenticListLength;
        public float[] triangleAccessBarycenters;
        public bool[] triangleAccessBarcenticInternalMask;
        public bool[] triangleAccessEdgeMask;
        public float[] triangleCenterBarycenters;
        public float[] nestedDistanceFromNodeCenter;
        public int[] topologyCount;
        public int[] topology;
        public float[] topologyOffset;
    }
}
