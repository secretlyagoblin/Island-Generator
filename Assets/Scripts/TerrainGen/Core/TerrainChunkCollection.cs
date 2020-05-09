using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WanderingRoad.Procgen.RecursiveHex;
using WanderingRoad.Core;

public class TerrainChunkCollection
{
    public List<HexGroup> HexGroups = new List<HexGroup>();
    private int Size;

    private List<TerrainChunk> _chunks;

    public TerrainChunkCollection(List<HexGroup> groups)
    {
        var bounds = groups[0].Bounds;

        foreach (var hexgroup in groups)
        {
            bounds.Encapsulate(hexgroup.Bounds);
        }

        var chunkBounds = new BoundsInt(0, 0, 0, 1000, 1000, 0);

        var chunk = new TerrainChunk(chunkBounds, groups.Where(x => chunkBounds.ToBounds().Intersects(x.Bounds)).ToList());

        _chunks.Add(chunk);

    }


}
