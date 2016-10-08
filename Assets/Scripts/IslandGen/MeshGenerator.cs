using UnityEngine;
using System.Collections.Generic;

public class MeshGenerator {

    List<Vector3> _verts = new List<Vector3>();
    List<int> _tris = new List<int>();

    Dictionary<int, List<Triangle>> _triangleDict = new Dictionary<int, List<Triangle>>();

    List<List<int>> _outlines = new List<List<int>>();
    HashSet<int> _checkedVerts = new HashSet<int>();

    int _width;
    int _height;

    MeshLens _lens;

    SquareGrid _squareGrid;

    int _perlinSeed;
    int _detailPerlinSeed;
    int _wallHeightPerlinSeed;


    public Mesh GenerateMesh(Map map, MeshLens lens, int seed)
    {
        _outlines.Clear();
        _checkedVerts.Clear();
        _triangleDict.Clear();

        _lens = lens;

        _squareGrid = new SquareGrid(map, lens);

        _verts = new List<Vector3>();
        _tris = new List<int>();

        RNG.Init(System.DateTime.Now.ToString());
        _perlinSeed = RNG.Next(0, 1000);
        _detailPerlinSeed = RNG.Next(0, 1000);
        _wallHeightPerlinSeed = RNG.Next(0, 1000);

        _width = map.SizeX;
        _height = map.SizeY;




        for (int x = 0; x < _squareGrid.Squares.GetLength(0); x++)
        {
            for (int y = 0; y < _squareGrid.Squares.GetLength(1); y++)
            {
                TriangulateSquare(_squareGrid.Squares[x, y]);
            }
        }

        Mesh mesh = new Mesh();

        var verts = _verts;
        var tris = _tris;

        var vertCountBase = verts.Count;

        var uvs = new List<Vector2>();
        int tileAmount = 10;
        for (int i = 0; i < verts.Count; i++)
        {
            var min = _lens.TransformPosition(0, 0, 0);
            var max = _lens.TransformPosition(map.SizeX, 0, map.SizeY);

            float percentX = Mathf.InverseLerp(min.x, max.x, verts[i].x) * tileAmount;
            float percentY = Mathf.InverseLerp(min.z, max.z, verts[i].z) * tileAmount;

            uvs.Add(new Vector2(percentX, percentY));
        }

        


        var wallMesh = CreateWallMesh();

        uvs.AddRange(wallMesh.uv);



       verts.AddRange(wallMesh.vertices);

       var wallTris = wallMesh.triangles;

        for (int i = 0; i < wallTris.Length; i++)
        {
            wallTris[i] = wallTris[i] + vertCountBase;
        }
        tris.AddRange(wallTris);

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();


        mesh.RecalculateNormals();
        mesh.RecalculateBounds();



        return mesh;
    }

    Mesh CreateWallMesh()
    {
        CalculateMeshOutlines();
        var wallVerts = new List<Vector3>();
        var wallTriangles = new List<int>();
        var wallUvs = new List<Vector2>();
        var wallMesh = new Mesh();

        var wallHeight = global::RNG.NextFloat(3f, 5f);

        for (int i = 0; i < _outlines.Count; i++)
        {
            for (int v = 0; v < _outlines[i].Count-1; v++)
            {
                var outline = _outlines[i];
                var startIndex = wallVerts.Count;
                wallVerts.Add(_verts[outline[v]]);
                wallVerts.Add(_verts[outline[v+1]]);
                wallVerts.Add(_verts[outline[v]] - Vector3.up * wallHeight * ((Mathf.PerlinNoise(_perlinSeed + (_verts[outline[v]].x / (float)_width) * 3f, _perlinSeed + (_verts[outline[v]].z / (float)_height) * 3f))*3f));
                wallVerts.Add(_verts[outline[v + 1]] - Vector3.up * wallHeight * ((Mathf.PerlinNoise(_perlinSeed + (_verts[outline[v+1]].x / (float)_width) * 3f, _perlinSeed + (_verts[outline[v+1]].z / (float)_height) * 3f)) * 3f));

                wallUvs.Add(new Vector2(0, 0));
                wallUvs.Add(new Vector2(0, 1));
                wallUvs.Add(new Vector2(1, 0));
                wallUvs.Add(new Vector2(1, 1));

                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);
            }
        }

