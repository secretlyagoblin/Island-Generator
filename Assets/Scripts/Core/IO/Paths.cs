using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Paths
{
    private static string _root = null;

    public static string GetRoot()
    {
        if(_root is null)
            _root = $"{Application.persistentDataPath}";
        return _root;
    }

    public static string Saves => $"{GetRoot()}/Saves"; 

    public static string Autosave => $"{Saves}/autosave.json"; 

    public static string Worlds => $"{GetRoot()}/Worlds"; 

    public static bool TryGetAutosave(out string json)
    {
        var save = Autosave;
        if (File.Exists(save))
        {
            json = File.ReadAllText(save);
            return true;
        }

        json = null;
        return false;

    }

    public static string GetWorldPath(int world) => $"{Worlds}/{world}";

    public static string GetHexGroupManifestPath(int world) => $"{GetWorldPath(world)}/hexGroups/manifest.json";

    public static string GetHexGroupPath(int world, string groupName) => $"{GetWorldPath(world)}/hexGroups/{groupName}.hexgroup.json";

    public static string GetTerrainChunkPathManifestPath(int world) => $"{GetWorldPath(world)}/terrainChunks/manifest.json";

    public static string GetTerrainChunkPath(int world, string groupName) => $"{GetWorldPath(world)}/terrainChunks/{groupName}.terrainChunk.json";


}
