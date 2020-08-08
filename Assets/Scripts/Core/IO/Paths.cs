using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Paths
{
    public static string Root { get { return $"{Application.persistentDataPath}"; } }

    public static string Saves { get { return $"{Root}/Saves"; } }

    public static string Autosave { get { return $"{Saves}/autosave.json"; } }

    public static string Worlds { get { return $"{Root}/Worlds"; } }

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

    public static string GetWorldPath(string world) { return $"{Worlds}/{world}"; }
}