        wallMesh.vertices = wallVerts.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        wallMesh.uv = wallUvs.ToArray();

        return wallMesh;

    }

    class SquareGrid {
        public Square[,] Squares
        { get; private set; }

        public SquareGrid(Map map, MeshLens lens)
        {
            int nodeCountX = map.SizeX;
            int nodeCountY = map.SizeY;

            var controlNodes = new ControlNode[nodeCountX, nodeCountY];

            var offset = lens.TransformVector(0.5f, 0, 0.5f);
            Debug.Log(offset);

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {

                    var pos = lens.TransformPosition(x, 0, y) + offset;
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, offset);
                }
            }

            Squares = new Square[nodeCountX - 1, nodeCountY - 1];

            for (int x = 0; x < nodeCountX -1; x++)
            {
                for (int y = 0; y < nodeCountY -1; y++)
                {
                    Squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }

    }

    class Square {
        public ControlNode TopLeft
        { get; private set; }
        public ControlNode TopRight
        { get; private set; }
        public ControlNode BottomRight
        { get; private set; }
        public ControlNode BottomLeft
        { get; private set; }

        public Node CentreTop
        { get; private set; }
        public Node CentreRight
        { get; private set; }
        public Node CentreBottom
        { get; private set; }
        public Node CentreLeft
        { get; private set; }

        public int Configuration
        { get; private set; }

        public Square(ControlNode topLeft, ControlNode topRight, ControlNode bottomRight, ControlNode bottomLeft)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;

            CentreTop = topLeft.Right;
            CentreRight = BottomRight.Above;
            CentreBottom = BottomLeft.Right;
            CentreLeft = bottomLeft.Above;

            if (topLeft.Active)
            {
                Configuration += 8;
            }
            if (topRight.Active)
            {
                Configuration += 4;
            }
            if (bottomRight.Active)
            {
                Configuration += 2;
            }
            if (BottomLeft.Active)
            {
                Configuration += 1;
            }

        }




    }

    class Node {
        public Vector3 Position
        { get; private set; }
        public int VertexIndex        
        { get; set; }

    public Node(Vector3 position)
        {
            VertexIndex = -1;
            Position = position;
        }
    }

    class ControlNode: Node {
        public bool Active
        { get; private set; }
        public Node Above
        { get; private set; }
        public Node Right
        { get; private set; }

        public ControlNode(Vector3 position, bool active, Vector3 offset) : base(position)
        {
            Active = active;
            Above = new Node(position + Vector3.forward * offset.z);
            Right = new Node(position + Vector3.right * offset.x);
        }
    }

    void TriangulateSquare(Square square)
    {
        switch (square.Configuration)
        {
            case 0:
                break;

            // 1 points:
            case 1:
                MeshFromPoints(square.CentreLeft, square.CentreBottom, square.BottomLeft);
                break;
            case 2:
                MeshFromPoints(square.BottomRight, square.CentreBottom, square.CentreRight);
                break;
            case 4:
                MeshFromPoints(square.TopRight, square.CentreRight, square.CentreTop);
                break;
            case 8:
                MeshFromPoints(square.TopLeft, square.CentreTop, square.CentreLeft);
                break;

            // 2 points:
            case 3:
                MeshFromPoints(square.CentreRight, square.BottomRight, square.BottomLeft, square.CentreLeft);
                break;
            case 6:
                MeshFromPoints(square.CentreTop, square.TopRight, square.BottomRight, square.CentreBottom);
                break;
            case 9:
                MeshFromPoints(square.TopLeft, square.CentreTop, square.CentreBottom, square.BottomLeft);
                break;
            case 12:
                MeshFromPoints(square.TopLeft, square.TopRight, square.CentreRight, square.CentreLeft);
                break;
            case 5:
                MeshFromPoints(square.CentreTop, square.TopRight, square.CentreRight, square.CentreBottom, square.BottomLeft, square.CentreLeft);
                break;
            case 10:
                MeshFromPoints(square.TopLeft, square.CentreTop, square.CentreRight, square.BottomRight, square.CentreBottom, square.CentreLeft);
                break;

            // 3 point:
            case 7:
                MeshFromPoints(square.CentreTop, square.TopRight, square.BottomRight, square.BottomLeft, square.CentreLeft);
                break;
            case 11:
                MeshFromPoints(square.TopLeft, square.CentreTop, square.CentreRight, square.BottomRight, square.BottomLeft);
                break;
            case 13:
                MeshFromPoints(square.TopLeft, square.TopRight, square.CentreRight, square.CentreBottom, square.BottomLeft);
                break;
            case 14:
                MeshFromPoints(square.TopLeft, square.TopRight, square.BottomRight, square.CentreBottom, square.CentreLeft);
                break;

            // 4 point:
            case 15:
                MeshFromPoints(square.TopLeft, square.TopRight, square.BottomRight, square.BottomLeft);
                _checkedVerts.Add(square.TopLeft.VertexIndex);
                _checkedVerts.Add(square.TopRight.VertexIndex);
                _checkedVerts.Add(square.BottomRight.VertexIndex);
                _checkedVerts.Add(square.BottomLeft.VertexIndex);
                break;
        }
    }

    void MeshFromPoints(params Node[] points)
    {
        AssignVerts(points);

        if (points.Length >= 3)
        {
            CreateTriangle(points[0], points[1], points[2]);
        }
        if (points.Length >= 4)
        {
            CreateTriangle(points[0], points[2], points[3]);
        }
        if (points.Length >= 5)
        {
            CreateTriangle(points[0], points[3], points[4]);
        }
        if (points.Length >= 6)
        {
            CreateTriangle(points[0], points[4], points[5]);
        }

    }

    struct Triangle {
        public int A
        { get; private set; }

        public int B
        { get; private set; }

        public int C
        { get; private set; }

        int[] _vertices;

        public Triangle (int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;

            _vertices = new int[3];
            _vertices[0] = a;
            _vertices[1] = b;
            _vertices[2] = c;
        }

        public int this[int i]
        {
            get
            {
                return _vertices[i];
            }
        }



        public bool Contains(int vertexIndex)
        {
            return vertexIndex == A || vertexIndex == B || vertexIndex == C;
        }
    }

    void AssignVerts(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if(points[i].VertexIndex == -1)
            {
                points[i].VertexIndex = _verts.Count;

                var point = points[i].Position;

                var y = Mathf.PerlinNoise(_perlinSeed + (point.x / (float)_width) * 30f, _perlinSeed + (point.z / (float)_height) * 30);
                var yTopper = Mathf.PerlinNoise(_detailPerlinSeed + (point.x / (float)_width) * 300f, _detailPerlinSeed + (point.z / (float)_height) * 300);

                y = y - 0.5f;
                y = y * 0.7f;

                yTopper = yTopper - 0.5f;
                yTopper = yTopper * 0.2f;
                y += yTopper;

                point.y += y;

                _verts.Add(point);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c)
    {
        _tris.Add(a.VertexIndex);
        _tris.Add(b.VertexIndex);
        _tris.Add(c.VertexIndex);

        Triangle triangle = new Triangle(a.VertexIndex, b.VertexIndex, c.VertexIndex);
        AddTriangeToDictionary(triangle.A, triangle);
        AddTriangeToDictionary(triangle.B, triangle);
        AddTriangeToDictionary(triangle.C, triangle);
    }

    void AddTriangeToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (_triangleDict.ContainsKey(vertexIndexKey))
        {
            _triangleDict[vertexIndexKey].Add(triangle);
        }
        else
        {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            _triangleDict.Add(vertexIndexKey, triangleList);
        }
    }

    int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = _triangleDict[vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++)
        {
            Triangle triangle = trianglesContainingVertex[i];

            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];
                if (vertexB != vertexIndex && !_checkedVerts.Contains(vertexB))
                {
                    if (IsOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }

        return -1;
    }

    void CalculateMeshOutlines()
    {

        for (int vertexIndex = 0; vertexIndex < _verts.Count; vertexIndex++)
        {
            if (!_checkedVerts.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1)
                {
                    _checkedVerts.Add(vertexIndex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    _outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, _outlines.Count - 1);
                    _outlines[_outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    void FollowOutline(int vertexIndex, int outlineIndex)
    {
        _outlines[outlineIndex].Add(vertexIndex);
        _checkedVerts.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

        if (nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    bool IsOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesContainingVertexA = _triangleDict[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; i++)
        {
            if (trianglesContainingVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1)
                {
                    break;
                }
            }
        }
        return sharedTriangleCount == 1;
    }
}
