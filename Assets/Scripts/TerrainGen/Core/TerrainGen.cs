using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderingRoad.Procgen.RecursiveHex;

public struct StampData
{
    public bool Filled;
}

public class TerrainStamp
{
    public BoundsInt Bounds;

    public StampData[,] Map;

    public TerrainStamp(BoundsInt bounds)
    {
        Bounds = bounds;
        Map = new StampData[bounds.size.x+1, bounds.size.y+1];

    }

    public bool ApplyPixels(Neighbourhood neighbourhood, System.Func<HexPayload,StampData> func)
    {
        var bounds = GetHexBounds(neighbourhood);
        bounds.ClampToBounds(Bounds);

        for (int i = bounds; i < length; i++)
        {

        }
    }

    private static BoundsInt GetHexBounds(Neighbourhood neighbourhood)
    {
        throw new NotImplementedException();
    }

}
