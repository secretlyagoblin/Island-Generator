using System;
using UnityEngine;
using System.IO;
using System.Linq;
using WanderingRoad;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

internal class TerrainManager : MonoBehaviour
{
    public GameState State;

    private Dictionary<Rect, Guid> _manifest;
    private Dictionary<Guid, Terrain> _loadedCells;


    BinaryFormatter _formatter = new BinaryFormatter();

    private void Awake()
    {
        State.OnSeedChanged += BuildTerrain;
    }

    void BuildTerrain(GameState state)
    {
        _manifest = Util.DeserialiseFile<Dictionary<Rect, Guid>>(Paths.GetTerrainChunkPathManifestPath(State.Seed), new ManifestSerialiser());

        UpdateCells(state);
    }

    void UpdateCells(GameState state)
    {
        var pos = state.MainCamera.transform.position;
        var rect = new Rect(new Vector2(pos.x, pos.z) - Vector2.one * 50, Vector2.one * 100);
        _loadedCells = _manifest
            .Where(x => x.Key.Overlaps(rect))
            .ToDictionary(
                x => x.Value,
                x => TerrainBuilder.BuildTerrain(
                    Util.DeserialiseFile<TerrainChunk>(
                        Paths.GetTerrainChunkPath(state.Seed,x.Value.ToString()),
                        new TerrainChunkConverter())));



    }
}