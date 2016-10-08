using UnityEngine;
using System.Collections;

public class MeshLens {

    float _mapSizeX;
    float _mapSizeY;

    float _mapOffsetX;
    float _mapOffsetY;

    Vector3 _mapScale;

    public MeshLens(float mapSizeX, float mapSizeY, Vector3 mapScale)
    {
        _mapSizeX = mapSizeX;
        _mapSizeY = mapSizeY;
        _mapScale = mapScale;

        _mapOffsetX = _mapSizeX * 0.5f;
        _mapOffsetY = _mapSizeY * 0.5f;
    }

    public Vector3 TransformPosition(Vector3 vector)
    {
        var returnVector = new Vector3(vector.x - _mapOffsetX, vector.y, vector.z - _mapOffsetY);
        returnVector.Scale(_mapScale);
        return returnVector;
    }

    public Vector3 TransformPosition(float x, float y, float z)
    {
        var returnVector = new Vector3(x - _mapOffsetX, y, z - _mapOffsetY);
        returnVector.Scale(_mapScale);
        return returnVector;
    }

    public Vector3 TransformVector(Vector3 vector)
    {
        return TransformVector(vector.x, vector.y, vector.z);
    
    }
    
    public Vector3 TransformVector(float x, float y, float z)
    {
        var returnVector = new Vector3(x, y, z);
        returnVector.Scale(_mapScale);
        return returnVector;
    }

}
