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

    public int[][] MeshTopology;
    public SimpleVector2Int[][] MeshTopologyOffset;
    public int[][] MeshTriangleTopology;
    public SimpleVector2Int[][] MeshTriangleTopologyOffset;

    public int[][] BarycentricParentIndices;
    public SimpleVector2Int[][] BarycentricParentOffsets;

    public int[][] RingBarycentricParentIdices;
    public SimpleVector2Int[][] RingBarycentricParentOffsets;
    public Barycenter[][] RingBarycenters;
    public int[][] RingIndices;
    public SimpleVector2Int[][] RingOffsets;
    public Vector3[][] RingPositions;


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


        Barycenters = new Barycenter[importObject.innerPointIndexList.Length][];
        BarycentricParentIndices = new int[importObject.innerPointIndexList.Length][];
        BarycentricParentOffsets = new SimpleVector2Int[importObject.innerPointIndexList.Length][];
        //ScaledTrianglesOwnedByPoints= public int[][]            

        var count = 0;

        for (int u = 0; u < importObject.innerPointIndexList.Length; u++)
        {
            var innerCount = importObject.innerPointIndexList[u];

            //ScaledVerts[u] = new int[innerCount];
            //SubTileOffsets[u] = new SimpleVector2Int[innerCount];

            Barycenters[u] = new Barycenter[innerCount];
            BarycentricParentIndices[u] = new int[innerCount];
            BarycentricParentOffsets[u] = new SimpleVector2Int[innerCount];

            for (int v = 0; v < innerCount; v++)
            {
                //ScaledVerts[u][v] = importObject.nestedPointIndexes[count];
                //SubTileOffsets[u][v] = new SimpleVector2Int(importObject.nested2dPointOffsets[count * 2], importObject.nested2dPointOffsets[(count * 2) + 1]);

                Barycenters[u][v] = new Barycenter(importObject.barycentricCoordinates[count * 3], importObject.barycentricCoordinates[(count * 3) + 1], importObject.barycentricCoordinates[(count * 3) + 2], true, false);
                BarycentricParentIndices[u][v] = importObject.barycentricParentIndices[count];
                BarycentricParentOffsets[u][v] = new SimpleVector2Int(importObject.barycentricParentOffsets[count*2], importObject.barycentricParentOffsets[(count * 2)+1]);
                count++;
            }
        }

        count = 0;

        ScaledVerts = new int[importObject.innerPointIndexList.Length][];
        SubTileOffsets = new SimpleVector2Int[importObject.innerPointIndexList.Length][];

        for (int u = 0; u < importObject.innerCellPointIndexList.Length; u++)
        {
            var innerCount = importObject.innerCellPointIndexList[u];

            ScaledVerts[u] = new int[innerCount];
            SubTileOffsets[u] = new SimpleVector2Int[innerCount];

            for (int v = 0; v < innerCount; v++)
            {
                var count2 = count * 2;
                ScaledVerts[u][v] = importObject.nestedPointIndexes[count];
                SubTileOffsets[u][v] = new SimpleVector2Int(importObject.nested2dPointOffsets[count2], importObject.nested2dPointOffsets[(count2) + 1]);

                count++;
            }
        }

        count = 0;

        MeshTopology = new int[importObject.topologyCount.Length][];
        MeshTopologyOffset = new SimpleVector2Int[importObject.topologyCount.Length][];
        MeshTriangleTopology = new int[importObject.topologyCount.Length][];
        MeshTriangleTopologyOffset = new SimpleVector2Int[importObject.topologyCount.Length][];

        for (int u = 0; u < importObject.topologyCount.Length; u++)
        {
            var innerCount = importObject.topologyCount[u];

            MeshTopology[u] = new int[innerCount];
            MeshTopologyOffset[u] = new SimpleVector2Int[innerCount];
            MeshTriangleTopology[u] = new int[innerCount];
            MeshTriangleTopologyOffset[u] = new SimpleVector2Int[innerCount];

            for (int v = 0; v < innerCount; v++)
            {
                var count2 = count * 2;
                MeshTopology[u][v] = importObject.topology[count];
                MeshTopologyOffset[u][v] = new SimpleVector2Int(importObject.topologyOffset[count2], importObject.topologyOffset[(count2) + 1]);
                MeshTriangleTopology[u][v] = importObject.triTopologyData[count];
                MeshTriangleTopologyOffset[u][v] = new SimpleVector2Int(importObject.triOffsetData[count2], importObject.triOffsetData[(count2) + 1]);
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

        count = 0;

        RingIndices = new int[Positions.Length][];
        RingOffsets = new SimpleVector2Int[Positions.Length][];
        RingBarycenters = new Barycenter[Positions.Length][];
        RingBarycentricParentIdices = new int[Positions.Length][];
        RingBarycentricParentOffsets = new SimpleVector2Int[Positions.Length][];
        RingPositions = new Vector3[Positions.Length][];

        for (int u = 0; u < RingIndices.Length; u++)
        {
            var innerCount = importObject.ringIndexCount[u];

            RingIndices[u] = new int[innerCount];
            RingOffsets[u] = new SimpleVector2Int[innerCount];
            RingBarycenters[u] = new Barycenter[innerCount];
            RingBarycentricParentIdices[u] = new int[innerCount];
            RingBarycentricParentOffsets[u] = new SimpleVector2Int[innerCount];
            RingPositions[u] = new Vector3[innerCount];

            for (int v = 0; v < innerCount; v++)
            {
                RingIndices[u][v] = importObject.ringIndices[count];
                RingOffsets[u][v] = new SimpleVector2Int(
                        importObject.ringOffsets[count * 2],
                        importObject.ringOffsets[count * 2 + 1]
                    );
                RingBarycentricParentIdices[u][v] = importObject.ringBarycentricParentIdices[count];
                RingBarycentricParentOffsets[u][v] = new SimpleVector2Int(
                        importObject.ringBarycentricParentOffsets[count * 2],
                        importObject.ringBarycentricParentOffsets[count * 2 + 1]
                    );
               RingBarycenters[u][v] = new Barycenter(
                            importObject.ringBarycenters[count * 3],
                            importObject.ringBarycenters[count * 3 + 1],
                            importObject.ringBarycenters[count * 3 + 2],
                            true,
                            true
                        );
                RingPositions[u][v] = new Vector3(
                    importObject.ringPositions[count * 2],
                     importObject.ringPositions[count * 2 + 1],
                     0
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
        public int[] topologyOffset;
        public int[] innerCellPointIndexList;
        public int[] barycentricParentIndices;
        public int[] barycentricParentOffsets;
        public int[] triTopologyData;
        public int[] triOffsetData;
        public int[] ringIndexCount;
        public int[] ringBarycentricParentIdices;
        public int[] ringBarycentricParentOffsets;
        public float[] ringBarycenters;
        public int[] ringIndices;
        public int[] ringOffsets;
        public float[] ringPositions;
    }
}
