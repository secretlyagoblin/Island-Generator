using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class ColorPalette: ScriptableObject {

    // Use this for initialization
    public Gradient GroundColor;
    public Gradient CliffColor;

    [MenuItem("Assets/Create/Color Palette")]
    public static void CreateAsset()
    {
        Util.CreateAsset<ColorPalette>();
    }


    // Update is called once per frame

}
