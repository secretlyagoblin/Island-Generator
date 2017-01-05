using UnityEngine;
using System.Collections;

public class MeshLens {

    float _mapSizeX;
    float _mapSizeY;

    float _mapOffsetX;
    float _mapOffsetY;

    Vector3 _mapScale;

    public MeshLens(Vector3 mapScale)
    {
        _mapSizeX = mapScale.x;
        _mapSizeY = mapScale.y;
        _mapScale = mapScale;

    }

    public Vector3 TransformNormalisedPosition(Vector3 vector)
    {
        var returnVector = new Vector3(vector.x - 0.5f, vector.y, vector.z - 0.5f);
        returnVector.Scale(_mapScale);
        return returnVector;
    }

    public Vector3 TransformNormalisedPosition(float x, float y, float z)
    {
        //var returnVector = new Vector3(x - 0.5f, y, z - 0.5f);
        var returnVector = new Vector3(x, y, z);
        returnVector.Scale(_mapScale);
        return returnVector;
    }

    public Vector3 TransformNormalisedVector(Vector3 vector)
    {
        return TransformNormalisedVector(vector.x, vector.y, vector.z);
    
    }
    
    public Vector3 TransformNormalisedVector(float x, float y, float z)
    {
        var returnVector = new Vector3(x, y, z);
        returnVector.Scale(_mapScale);
        return returnVector;
    }

    public Vector3 TransformWorldspaceVector(Vector3 vector)
    {
        return new Vector3(vector.x - (_mapScale.x*0.5f), vector.y, vector.z - (_mapScale.z * 0.5f));
    }
}
