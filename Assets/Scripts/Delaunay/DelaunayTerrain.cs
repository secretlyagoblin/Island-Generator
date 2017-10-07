using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Topology;

public class DelaunayTerrain : MonoBehaviour {
    // Maximum size of the terrain.
    public int xsize = 1;
    public int ysize = 1;

    // Number of random points to generate.
    public int randomPoints = 50;

    // Triangles in each chunk.
    public int trianglesInChunk = 20000;

    // The delaunay mesh
    private TriangleNet.Mesh _mesh = null;

    public MeshFilter Filter;

    void Start()
    {
        Generate();
    }

    public virtual void Generate() {
        RNG.DateTimeInit();

        
        Polygon polygon = new Polygon();
        for (int i = 0; i < randomPoints; i++) {
            polygon.Add(new Vertex(Random.Range(0.0f, xsize), Random.Range(0.0f, ysize)));
        }
        polygon.Add(new Vertex(0,0));
        polygon.Add(new Vertex(0, ysize));
        polygon.Add(new Vertex(xsize, 0));
        polygon.Add(new Vertex(xsize, ysize));


        TriangleNet.Meshing.ConstraintOptions options = new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true };
        _mesh = (TriangleNet.Mesh)polygon.Triangulate(options);

        var meshes = MakeMeshes();

        for (int i = 0; i < meshes.Count; i++)
        {
            var mesh = MeshMasher.AutoWelder.AutoWeld(meshes[i], 0.000001f, 0.01f);

            var sMesh = new MeshMasher.SmartMesh(mesh);
            var minTree = sMesh.MinimumSpanningTree();
            minTree = sMesh.CullLeavingOnlySimpleLines(minTree);
            //sMesh.DrawMesh(transform);

            mesh = sMesh.BuildMeshSurfaceWithCliffs(minTree);
            mesh = MeshMasher.AutoWelder.AutoWeld(mesh, 0.000001f, 0.01f);

            Filter.mesh = mesh;

            var stack = Maps.Map.SetGlobalDisplayStack();

            var map = new Maps.Map(256, 256);
            map.GetHeightmapFromSquareXZMesh(mesh).Display().SmoothMap(2).Display().Normalise().GetAbsoluteBumpMap().Normalise().BooleanMapFromThreshold(0.05f).Display();

            stack.CreateDebugStack(transform);





        }

        


    }

    
    public List<Mesh> MakeMeshes() {
        IEnumerator<Triangle> triangleEnumerator = _mesh.Triangles.GetEnumerator();

        var output = new List<Mesh>();

        for (int chunkStart = 0; chunkStart < _mesh.Triangles.Count; chunkStart += trianglesInChunk) {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            var chunkEnd = chunkStart + trianglesInChunk;
            for (int i = chunkStart; i < chunkEnd; i++) {
                if (!triangleEnumerator.MoveNext()) {
                    break;
                }

                var triangle = triangleEnumerator.Current;
                

                // For the triangles to be right-side up, they need
                // to be wound in the opposite direction
                var v0 = GetPoint3D(triangle.vertices[2].id);
                var v1 = GetPoint3D(triangle.vertices[1].id);
                var v2 = GetPoint3D(triangle.vertices[0].id);

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


    // Equivalent to calling new Vector3(GetPointLocation(i).x, GetElevation(i), GetPointLocation(i).y)
    public Vector3 GetPoint3D(int index) {
        Vertex vertex = _mesh.vertices[index];
        return new Vector3((float)vertex.x, 0f, (float)vertex.y);
    }

    public void OnDrawGizmos() {
        if (_mesh == null) {
            // Probably in the editor
            return;
        }

        Gizmos.color = Color.red;
        foreach (Edge edge in _mesh.Edges) {
            Vertex v0 = _mesh.vertices[edge.P0];
            Vertex v1 = _mesh.vertices[edge.P1];
            Vector3 p0 = new Vector3((float)v0.x, 0.0f, (float)v0.y);
            Vector3 p1 = new Vector3((float)v1.x, 0.0f, (float)v1.y);
            //Gizmos.DrawLine(p0, p1);
        }
    }
}