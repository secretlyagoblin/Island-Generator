using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Topology;

namespace MeshMasher {

    public static class DelaunayGen {

        public static Mesh GetMeshFromMap(Maps.Map heightMap, float triangleRatio)
        {
            RNG.Init();
            var sampler = new PoissonDiscSampler(1f, 1f, triangleRatio);
            var polygon = new Polygon();

            foreach (var sample in sampler.Samples())
            {
                polygon.Add(new Vertex(sample.x, sample.y));
            }

            polygon.Add(new Vertex(0f, 0f));
            polygon.Add(new Vertex(0, 1f));
            polygon.Add(new Vertex(1f, 0f));
            polygon.Add(new Vertex(1f, 1f));

            var options = new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = false };
            var del = (TriangleNet.Mesh)polygon.Triangulate(options);

            var meshes = MakeMeshes(del, heightMap);

            var min = triangleRatio * 0.001f;
            var rat = triangleRatio * 2f;

            var mesh = AutoWelder.AutoWeld(meshes[0], min, rat);

            return mesh;
        }

        static List<Mesh> MakeMeshes(TriangleNet.Mesh mesh, Maps.Map heightMap)
        {
            var trianglesInChunk = 20000;
            IEnumerator<Triangle> triangleEnumerator = mesh.Triangles.GetEnumerator();

            var output = new List<Mesh>();

            for (int chunkStart = 0; chunkStart < mesh.Triangles.Count; chunkStart += trianglesInChunk)
            {
                var vertices = new List<Vector3>();
                var normals = new List<Vector3>();
                var uvs = new List<Vector2>();
                var triangles = new List<int>();

                var chunkEnd = chunkStart + trianglesInChunk;
                for (int i = chunkStart; i < chunkEnd; i++)
                {
                    if (!triangleEnumerator.MoveNext())
                    {
                        break;
                    }

                    var triangle = triangleEnumerator.Current;


                    // For the triangles to be right-side up, they need
                    // to be wound in the opposite direction
                    var v0 = GetPoint3D(mesh, triangle.vertices[2].id);
                    var v1 = GetPoint3D(mesh, triangle.vertices[1].id);
                    var v2 = GetPoint3D(mesh, triangle.vertices[0].id);

                    v0.y = heightMap.BilinearSampleFromNormalisedVector2(new Vector2(v0.x, v0.z));
                    v1.y = heightMap.BilinearSampleFromNormalisedVector2(new Vector2(v1.x, v1.z));
                    v2.y = heightMap.BilinearSampleFromNormalisedVector2(new Vector2(v2.x, v2.z));

                    triangles.Add(vertices.Count);
                    triangles.Add(vertices.Count + 1);
                    triangles.Add(vertices.Count + 2);

                    vertices.Add(v0);
                    vertices.Add(v1);
                    vertices.Add(v2);

                    var normal = Vector3.Cross(v1 - v0, v2 - v0);
                    normals.Add(normal);
                    normals.Add(normal);
                    normals.Add(normal);

                    uvs.Add(new Vector2(0.0f, 0.0f));
                    uvs.Add(new Vector2(0.0f, 0.0f));
                    uvs.Add(new Vector2(0.0f, 0.0f));
                }

                var chunkMesh = new Mesh();
                chunkMesh.vertices = vertices.ToArray();
                chunkMesh.uv = uvs.ToArray();
                chunkMesh.triangles = triangles.ToArray();
                chunkMesh.normals = normals.ToArray();

                output.Add(chunkMesh);


            }

            return output;
        }

        static Vector3 GetPoint3D(TriangleNet.Mesh mesh, int index)
        {
            Vertex vertex = mesh.vertices[index];
            return new Vector3((float)vertex.x, 0f, (float)vertex.y);
        }


    }
}
