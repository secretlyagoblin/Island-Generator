using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace MeshMasher {

    public class NestedMesh {

        public Vector3[] Verts;
        public int[] Tris;
        public int[] DerivedTri;//1/3 tris
        public SimpleVector2Int[] TileOffsets;
        public double Scale = 1.0;
        public int NestedLevel = 0;

        private int[] _barycenterParentMap;

        private Barycenter[] _barycenters;
        private Barycenter[] _centerBarycenters;

        private MeshTile _meshTile;
        private double _parentScale;

        public NestedMesh(Vector2Int[] tileArray, MeshTile meshTile)
        {
            _meshTile = meshTile;
            var tiles = new SimpleVector2Int[tileArray.Length];

            for (int i = 0; i < tileArray.Length; i++)
            {
                tiles[i] = new SimpleVector2Int(tileArray[i].x, tileArray[i].y);
            }

            //mesh.Init(); // shouldn't need to do that

            var verts = new List<Vector3>();
            var tris = new List<int>();
            var derivedTri = new List<int>();
            var derivedOffset = new List<SimpleVector2Int>();

            var baseDict = new Dictionary<SimpleVector2Int, int[]>();

            var indexMap = new int[_meshTile.Positions.Length];

            for (int i = 0; i < indexMap.Length; i++)
            {
                indexMap[i] = -1;
            }

            for (int i = 0; i < tiles.Length; i++)
            {
                baseDict.Add(tiles[i], (int[])indexMap.Clone());
            }

            var vertCount = -1;

            for (int i = 0; i < tiles.Length; i++)
            {
                for (int u = 0; u < _meshTile.Positions.Length; u++)
                {
                    vertCount++;

                    var pos = _meshTile.Positions[u] + new Vector3(tiles[i].x * _meshTile.Scale, tiles[i].y * _meshTile.Scale);
                    verts.Add(pos);

                    baseDict[tiles[i]][u] = vertCount;
                }
            }

            var tilesX = new int[tiles.Length];
            var tilesY = new int[tiles.Length];

            for (int i = 0; i < tiles.Length; i++)
            {
                tilesX[i] = tiles[i].x;
                tilesY[i] = tiles[i].y;
            }

            for (int i = 0; i < tiles.Length; i++)
            {
                for (int u = 0; u < _meshTile.Triangles.Length / 3; u++)
                {
                    var triangle = u * 3;

                    var oa = _meshTile.Offsets[triangle];
                    var ob = _meshTile.Offsets[triangle + 1];
                    var oc = _meshTile.Offsets[triangle + 2];

                    var tileA = oa + tiles[i];
                    var tileB = ob + tiles[i];
                    var tileC = oc + tiles[i];

                    if (!SetContainsPoints(tilesX, tilesY, tileA, tileB, tileC))
                        continue;

                    var a = _meshTile.Triangles[triangle];
                    var b = _meshTile.Triangles[triangle + 1];
                    var c = _meshTile.Triangles[triangle + 2];

                    tris.Add(baseDict[tileC][c]);
                    tris.Add(baseDict[tileB][b]);
                    tris.Add(baseDict[tileA][a]);

                    derivedTri.Add(u);
                    derivedOffset.Add(tiles[i] + _meshTile.CenterOffsets[u]);

                }
            }

            Verts = verts.ToArray();
            Tris = tris.ToArray();
            DerivedTri = derivedTri.ToArray();
            TileOffsets = derivedOffset.ToArray();
        }

        public NestedMesh(NestedMesh originMesh, int[] meshAccessIndices, NestedMeshAccessType type = NestedMeshAccessType.Vertex)
        {
            _meshTile = originMesh._meshTile;
            _parentScale = originMesh.Scale;
            Scale = originMesh.Scale * (1f / _meshTile.NestingScale);
            NestedLevel = originMesh.NestedLevel + 1;

            switch (type)
            {
                case NestedMeshAccessType.Vertex:
                    PopulateMeshByVertexContainment(originMesh, meshAccessIndices);
                    break;
                case NestedMeshAccessType.Triangles:
                    PopulateMeshByTriangleCenterContainment(originMesh, meshAccessIndices);
                    break;
            }

        }

        // Populate Mesh

        private void PopulateMeshByVertexContainment(NestedMesh originMesh, int[] meshAccessIndices)
        {
            var defaultSize = 200;

            var verts = new List<Vector3>(defaultSize);
            var tris = new List<int>(defaultSize*3);
            var bary = new List<Barycenter>(defaultSize);
            var baryMap = new List<int>(defaultSize);
            var derivedTri = new List<int>(defaultSize);
            var derivedOffset = new List<SimpleVector2Int>(defaultSize);
            var baseDict = new Dictionary<SimpleVector2Int, int[]>(defaultSize);
            var indexMap = new int[_meshTile.Positions.Length];
            var currentNestedOffset = CalcuateOffset(NestedLevel);

            //var triangleBarycenters = new List<Barycenter>();
            //var triangleBarycenterParentMap = new List<int>();

            //Debug.Log("Current Nested Level " + NestedLevel);
            //Debug.Log("Creating Vertex Access Mesh at Offset Level " + currentNestedOffset);

            for (int i = 0; i < indexMap.Length; i++)
            {
                indexMap[i] = -1;
            }
            
            var vertCount = -1;

            for (int i = 0; i < meshAccessIndices.Length; i++)
            {
                var subVerts = _meshTile.ScaledVerts[originMesh.DerivedTri[meshAccessIndices[i]]];
                var subTiles = _meshTile.SubTileOffsets[originMesh.DerivedTri[meshAccessIndices[i]]];
                var tile = originMesh.TileOffsets[meshAccessIndices[i]];

                var subBarycenters = _meshTile.Barycenters[originMesh.DerivedTri[meshAccessIndices[i]]];

                for (int u = 0; u < subVerts.Length; u++)
                {
                    vertCount++;

                    var subVert = subVerts[u];
                    var subTile = subTiles[u];
                    var subBarycenter = subBarycenters[u];

                    var pos = CalculateVertexOffset(subVert, tile, subTile, currentNestedOffset);

                    verts.Add(pos);
                    bary.Add(subBarycenter);
                    baryMap.Add(meshAccessIndices[i]);
                    //baryMap.Add(originMesh.)


                    var key = subTile + (tile * _meshTile.NestingScale);

                    if (baseDict.ContainsKey(key))
                    {
                        baseDict[key][subVert] = vertCount;
                    }
                    else
                    {
                        baseDict.Add(key, (int[])indexMap.Clone());
                        baseDict[key][subVert] = vertCount;
                    }
                }
            }

            var tiles = baseDict.Keys.ToArray();
            var tilesX = new int[tiles.Length];
            var tilesY = new int[tiles.Length];

            for (int i = 0; i < tiles.Length; i++)
            {
                tilesX[i] = tiles[i].x;
                tilesY[i] = tiles[i].y;
            }

            for (int i = 0; i < tiles.Length; i++)
            {
                //TODO hey in theory I should just be able to test this against the relevant triangles instead of the whole set

                //for (int u = 0; u < meshAccessIndices.Length; u++)
                //{
                //    var triangle = originMesh.DerivedTri[meshAccessIndices[i]];
                //    triangle = u * 3;

                for (int u = 0; u < _meshTile.Triangles.Length / 3; u++) 
                {
                    var triangle = u * 3;

                    var oa = _meshTile.Offsets[triangle];
                    var ob = _meshTile.Offsets[triangle + 1];
                    var oc = _meshTile.Offsets[triangle + 2];

                    var tileA = oa + tiles[i];
                    var tileB = ob + tiles[i];
                    var tileC = oc + tiles[i];

                    if (!SetContainsPoints(tilesX, tilesY, tileA, tileB, tileC))
                        continue;

                    var a = _meshTile.Triangles[triangle];
                    var b = _meshTile.Triangles[triangle + 1];
                    var c = _meshTile.Triangles[triangle + 2];

                    var trueA = baseDict[tileA][a];
                    var trueB = baseDict[tileB][b];
                    var trueC = baseDict[tileC][c];

                    if (trueA == -1 | trueB == -1 | trueC == -1)
                        continue;

                    tris.Add(trueC);
                    tris.Add(trueB);
                    tris.Add(trueA);

                    derivedTri.Add(u);
                    derivedOffset.Add(tiles[i]);
                }
            }

            FinaliseData(
                verts.ToArray(),
                tris.ToArray(),
                derivedTri.ToArray(),
                derivedOffset.ToArray(),
                //triangleBarycenterParentMap.ToArray(),
                //triangleBarycenters.ToArray(),
                baryMap.ToArray(),
                bary.ToArray(),
                new Barycenter[] {}
            );
        }

        public void PopulateMeshByTriangleCenterContainment(NestedMesh originMesh, int[] meshAccessIndices)
        {
            var defaultSize = meshAccessIndices.Length * 35;

            var verts = new List<Vector3>(defaultSize);
            var tris = new List<int>(defaultSize * 3);
            var derivedTri = new List<int>(defaultSize);
            var derivedOffset = new List<SimpleVector2Int>(defaultSize);
            var parentNestedLevel = CalcuateOffset(NestedLevel - 1);
            var currentNestedOffset = CalcuateOffset(NestedLevel);

            var vertexBarycenters = new List<Barycenter>(defaultSize);
            var triangeBarycenters = new List<Barycenter>(defaultSize);
            var vertexTriangleParents = new List<int>(defaultSize);

            //Debug.Log("Creating Triangle Access Mesh at Offset Level " + currentNestedOffset);

            var vertCount = -1;

            //Step1:
            //get all triangle indices
            //step2:
            //get all unique indices (applying offsets)
            //step3:
            //remap triangles to match new indices
            //step4:
            //map to derived tris

            var vertsForCulling = new List<DistinctIndex>(defaultSize);

            //UnityEngine.Profiling.Profiler.BeginSample("Initial Mesh Sampling");

            for (int i = 0; i < meshAccessIndices.Length; i++)
            {
                var subTriangleIndexes = _meshTile.NestedTriangleIndexes[originMesh.DerivedTri[meshAccessIndices[i]]];
                var subTriangleTiles = _meshTile.TriangleSubTileOffsets[originMesh.DerivedTri[meshAccessIndices[i]]];
                var innerBarycenters = _meshTile.TriangleBarycentricContainment[originMesh.DerivedTri[meshAccessIndices[i]]];
                var tile = originMesh.TileOffsets[meshAccessIndices[i]];
                var innerTriangleBarycenters = _meshTile.TriangleCenterBarycenters[originMesh.DerivedTri[meshAccessIndices[i]]];


                for (int u = 0; u < subTriangleIndexes.Length; u++)
                {
                    var subTriangleIndex = subTriangleIndexes[u];
                    var subTriangleTile = subTriangleTiles[u];
                    var triangleVertexIndex = subTriangleIndex * 3;
                    var innerTriangleBarycenter = innerTriangleBarycenters[u];

                    for (int v = 2; v >= 0; v--)
                    {
                        var vertexIndex = _meshTile.Triangles[triangleVertexIndex + v];
                        var localOffset = _meshTile.Offsets[triangleVertexIndex + v] + subTriangleTile;

                        var pos = CalculateVertexOffset(vertexIndex, tile, localOffset, currentNestedOffset);

                        verts.Add(pos);
                        vertCount++;
                        tris.Add(vertCount);

                        vertsForCulling.Add(new DistinctIndex(vertexIndex, localOffset.x, localOffset.y));

                        var innerBarycenter = innerBarycenters[u * 3 + v];
                        vertexBarycenters.Add(innerBarycenter);
                        vertexTriangleParents.Add(meshAccessIndices[i]);

                    }

                    triangeBarycenters.Add(innerTriangleBarycenter);
                    derivedTri.Add(subTriangleIndexes[u]);
                    derivedOffset.Add(subTriangleTiles[u]);
                }
            }

            //UnityEngine.Profiling.Profiler.EndSample();

            //UnityEngine.Profiling.Profiler.BeginSample("Final Resolution");

            var distinctValues = new List<DistinctIndex>(defaultSize);


            var distinctDict = new Dictionary<DistinctIndex, int>();

            var vertCullLength = vertsForCulling.Count;
            var distinctValueCount = 0;

            var indexMap = new int[vertCullLength];

            var distinctIndices = new List<int>(defaultSize);

            for (int i = 0; i < vertCullLength; i++)
            {
                var test = vertsForCulling[i];
                var testBary = vertexBarycenters[i];
                var index = distinctValueCount;

                //speed info here https://stackoverflow.com/questions/2728500/hashsett-versus-dictionaryk-v-w-r-t-searching-time-to-find-if-an-item-exist#10348367
                if (distinctDict.ContainsKey(test))
                {
                    index = distinctDict[test];

                    if (testBary.contained == true)
                    {
                        distinctIndices[distinctDict[test]] = i;
                        //distinctDict[test] = index;
                    }
                }
                else
                {
                    distinctDict.Add(test, index);
                    distinctIndices.Add(i);
                    distinctValueCount++;
                }

                indexMap[i] = index;
            }

            var distinctVerts = new Vector3[distinctIndices.Count];
            var distinctBarycenters = new Barycenter[distinctIndices.Count];
            var distinctBarycenterParentMap = new int[distinctIndices.Count];

            for (int i = 0; i < distinctIndices.Count; i++)
            {
                distinctVerts[i] = verts[distinctIndices[i]];
                distinctBarycenters[i] = vertexBarycenters[distinctIndices[i]];
                distinctBarycenterParentMap[i] = vertexTriangleParents[distinctIndices[i]];
            }

            for (int i = 0; i < tris.Count; i++)
            {
                tris[i] = indexMap[i];
            }

            //UnityEngine.Profiling.Profiler.EndSample();


            FinaliseData(
                distinctVerts,
                tris.ToArray(),
                derivedTri.ToArray(),
                derivedOffset.ToArray(),
                distinctBarycenterParentMap,
                distinctBarycenters,
                triangeBarycenters.ToArray()
                );
        }

        //Blerp based on parents

        public T[] BlerpParentNodeValues<T>(T[] originValues, NestedMesh parentMesh) where T: IBlerpable<T>
        {

            var nestedValues = new T[Verts.Length];

            for (int i = 0; i < Verts.Length; i++)
            {
                var barycenter = _barycenters[i];
                var parentTriangle = _barycenterParentMap[i];
                var index = parentTriangle * 3;

                var a = originValues[parentMesh.Tris[index]];
                var b = originValues[parentMesh.Tris[index+1]];
                var c = originValues[parentMesh.Tris[index+2]];

                nestedValues[i] = (originValues[0].Blerp(a, b, c, barycenter));
            }
            
            return nestedValues;
        }

        public T[] BlerpNodeToCellValuesUsingDerivedCenter<T>(T[] nodeValues) where T : IBlerpable<T>
        {
            var len = Tris.Length / 3;
            var nestedValues = new T[len];

            for (int i = 0; i < len; i++)
            {
                var index = i * 3;
                var a = nodeValues[Tris[index]];
                var b = nodeValues[Tris[index + 1]];
                var c = nodeValues[Tris[index + 2]];

                nestedValues[i] = (nodeValues[0].Blerp(a, b, c, Barycenter.center));
            }
            return nestedValues;
        }

        public T[] BlerpNodeToCellValuesUsingParentCenter<T>(T[] originValues, NestedMesh parentMesh) where T : IBlerpable<T>
        {
            var len = Tris.Length / 3;
            var nestedValues = new T[len];

            for (int i = 0; i < len; i++)
            {
                var barycenter = _centerBarycenters[i];
                var index = i * 3;

                var a = originValues[parentMesh.Tris[index]];
                var b = originValues[parentMesh.Tris[index + 1]];
                var c = originValues[parentMesh.Tris[index + 2]];

                nestedValues[i] = (originValues[0].Blerp(a, b, c, barycenter));
            }
            return nestedValues;
        }

        //Create Mesh

        public Mesh CreateMesh()
        {
            var mesh = new Mesh()
            {
                name = "generatedMesh",
                vertices = Verts,
                triangles = Tris,
                //colors = _barycenters.Select(x => new Color(x.u, x.v, x.w)).ToArray()
            };

            return mesh;
        }

        //Create Debug Mesh

        public Mesh CreateBaryDebugMesh()
        {
            var mesh = new Mesh()
            {
                name = "generatedMesh",
                vertices = Verts,
                triangles = Tris,
                colors = _barycenters.Select(x => new Color(x.u, x.v, x.w)).ToArray()
            };
            return mesh;
        }

        //Helper Functions

        private bool SetContainsPoints(int[] tilesX, int[] tilesY, SimpleVector2Int tileA, SimpleVector2Int tileB, SimpleVector2Int tileC)
        {
            var containsA = false;
            var containsB = false;
            var containsC = false;

            var ax = tileA.x;
            var ay = tileA.y;

            var bx = tileB.x;
            var by = tileB.y;

            var cx = tileC.x;
            var cy = tileC.y;

            for (int e = 0; e < tilesX.Length; e++)
            {
                var x = tilesX[e];
                var y = tilesY[e];

                if (x == ax && y == ay)
                {
                    containsA = true;
                }

                if (x == bx && y == by)
                {
                    containsB = true;
                }

                if (x == cx && y == cy)
                {
                    containsC = true;
                }

                if (containsA && containsB && containsC)
                {
                    return true;
                }

            }

            //if (!allowTwoPoints)
            //    return false;
            //
            //if ((containsA ? 1 : 0) + (containsB ? 1 : 0) + (containsC ? 1 : 0) >= 2)
            //    return true;

            return false;
        }

        private float CalcuateOffset(int nestingLevel)
        {
            //need to go from offset 0 to offset 12.5 to offset 12.5 + 12.5/4
            var localOffset = 0f;
            float localOffsetAdjuster = _meshTile.Scale;

            for (int i = 0; i < nestingLevel; i++)
            {
                localOffsetAdjuster = localOffsetAdjuster / _meshTile.NestingScale;
                localOffset += localOffsetAdjuster;
            }

            return localOffset;
        }

        private Vector3 CalculateVertexOffset(int vertexIndex, SimpleVector2Int tile, SimpleVector2Int subTile, float offsetMagnitude)
        {
            var pos = _meshTile.Positions[vertexIndex];
            pos += new Vector3(subTile.x * _meshTile.Scale, subTile.y * _meshTile.Scale, 0); //suboffset
            pos += new Vector3(tile.x * _meshTile.Scale * _meshTile.NestingScale, tile.y * _meshTile.Scale * _meshTile.NestingScale, 0); //offset
            pos = pos * (float)Scale;
            pos -= new Vector3(offsetMagnitude, offsetMagnitude, 0); //shift

            return pos;
        }

        private void FinaliseData(
            Vector3[] verts,
            int[] tris,
            int[] derivedTri,
            SimpleVector2Int[] tileOffsets,
            int[] barycenterParentMap,
            Barycenter[] barycenters,
            Barycenter[] centerBarycenters)
        {
            Verts = verts;
            Tris = tris;
            DerivedTri = derivedTri;
            TileOffsets = tileOffsets;


            _barycenterParentMap = barycenterParentMap;
            _barycenters = barycenters;
            _centerBarycenters = centerBarycenters;

        }

        private struct DistinctIndex : IEquatable<DistinctIndex>
        {
            public int i;
            public int x;
            public int y;

            public DistinctIndex(int i, int x, int y)
            {
                this.i = i;
                this.x = x;
                this.y = y;
            }

            bool IEquatable<DistinctIndex>.Equals(DistinctIndex other)
            {
                return other.x == x && other.y == y && other.i == i;
            }

            public bool Equals(DistinctIndex other)
            {
                return x == other.x && y == other.y && other.i == i;
            }

            public override int GetHashCode()
            {
                return x.GetHashCode() * y.GetHashCode() * i.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }
        }

        private struct BarycenterWithParent
        {
            public Barycenter b;
            public int i; //index
            public int p; //parent

        }
    }

    public enum NestedMeshAccessType {
        Triangles,Vertex
    }

}