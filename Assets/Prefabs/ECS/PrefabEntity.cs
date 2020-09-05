using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct PrefabEntity : IComponentData
{
    public Entity SpherePrefab;
}
