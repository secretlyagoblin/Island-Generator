using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct GroupData : ISharedComponentData
{
    public int Id;
    public float3 Position;
    //public float Distance;
}