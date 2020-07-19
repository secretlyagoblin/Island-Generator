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
    private Dictionary<Guid, TerrainChunk> _loadedCells;


    BinaryFormatter _formatter = new BinaryFormatter();

    private void Awake()
    {
        State.OnSeedChanged += BuildTerrain;
    }

    void BuildTerrain(GameState state)
    {
        _manifest = JsonUtility.FromJson<Dictionary<Rect, Guid>>(File.ReadAllText(state.TerrainManifestPath));
        UpdateCells(state);
    }

    void UpdateCells(GameState state)
    {
        var pos = state.MainCamera.transform.position;
        var rect = new Rect(new Vector2(pos.x, pos.z) - Vector2.one * 50, Vector2.one * 100);
        var cells = _manifest
            .Where(x => x.Key.Overlaps(rect))
            .ToDictionary(
                x => x.Value,
                x => Util.DeserialiseFile<TerrainChunk>(_formatter, $"{state.TerrainRootPath}{x.Value.ToString()}.chunk"));
    }
}