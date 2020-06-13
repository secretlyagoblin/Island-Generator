using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderingRoad.Procgen.RecursiveHex;

public class RegionProps
{
    private IDeterminePropRelationships _relationships;

    private Dictionary<Vector3Int, Neighbourhood> _hoods;

    public RegionProps(HexGroup group, IDeterminePropRelationships relationships)
    {
        _relationships = relationships;
        _hoods = group.GetNeighbourhoodDictionary();
    }

    public bool GetFarProps(List<PropData> propDataToWrite)
    {
        return _relationships.GetFarProps(_hoods, propDataToWrite);
    }

    public bool GetCloseProps(List<PropData> propDataToWrite)
    {
        return _relationships.GetCloseProps(_hoods, propDataToWrite);
    }

}

public struct PropData
{
    public Vector2 Position;
    public float HeightGuide;
    public float Yaw;
    public float Rotation;     
    public PropType PropType;
}

public enum PropType
{
    Walkable,
    LowBlocker,
    MediumBlocker,
    HighBlocker,
    Backdrop
}

public interface IDeterminePropRelationships
{
    bool GetCloseProps(Dictionary<Vector3Int, Neighbourhood> hoods, List<PropData> propDataToWrite);

    bool GetFarProps(Dictionary<Vector3Int, Neighbourhood> hoods, List<PropData> propDataToWrite);
}
