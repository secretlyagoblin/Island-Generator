﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WanderingRoad.Procgen.RecursiveHex;
using WanderingRoad;
using System;

public class TerrainChunkCollection
{
    public List<HexGroup> HexGroups = new List<HexGroup>();
    public int Size { get; private set; }
    public int Multiplier { get; private set; }

    internal List<TerrainChunk> _chunks = new List<TerrainChunk>();

    private Func<float, float, HexPayload, float> _heightCalculation;

    public TerrainChunkCollection(List<HexGroup> groups, int Size, int Multiplier, Func<float, float, HexPayload, float> heightCalculation)
    {
        var bounds = groups[0].Bounds;

        foreach (var hexgroup in groups)
        {
            bounds.Encapsulate(hexgroup.Bounds);
        }

        HexGroups = groups;

        var chunkBounds = new RectInt(-Size, -Size, Size * 2, Size * 2);

        var chunk = new TerrainChunk(chunkBounds, groups.Where(x => chunkBounds.ToBounds().Overlaps(x.Bounds)).ToList(),Multiplier, heightCalculation);

        chunk.Bounds.ToBounds().DrawRect(Color.blue, 100f);
        chunk.ScaledBounds.ToBounds().DrawRect(Color.red, 100f);

        _chunks.Add(chunk);

    }

    public Vector3[][] GetPositions()
    {     
        return _chunks.Select(x => x.To1dPositionArray()).ToArray();
    }


}
