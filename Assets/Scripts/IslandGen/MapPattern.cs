using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapPattern  {

    public static Map SimpleIsland(int sizeX, int sizeY)
    {
        RNG.DateTimeInit();

        var map = Map.BlankMap(sizeX, sizeY)
            .CreateCircularFalloff(sizeX * 0.3f)
            .SmoothMap(25)
            .Invert();

        map += Map.BlankMap(sizeX, sizeY)
            .PerlinFillMap(50, 0, 0, RNG.NextFloat(0, 1000), 3, 0.5f, 2f)
            .Remap(-0.6f, 0.3f);

        map.SmoothMap(5).Remap(0,0.5f);

        return map;
    }


}
