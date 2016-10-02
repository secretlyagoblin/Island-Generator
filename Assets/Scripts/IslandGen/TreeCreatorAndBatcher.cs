using UnityEngine;
using System.Collections.Generic;

public static class TreeCreatorAndBatcher {

    static List<Mesh> _MeshesToReturn;

    static List<Vector3> _currentVerts;
    static List<Vector2> _currentUvs;
    static List<int> _currentTris;

    static float _baseWidth;
    static float _baseHeight;
    static float _tipHeight;
    static float _horizontalDeviation;
    static float _verticalDeviation;

    public static Mesh[] CreateTreeMeshesFromPositions(float baseWidth, float baseHeight, float tipHeight, float horizontalDeviation, float verticalDeviation, Vector3[] positions)
    {
        _baseWidth = baseWidth;
        _baseHeight = baseHeight;
        _tipHeight = tipHeight;
        _horizontalDeviation = horizontalDeviation;
        _verticalDeviation = verticalDeviation;

        _MeshesToReturn = new List<Mesh>();
        _currentVerts = new List<Vector3>();
        _currentUvs = new List<Vector2>();
        _currentTris = new List<int>();

        for (int i = 0; i < positions.Length; i++)
        {
            if(_currentVerts.Count+5 >= 65000) StoreAndClearMesh();
            AddTree(positions[i]);
        }


        StoreAndClearMesh();

        return _MeshesToReturn.ToArray();
    }

    static void AddTree(Vector3 positon)
    {
        var offsetCenter = positon + ((Vector3.up * (_baseHeight + jitterHorizontal())));
        var halfWidth = _baseWidth * 0.5f;

        var a = new Vector3(offsetCenter.x + halfWidth + jitterHorizontal(), offsetCenter.y + jitterHorizontal(), offsetCenter.z + halfWidth + jitterHorizontal());
        var b = new Vector3(offsetCenter.x + halfWidth + jitterHorizontal(), offsetCenter.y + jitterHorizontal(), offsetCenter.z - halfWidth + jitterHorizontal());
        var c = new Vector3(offsetCenter.x - halfWidth + jitterHorizontal(), offsetCenter.y + jitterHorizontal(), offsetCenter.z - halfWidth + jitterHorizontal());
        var d = new Vector3(offsetCenter.x - halfWidth + jitterHorizontal(), offsetCenter.y + jitterHorizontal(), offsetCenter.z + halfWidth + jitterHorizontal());

        var tip = offsetCenter + ((Vector3.up * (_tipHeight + jitterVertical())));
        tip.x += jitterHorizontal();
        tip.z += jitterHorizontal();

        var currentCount = _currentVerts.Count;

        _currentVerts.Add(a);
        _currentVerts.Add(b);
        _currentVerts.Add(c);
        _currentVerts.Add(d);
        _currentVerts.Add(tip);



        _currentUvs.Add(new Vector2(0,0));
        _currentUvs.Add(new Vector2(0, 1));
        _currentUvs.Add(new Vector2(0, 0));
        _currentUvs.Add(new Vector2(0, 1));
        _currentUvs.Add(new Vector2(1, 0.5f));

        _currentTris.Add(currentCount+1);
        _currentTris.Add(currentCount+4);
        _currentTris.Add(currentCount );

        _currentTris.Add(currentCount + 2);
        _currentTris.Add(currentCount + 4);
        _currentTris.Add(currentCount + 1);

        _currentTris.Add(currentCount+3);
        _currentTris.Add(currentCount + 4);
        _currentTris.Add(currentCount+2);

        _currentTris.Add(currentCount);
        _currentTris.Add(currentCount + 4);
        _currentTris.Add(currentCount +3);

    }

    static float jitterHorizontal()
    {
        float deviation = _horizontalDeviation;
        deviation -= (_horizontalDeviation / 2);
        var value = RNG.NextFloat(-deviation, deviation);
        return value;
    }

    static float jitterVertical()
    {
        float deviation = _verticalDeviation;
        deviation -= deviation / 2;
        var value = RNG.NextFloat(-deviation, deviation);
        return value;
    }

    static void StoreAndClearMesh()
    {
        var mesh = new Mesh();
        mesh.vertices = _currentVerts.ToArray();
        mesh.triangles = _currentTris.ToArray();
        mesh.uv = _currentUvs.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        _MeshesToReturn.Add(mesh);

        _currentVerts.Clear();
        _currentTris.Clear();
    }

    


}
