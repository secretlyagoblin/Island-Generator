﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MeshMasher.MeshTiling;

namespace MeshMasher {

    public class NestedMesh {

        static int _offsetSize = 50;

        public Vector3[] Verts;
        public int[] Tris;
        public int[] DerivedTri;//1/3 tris
        public SimpleVector2Int[] DerivedOffset;
        public double Scale = 1.0;
        public int NestedLevel = 0;

        static MeshTileTemplate mesh;// = new BuildMesh();

        public NestedMesh(Vector2Int[] tileArray)
        {
            if (mesh == null)
            {
                mesh = new MeshTileTemplate();
                mesh.Init();
            }

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

            var indexMap = new int[mesh.Positions.Length];

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
                for (int u = 0; u < mesh.Positions.Length; u++)
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

                    var pos = mesh.Positions[u] + new Vector3(tiles[i].x * _offsetSize, tiles[i].y * _offsetSize);

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
                for (int u = 0; u < mesh.Triangles.Length / 3; u++)
                {
                    var triangle = u * 3;

                    var oa = mesh.Offsets[triangle];
                    var ob = mesh.Offsets[triangle + 1];
                    var oc = mesh.Offsets[triangle + 2];

                    var tileA = oa + tiles[i];
                    var tileB = ob + tiles[i];
                    var tileC = oc + tiles[i];

                    if (!SetContainsPoints(tilesX, tilesY, tileA, tileB, tileC))
                        continue;

                    var a = mesh.Triangles[triangle];
                    var b = mesh.Triangles[triangle + 1];
                    var c = mesh.Triangles[triangle + 2];

                    tris.Add(baseDict[tileC][c]);
                    tris.Add(baseDict[tileB][b]);
                    tris.Add(baseDict[tileA][a]);

                    derivedTri.Add(u);
                    derivedOffset.Add(tiles[i] + mesh.CenterOffsets[u]);

                }
            }

            Verts = verts.ToArray();
            Tris = tris.ToArray();
            DerivedTri = derivedTri.ToArray();
            DerivedOffset = derivedOffset.ToArray();
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

        public NestedMesh(NestedMesh originMesh, int[] meshTris)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var derivedTri = new List<int>();
            var derivedOffset = new List<SimpleVector2Int>();

            Scale = originMesh.Scale * 0.25;
            NestedLevel = originMesh.NestedLevel + 1;

            var baseDict = new Dictionary<SimpleVector2Int, int[]>();

            var indexMap = new int[mesh.Positions.Length];

            for (int i = 0; i < indexMap.Length; i++)
            {
                indexMap[i] = -1;
            }

            //for (int i = 0; i < tiles.Count; i++)
            //{
            //    baseDict.Add(tiles[i], (int[])indexMap.Clone());
            //}

            //need to go from offset 0 to offset 12.5 to offset 12.5 + 12.5/4

            var localOffset = 0f;
            var scale = 50f;

            for (int i = 0; i < NestedLevel; i++)
            {
                scale = scale / 4;
                localOffset += scale;
            }

            Debug.Log("Offset Level " + localOffset);

            var vertCount = -1;

            for (int i = 0; i < meshTris.Length; i++)
            {
                var subVerts = mesh.ScaledVerts[originMesh.DerivedTri[meshTris[i]]];
                var subOffsets = mesh.ScaledOffsets[originMesh.DerivedTri[meshTris[i]]];
                var offset = originMesh.DerivedOffset[meshTris[i]];

                for (int u = 0; u < subVerts.Length; u++)
                {
                    vertCount++;

                    //var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //p.transform.localScale = Vector3.one * (float)Scale;

                    var pos = mesh.Positions[subVerts[u]];

                    pos += new Vector3(subOffsets[u].x * _offsetSize, subOffsets[u].y * _offsetSize, 0); //suboffset

                    pos += new Vector3(offset.x * _offsetSize * 4, offset.y * _offsetSize * 4, 0); //offset

                    //p.name = offset + " "+ subOffsets[u];

                    //start here

                    pos = pos * (float)Scale;

                    pos -= new Vector3((float)localOffset, (float)localOffset, 0); //shift



                    //p.transform.position = pos;


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
                for (int u = 0; u < mesh.Triangles.Length / 3; u++)
                {
                    var triangle = u * 3;

                    var oa = mesh.Offsets[triangle];
                    var ob = mesh.Offsets[triangle + 1];
                    var oc = mesh.Offsets[triangle + 2];

                    var tileA = oa + tiles[i];
                    var tileB = ob + tiles[i];
                    var tileC = oc + tiles[i];

                    if (!SetContainsPoints(tilesX, tilesY, tileA, tileB, tileC))
                        continue;

                    var a = mesh.Triangles[triangle];
                    var b = mesh.Triangles[triangle + 1];
                    var c = mesh.Triangles[triangle + 2];

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

        bool SetContainsPoints(int[] tilesX, int[] tilesY, SimpleVector2Int tileA, SimpleVector2Int tileB, SimpleVector2Int tileC)
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

            return false;
        }
    }
}