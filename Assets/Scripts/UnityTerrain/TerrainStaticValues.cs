using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainStaticValues  {

    public static readonly int MapResolution = 128;
    public static readonly int DetailResolutionPerPatch = 8;
    public static readonly int BaseTextureResolution = 256;


    public static int DetailMapResolution
    {
        get
        {
            return MapResolution;
        }
    }

    public static int ControlTextureResolution
    {
        get
        {
            return MapResolution;
        }
    }



    public static int HeightmapResolution
    {
        get
        {
            return MapResolution + 1;
        }
    }


}
