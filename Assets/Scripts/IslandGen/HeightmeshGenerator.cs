using UnityEngine;
using System.Collections;

public static class HeightmeshGenerator {

    public static HeightmeshPatch GenerateHeightmeshPatch(Map heightMap, MeshLens lens)
    {
        var sizeX = heightMap.SizeX;
        var sizeY = heightMap.SizeY;

        var heightmesh = new HeightmeshPatch(sizeX, sizeY);
        var vertexIndex = 0;

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                var normalisedX = x / (float)(sizeX);
                var normalisedY = y / (float)(sizeY);

                heightmesh.AddVertex(lens.TransformNormalisedPosition(normalisedX, heightMap[x, y], normalisedY), vertexIndex);

                //Debug.Log("x: " + normalisedX + " y: " + heightMap[x, y] + " z: " + normalisedY);

                heightmesh.AddUV(new Vector2(normalisedX, normalisedY), vertexIndex);

                if(x < sizeX-1 && y < sizeY - 1)
                {
                    heightmesh.AddTriangle(vertexIndex, vertexIndex + sizeX + 1, vertexIndex + sizeX);
                    heightmesh.AddTriangle(vertexIndex+sizeX+1, vertexIndex, vertexIndex + 1);
                }
                vertexIndex++;
            }
        }
        return heightmesh;
    }

    public static HeightmeshSeam GenerateMeshSeam(Map MapA, Coord CoordA, Map MapB,Coord CoordB, MeshLens lens)
    {
        if (CoordA.TileX == CoordB.TileX && CoordA.TileY == CoordB.TileY)
        {
            Debug.Log("Patches are Identical!");
            return null;
        }

        if (CoordA.TileX != CoordB.TileX && CoordA.TileY != CoordB.TileY)
        {
            Debug.Log("Patches are Diagonal!");
            return null;
        }

        Vector3[] seamA;
        Vector3[] seamB;

        if (CoordA.TileX < CoordB.TileX)
        {
            seamA = MapA.GetNormalisedVectorColumn(MapA.SizeX - 1);
            seamB = MapB.GetNormalisedVectorColumn(0);
        }
        else if (CoordA.TileX > CoordB.TileX)
        {
            seamA = MapA.GetNormalisedVectorColumn(0);
            seamB = MapB.GetNormalisedVectorColumn(MapB.SizeY - 1);
        }
        else if (CoordA.TileY < CoordB.TileY)
        {

            seamA = MapA.GetNormalisedVectorRow(MapA.SizeY - 1);
            seamB = MapB.GetNormalisedVectorRow(0);
        }
        else
        {
            seamA = MapA.GetNormalisedVectorRow(0);
            seamB = MapB.GetNormalisedVectorRow(MapB.SizeY - 1);
        }



        for (int a = 0; a < seamA.Length; a++)
        {
            for (int b = 0; b < seamB.Length; b++)
            {
                var pointA = lens.TransformNormalisedPosition(seamA[a] + CoordA.Vector3);
                var pointB = lens.TransformNormalisedPosition(seamB[b] + CoordB.Vector3);

                Debug.DrawLine(pointA, pointB, Color.white, 100f);
            }
        }



        return new HeightmeshSeam();
    }
}

public class HeightmeshPatch {

    public Vector3[] Vertices
    { get; private set; }
    public Vector2[] UVs
    { get; private set; }
    public int[] Triangles
    { get; private set; }

    int _triangleIndex = 0;

    public HeightmeshPatch(int meshWidth, int meshHeight)
    {
        Vertices = new Vector3[meshWidth * meshHeight];
        UVs = new Vector2[meshWidth * meshHeight];
        Triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        Triangles[_triangleIndex] = a;
        Triangles[_triangleIndex+1] = b;
        Triangles[_triangleIndex+2] = c;

        _triangleIndex += 3;
    }

    public void AddVertex(Vector3 vector, int index)
    {
        Vertices[index] = vector;
    }

    public void AddUV(Vector2 vector, int index)
    {
        UVs[index] = vector;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = Vertices;
        mesh.uv = UVs;
        mesh.triangles = Triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

}

public class HeightmeshSeam {

    public Vector3[] Vertices
    { get; private set; }
    public Vector2[] UVs
    { get; private set; }
    public int[] Triangles
    { get; private set; }

    int _triangleIndex = 0;

    float[] _mapA;
    float[] _mapB;



}
