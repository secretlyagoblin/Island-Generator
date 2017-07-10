using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain/TerrainSettings")]
public class ProcTerrainSettings : ScriptableObject {

    public SplatCollection SplatCollection;
    public Gradient MajorRocks;
    public Gradient MinorRocks;
    public AnimationCurve Falloff;

    public AnimationCurve LargeSizeMultiplier;
    public AnimationCurve SmallSizeMultiplier;
    public AnimationCurve CliffFalloff;

    public DetailObjectCollection Details;

    public GameObject RockToCreateSteep;
    public GameObject RockToCreateShallow;
    public GameObject Tree;
    public GameObject Sphere;
}
