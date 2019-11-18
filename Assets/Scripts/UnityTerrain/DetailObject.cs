using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain/Detail Object")]
public class DetailObject : ScriptableObject {

    public Texture2D DetailTexture;
    public float MinWidth = 1f;
    public float MaxWidth = 2f;
    public float MinHeight = 1f;
    public float MaxHeight = 2f;
    public float NoiseSpread = 0.1f;
    public Color HealthyColor = Color.green;
    public Color DryColor = Color.yellow;
    public bool IsBillboard = true;

    public DetailPrototype GetDetail()
    {
        var detailPrototype = new DetailPrototype();
        detailPrototype.prototypeTexture = DetailTexture;

        detailPrototype.minWidth = MinWidth;
        detailPrototype.maxWidth = MaxWidth;
        detailPrototype.minHeight = MinHeight;
        detailPrototype.maxHeight = MaxHeight;

        detailPrototype.noiseSpread = NoiseSpread;
        detailPrototype.healthyColor = HealthyColor;
        detailPrototype.dryColor = DryColor;
        //detailPrototype.usePrototypeMesh = false;
        detailPrototype.renderMode = DetailRenderMode.GrassBillboard;

        return detailPrototype;

    }


}
