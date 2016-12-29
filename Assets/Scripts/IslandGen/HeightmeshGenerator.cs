using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System;

public class HeightmeshGenerator {

    public HeightmeshPatch GenerateHeightmeshPatch(Map heightMap, MeshLens lens)
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

    public HeightmeshSeam GenerateMeshSeam(Map mapA, Coord coordA, Map mapB,Coord coordB, MeshLens lens)
    {
        if (coordA.TileX == coordB.TileX && coordA.TileY == coordB.TileY)
        {
            Debug.Log("Patches are Identical!");
            return null;
        }

        if (coordA.TileX != coordB.TileX && coordA.TileY != coordB.TileY)
        {
            Debug.Log("Patches are Diagonal!");
            return null;
        }

        var newCoordA = new Coord(0, 0);
        coordB = new Coord(coordB.TileX - coordA.TileX, coordB.TileY - coordA.TileY);
        coordA = new Coord(0, 0);




        Vector3[] seamA;
        Vector3[] seamB;

        var flip = true;

        if (coordA.TileX < coordB.TileX)
        {
            seamA = mapA.GetNormalisedVectorColumn(mapA.SizeX - 1);
            seamB = mapB.GetNormalisedVectorColumn(0);
        }
        else if (coordA.TileX > coordB.TileX)
        {
            seamA = mapA.GetNormalisedVectorColumn(0);
            seamB = mapB.GetNormalisedVectorColumn(mapB.SizeY - 1);
        }
        else if (coordA.TileY < coordB.TileY)
        {
            flip = false;
            seamA = mapA.GetNormalisedVectorRow(mapA.SizeY - 1);
            seamB = mapB.GetNormalisedVectorRow(0);
        }
        else
        {
            flip = false;
            seamA = mapA.GetNormalisedVectorRow(0);
            seamB = mapB.GetNormalisedVectorRow(mapB.SizeY - 1);
        }

        Vector3[] longest;
        Coord longestCoord;
        Vector3[] shortest;
        Coord shortestCoord;

        if (seamA.Length > seamB.Length)
        {
            longest = seamA;
            longestCoord = coordA;
            shortest = seamB;
            shortestCoord = coordB;
            flip = !flip;
        }
        else
        {
            longest = seamB;
            longestCoord = coordB;
            shortest = seamA;
            shortestCoord = coordA;
           

        }

        var seam = new HeightmeshSeam(longest, longestCoord, shortest, shortestCoord, lens);


        var previousShortestIndex = longest.Length;

        for (int longestIndex = 0; longestIndex < longest.Length-1; longestIndex++)
        {

            int shortestIndex = (int)((longestIndex / (float)longest.Length) * shortest.Length);
            shortestIndex += longest.Length;

            if (flip)
            {
                seam.AddTriangle(shortestIndex, longestIndex + 1, longestIndex);
            }
            else
            {
                seam.AddTriangle(longestIndex, longestIndex + 1, shortestIndex);
            }



            if (previousShortestIndex != shortestIndex)
            {
                if (flip)
                {
                    seam.AddTriangle(previousShortestIndex, shortestIndex, longestIndex);
                }
                else
                {
                    seam.AddTriangle(longestIndex , shortestIndex, previousShortestIndex);
                }
                

                previousShortestIndex = shortestIndex;
            }

        }

        

        if (previousShortestIndex + 1 < longest.Length + shortest.Length)
        {


            if (flip)
            {
                seam.AddTriangle(previousShortestIndex, previousShortestIndex + 1, longest.Length - 1);
            }
            else
            {
                seam.AddTriangle(longest.Length - 1, previousShortestIndex + 1, previousShortestIndex);
            }
        }

        //previousIndexLongestSegment = longest.Length-1;
        //var lastIndexA = longest.Length + shortest.Length;

        //seam.AddTriangle(previousIndexLongestSegment, lastIndexA, lastIndexA - 1);



        //for (int a = 0; a < seamA.Length; a++)
        //{
        //    for (int b = 0; b < seamB.Length; b++)
        //    {
        //        var pointA = lens.TransformNormalisedPosition(seamA[a] + coordA.Vector3);
        //        var pointB = lens.TransformNormalisedPosition(seamB[b] + coordB.Vector3);
        //
        //        Debug.DrawLine(pointA, pointB, Color.white, 100f);
        //    }
        //}
        return seam;
    }

    public HeightmeshQuad GenerateMeshCorner(Map startMap, Map upMap, Map rightMap,Map diagonalMap, MeshLens lens)
    {
        var a = startMap.GetNormalisedVector3FromIndex(startMap.SizeX - 1, startMap.SizeY - 1);
        var b = upMap.GetNormalisedVector3FromIndex(upMap.SizeX - 1, 0) + Vector3.forward;
        var c = rightMap.GetNormalisedVector3FromIndex(0, rightMap.SizeY - 1) + Vector3.right;
        var d = new Vector3(1,diagonalMap.GetNormalisedVector3FromIndex(0, 0).y,1);

        //Debug.Log("Start Normalised Quad");
        //Debug.Log(a);

        var quad = new HeightmeshQuad(a, b, c, d, lens);

        quad.AddTriangle(0, 1, 2);
        quad.AddTriangle(1, 3, 2);

        return quad;


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
    public List<int> Triangles
    { get; private set; }

    public HeightmeshSeam(Vector3[] longArray,Coord longCoord, Vector3[] shortArray, Coord shortCoord,  MeshLens lens)
    {
        Vertices = new Vector3[longArray.Length+shortArray.Length];
        UVs = new Vector2[longArray.Length + shortArray.Length];

        CalculateVectorArray(longArray, 0, longCoord, lens);
        CalculateVectorArray(shortArray, longArray.Length, shortCoord, lens);
        Triangles = new List<int>();

        
    }

    void CalculateVectorArray(Vector3[] array, int startIndex, Coord coord, MeshLens lens)
    {
        for (int i = 0; i < array.Length; i++)
        {
            Vertices[i+startIndex] = lens.TransformNormalisedPosition(array[i] + coord.Vector3);

            var uvx = array[i].x;
            if (uvx == 0)
                uvx = 1;

            var uvy = array[i].z;
            if (uvy == 0)
                uvy = 1;

            UVs[i + startIndex] = new Vector2(uvx, uvy);
        }
    }



    

    public void AddTriangle(int a, int b, int c)
    {
        Triangles.Add(a);
        Triangles.Add(b);
        Triangles.Add(c);
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = Vertices;
        mesh.uv = UVs;
        mesh.SetTriangles(Triangles,0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }


}

public class HeightmeshQuad {

    public Vector3[] Vertices
    { get; private set; }
    public Vector2[] UVs
    { get; private set; }
    public List<int> Triangles
    { get; private set; }

    public void AddTriangle(int a, int b, int c)
    {
        Triangles.Add(a);
        Triangles.Add(b);
        Triangles.Add(c);
    }

    public HeightmeshQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, MeshLens lens)
    {
        Vertices = new Vector3[] { lens.TransformNormalisedPosition(a), lens.TransformNormalisedPosition(b), lens.TransformNormalisedPosition(c), lens.TransformNormalisedPosition(d) };
        UVs = new Vector2[] {a,b,c,d};

        Triangles = new List<int>();    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = Vertices;
        mesh.uv = UVs;
        mesh.SetTriangles(Triangles,0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
