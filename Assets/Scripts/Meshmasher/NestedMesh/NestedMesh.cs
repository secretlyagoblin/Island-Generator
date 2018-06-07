using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MeshMasher.MeshTiling;

namespace MeshMasher {

    public class NestedMesh {

        //static int _offsetSize = 50;

        public Vector3[] Verts;
        public int[] Tris;
        public int[] DerivedTri;//1/3 tris
        public SimpleVector2Int[] DerivedOffset;
        public double Scale = 1.0;
        public int NestedLevel = 0;

        private MeshTile _meshTile;

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
            DerivedOffset = derivedOffset.ToArray();
        }

        public NestedMesh(NestedMesh originMesh, int[] meshAccessIndices)
        {
            _meshTile = originMesh._meshTile;

            var verts = new List<Vector3>();
            var tris = new List<int>();
            var derivedTri = new List<int>();
            var derivedOffset = new List<SimpleVector2Int>();

            Debug.Log("Goddamn it chris make this metadata");

            Scale = originMesh.Scale * (1f/ _meshTile.NestingScale);
            NestedLevel = originMesh.NestedLevel + 1;


            var baseDict = new Dictionary<SimpleVector2Int, int[]>();

            var indexMap = new int[_meshTile.Positions.Length];

            for (int i = 0; i < indexMap.Length; i++)
            {
                indexMap[i] = -1;
            }

            //need to go from offset 0 to offset 12.5 to offset 12.5 + 12.5/4

            var localOffset = 0f;
            var localOffsetAdjuster = 50f;

            //var tilingAmount = 5;
            //Debug.Log("Goddamn it chris make this metadata");

            for (int i = 0; i < NestedLevel; i++)
            {
                localOffsetAdjuster = localOffsetAdjuster / _meshTile.NestingScale;
                localOffset += localOffsetAdjuster;
            }

            Debug.Log("Offset Level " + localOffset);

            var vertCount = -1;

            for (int i = 0; i < meshAccessIndices.Length; i++)
            {
                var subVerts = _meshTile.ScaledVerts[originMesh.DerivedTri[meshAccessIndices[i]]];
                var subOffsets = _meshTile.ScaledOffsets[originMesh.DerivedTri[meshAccessIndices[i]]];
                var offset = originMesh.DerivedOffset[meshAccessIndices[i]];

                for (int u = 0; u < subVerts.Length; u++)
                {
                    vertCount++;

                    //var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //p.transform.localScale = Vector3.one * (float)Scale;

                    var pos = _meshTile.Positions[subVerts[u]];
                    pos += new Vector3(subOffsets[u].x * _meshTile.Scale, subOffsets[u].y * _meshTile.Scale, 0); //suboffset
                    pos += new Vector3(offset.x * _meshTile.Scale * _meshTile.NestingScale, offset.y * _meshTile.Scale * _meshTile.NestingScale, 0); //offset
                    pos = pos * (float)Scale;
                    pos -= new Vector3((float)localOffset, (float)localOffset, 0); //shift

                    verts.Add(pos);

                    var key = subOffsets[u] + (offset * _meshTile.NestingScale);

                    if (baseDict.ContainsKey(key))
                    {
                        baseDict[key][subVerts[u]] = vertCount;
                    }
                    else
                    {
                        baseDict.Add(key, (int[])indexMap.Clone());
                        baseDict[key][subVerts[u]] = vertCount;
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
            Tris = tris.ToArray();
            DerivedTri = derivedTri.ToArray();
            DerivedOffset = derivedOffset.ToArray();
        }

        /*
        public NestedMesh(NestedMesh originMesh, int[] meshAccessIndices, int bullsht)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var derivedTri = new List<int>();
            var derivedOffset = new List<SimpleVector2Int>();

            Scale = originMesh.Scale * 0.25;
            NestedLevel = originMesh.NestedLevel + 1;
            _meshTile = originMesh._meshTile;

            var baseDict = new Dictionary<SimpleVector2Int, int[]>();

            var indexMap = new int[_meshTile.Positions.Length];

            for (int i = 0; i < indexMap.Length; i++)
            {
                indexMap[i] = -1;
            }

            var localOffset = 0f;
            var scale = 50f;

            for (int i = 0; i < NestedLevel; i++)
            {
                scale = scale / 4;
                localOffset += scale;
            }

            Debug.Log("Offset Level " + localOffset);

            var vertCount = -1;

            for (int i = 0; i < meshAccessIndices.Length; i++)
            {
                var centers = _meshTile.NestedTriangleIndexes[meshAccessIndices[i]];
                var centerOffsets = _meshTile.NestedTriangleOffsets[meshAccessIndices[i]];

                var subVerts = new List<int>();
                var subOffsets = new List<SimpleVector2Int>();
                var offset = originMesh.DerivedOffset[meshAccessIndices[i]];

                for (int u = 0; u < centers.Length; u++)
                {
                    var index = centers[u] * 3;
                    if (!subVerts.Contains(_meshTile.Triangles[index]))
                    {
                        subVerts.Add(_meshTile.Triangles[index]);
                        subOffsets.Add(centerOffsets[u]+ offset);
                    }

                    index++;

                    if (!subVerts.Contains(_meshTile.Triangles[index]))
                    {
                        subVerts.Add(_meshTile.Triangles[index]);
                        subOffsets.Add(centerOffsets[u] + offset);
                    }

                    index++;

                    if (!subVerts.Contains(_meshTile.Triangles[index]))
                    {
                        subVerts.Add(_meshTile.Triangles[index]);
                        subOffsets.Add(centerOffsets[u] + offset);
                    }
                }

                //subVerts = subVerts.Distinct().ToList();
                //var subOffsets = mesh.ScaledOffsets[originMesh.DerivedTri[meshAccessIndices[i]]];


                //var subVerts = mesh.ScaledTrianglesOwnedByPoints[meshAccessIndices[i]];

                for (int u = 0; u < subVerts.Count; u++)
                {
                    vertCount++;

                    //var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //p.transform.localScale = Vector3.one * (float)Scale;

                    var pos = _meshTile.Positions[subVerts[u]];
                    pos += new Vector3(subOffsets[u].x * _offsetSize, subOffsets[u].y * _offsetSize, 0); //suboffset
                    pos += new Vector3(offset.x * _offsetSize * 4, offset.y * _offsetSize * 4, 0); //offset
                    pos = pos * (float)Scale;
                    pos -= new Vector3((float)localOffset, (float)localOffset, 0); //shift

                    verts.Add(pos);

                    var key = subOffsets[u] + (offset * 4);

                    if (baseDict.ContainsKey(key))
                    {
                        baseDict[key][subVerts[u]] = vertCount;
                    }
                    else
                    {
                        baseDict.Add(key, (int[])indexMap.Clone());
                        baseDict[key][subVerts[u]] = vertCount;
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
            Tris = tris.ToArray();
            DerivedTri = derivedTri.ToArray();
            DerivedOffset = derivedOffset.ToArray();
        }
        */

        public T[] LerpBarycentricValues<T>(T[] originValues,  int[] meshTris) where T: IBlerpable<T>
        {
            //var nestedValues = new List<T>();

            //var innerScale = Scale * 0.25;
            //var nestedLevel = NestedLevel + 1;

            //var localOffset = 0f;
            //var scale = 50f;
            //var tilingAmount = 5;
            //Debug.Log("Goddamn it chris make this metadata");
            //
            //for (int i = 0; i < nestedLevel; i++)
            //{
            //    scale = scale / tilingAmount;
            //    localOffset += scale;
            //}

            var arraySize = 0;

            for (int i = 0; i < meshTris.Length; i++)
            {
                arraySize += _meshTile.Barycenters[DerivedTri[meshTris[i]]].Length;
            }

            var nestedValues = new T[arraySize];
            var nestedValuesIndex = 0;

            for (int i = 0; i < meshTris.Length; i++)
            {
                var barycenters = _meshTile.Barycenters[DerivedTri[meshTris[i]]];
                var offset = DerivedOffset[meshTris[i]];

                for (int u = 0; u < barycenters.Length; u++)
                {
                    var bc = barycenters[u];
                    var index = meshTris[i] * 3;

                    var a = originValues[Tris[index]];
                    var b = originValues[Tris[index+1]];
                    var c = originValues[Tris[index+2]];

                    nestedValues[nestedValuesIndex] = (originValues[0].Blerp(a,b,c,bc));
                    nestedValuesIndex++;              
                }
            }

            return nestedValues;
        } 

        public Mesh CreateMesh()
        {
            var mesh = new Mesh()
            {
                name = "generatedMesh",
                vertices = Verts,
                triangles = Tris,
            };

            return mesh;
        }

        bool SetContainsPoints(int[] tilesX, int[] tilesY, SimpleVector2Int tileA, SimpleVector2Int tileB, SimpleVector2Int tileC, bool allowTwoPoints = false)
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

            if (!allowTwoPoints)
                return false;

            if ((containsA ? 1 : 0) + (containsB ? 1 : 0) + (containsC ? 1 : 0) >= 2)
                return true;

            return false;
        }
    }

    public enum NestedMeshAccessType {
        Triangles,Vertex
    }


}