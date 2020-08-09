using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Paths
{
    public static string Root => $"{Application.persistentDataPath}"; 

    public static string Saves => $"{Root}/Saves"; 

    public static string Autosave => $"{Saves}/autosave.json"; 

    public static string Worlds => $"{Root}/Worlds"; 

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

    public static string GetWorldPath(string world) => $"{Worlds}/{world}";

    public static string GetHexGroupManifestPath(string world) => $"{GetWorldPath(world)}/hexGroups/manifest.json";

    public static string GetHexGroupPath(string world, string groupName) => $"{GetWorldPath(world)}/hexGroups/{groupName}.hexgroup.json";
}
