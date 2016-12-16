using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapPattern  {

    public static Map SimpleIsland(int sizeX, int sizeY)
    {
        RNG.DateTimeInit();
        return Map.BlankMap(sizeX, sizeY).CreateCircularFalloff(sizeX * 0.3f).GetDistanceMap(25).Invert()+Map.BlankMap(sizeX, sizeY).PerlinFill(50,0,0,RNG.NextFloat(0,1000)).Remap(-0.3f,0.3f);
    }


}
