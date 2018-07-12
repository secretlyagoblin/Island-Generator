﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MeshMasher.MeshTiling;

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
        private MeshTile _meshTile;
        private double _parentScale;

        public NestedMesh(Vector2Int[] tileArray, string meshTileJSON)
        {
            _meshTile = new MeshTile(meshTileJSON);

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

                    //var instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //instance.transform.position = mesh.Positions[u] + new Vector3(chunksToSpawn[i].x * intOffset, chunksToSpawn[i].y * intOffset);
                    //instance.transform.localScale = Vector3.one*0.65f;
                    //values[u] = new Color(Random.Range(0.1f, 1f), Random.Range(0.2f, 1f), Random.Range(0.3f, 1f));
                    //valuesHeight[u] = Random.Range(0.1f, 15f);

                    //var mat = new Material(instance.GetComponent<MeshRenderer>().material);
                    //
                    //mat.color = values[i];
                    //instance.GetComponent<MeshRenderer>().material = mat;

                    var pos = _meshTile.Positions[u] + new Vector3(tiles[i].x * _meshTile.Scale, tiles[i].y * _meshTile.Scale);

                    //var instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //instance.transform.position = pos;

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

            Verts = verts.ToArray();
            _barycenters = bary.ToArray();
            _barycenterParentMap = baryMap.ToArray();
            Tris = tris.ToArray();
            DerivedTri = derivedTri.ToArray();
            TileOffsets = derivedOffset.ToArray();
        }

        public void PopulateMeshByTriangleCenterContainment(NestedMesh originMesh, int[] meshAccessIndices)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var derivedTri = new List<int>();
            var derivedOffset = new List<SimpleVector2Int>();
            var parentNestedLevel = CalcuateOffset(NestedLevel-1);
            var currentNestedOffset = CalcuateOffset(NestedLevel);

            var barys = new List<Barycenter>();
            var triangleParents = new List<int>();

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

            var vertsForCulling = new List<KeyValuePair<int, SimpleVector2Int>>();

            for (int i = 0; i < meshAccessIndices.Length; i++)
            {
                var subTriangleIndexes = _meshTile.NestedTriangleIndexes[originMesh.DerivedTri[meshAccessIndices[i]]];
                var subTriangleTiles = _meshTile.TriangleSubTileOffsets[originMesh.DerivedTri[meshAccessIndices[i]]];
                var innerBarycenters = _meshTile.TriangleBarycentricContainment[originMesh.DerivedTri[meshAccessIndices[i]]];
                var tile = originMesh.TileOffsets[meshAccessIndices[i]];
                

                for (int u = 0; u < subTriangleIndexes.Length; u++)
                {
                    var subTriangleIndex = subTriangleIndexes[u];
                    var subTriangleTile = subTriangleTiles[u];
                    var triangleVertexIndex = subTriangleIndex * 3;

                    for (int v = 2; v >= 0; v--)
                    {
                        var vertexIndex = _meshTile.Triangles[triangleVertexIndex + v];
                        var localOffset = _meshTile.Offsets[triangleVertexIndex + v] + subTriangleTile;

                        var pos = CalculateVertexOffset(vertexIndex, tile, localOffset, currentNestedOffset);

                        verts.Add(pos);
                        vertCount++;
                        tris.Add(vertCount);

                        vertsForCulling.Add(new KeyValuePair<int, SimpleVector2Int>(vertexIndex, localOffset));

                        var innerBarycenter = innerBarycenters[u*3 + v];
                        barys.Add(innerBarycenter);
                        triangleParents.Add(meshAccessIndices[i]);
                    }

                    derivedTri.Add(subTriangleIndexes[u]);
                    derivedOffset.Add(subTriangleTiles[u]);
                }
            }

            var distinctValues = new List<KeyValuePair<int, SimpleVector2Int>>();
            var distinctVerts = new List<Vector3>();
            var distinctBarycenters = new List<Barycenter>();
            var distinctTriangleParentMap = new List<int>();

            var indexMap = new int[vertsForCulling.Count];   
            
            for (int i = 0; i < vertsForCulling.Count; i++)
            {
                var test = vertsForCulling[i];
                var testBary = barys[i];            
                var index = distinctValues.Count;
                var resolved = false;

                for (int u = 0; u < distinctValues.Count; u++)
                {
                    var distinct = distinctValues[u];
                    var distinctBary = distinctBarycenters[u];
                    var distinctTriangleParent = distinctTriangleParentMap[u];

                    if (test.Key == distinct.Key && test.Value == distinct.Value)
                    {
                        if (testBary.contained == true)
                        {
                            distinctBarycenters[u] = testBary;
                            distinctTriangleParentMap[u] = triangleParents[i];
                        }

                        if (resolved == false)
                        {
                            index = u;
                            resolved = true;
                        }
                    }
                }

                if (resolved == false)
                {
                    distinctValues.Add(test);
                    distinctVerts.Add(verts[i]);
                    distinctBarycenters.Add(barys[i]);
                    distinctTriangleParentMap.Add(triangleParents[i]);
                }
                       
                indexMap[i] = index;                
            }
            
            for (int i = 0; i < tris.Count; i++)
            {
                tris[i] = indexMap[i];
            }
            
            Verts = distinctVerts.ToArray();
            Tris = tris.ToArray();
            _barycenterParentMap = distinctTriangleParentMap.ToArray();
            _barycenters = distinctBarycenters.ToArray();
            DerivedTri = derivedTri.ToArray();
            TileOffsets = derivedOffset.ToArray();
        }

        //Blerp based on parents

        public T[] BlerpParentValues<T>(T[] originValues, NestedMesh parentMesh) where T: IBlerpable<T>
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
    }

    public enum NestedMeshAccessType {
        Triangles,Vertex
    }

}