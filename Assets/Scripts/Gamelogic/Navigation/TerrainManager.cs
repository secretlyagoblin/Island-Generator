using System;
using UnityEngine;
using System.IO;
using System.Linq;

internal class TerrainManager
{
    public Rect RenderRect { get; internal set; }


    internal void LoadRegion(WorldSettings settings)
    {
        //Clear and delete old data?

        var path = settings.DataPath + "Terrain";
        var chunks = Directory.GetFiles(path, "*.terrain.json");
        var fileHelper = new WanderingRoad.IO.SaveHelper<TerrainChunk>();
        var finalChunks = chunks.Select(x => fileHelper.LoadAsset<SerialisedChunk>(x)).ToList();

        //TerrainBuilder.BuildTerrain(finalChunks);

        throw new NotImplementedException();
    }
}