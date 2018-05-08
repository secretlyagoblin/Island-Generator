using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain/Terrain Splat")]
public class TerrainSplat : ScriptableObject {

    public Texture2D MainTexture;
    public Texture2D NormalMap;

    public SplatPrototype GetSplat()
    {
        var splat = new SplatPrototype();
        splat.texture = MainTexture;
        splat.normalMap = NormalMap;

        return splat;
    }


}
