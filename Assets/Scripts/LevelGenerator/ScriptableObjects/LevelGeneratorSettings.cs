using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LevelGenerator {

    [CreateAssetMenu(fileName = "Level Generator Settings", menuName = "WorldGen/Level Generator Settings", order = 1)]
    public class LevelGeneratorSettings: ScriptableObject {

        public Material MeshColourMaterial;
        public GameObject TemplateObject;
        public TextAsset MeshTileData;
    }
}
