using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderingRoad.Random;

public class StartupManager:MonoBehaviour
{
    public Camera MainCamera;
    public bool WipeAutosave = false;

    GameSettings _settings = new GameSettings()
    {
        PlayerPosition = Vector3.zero,
        WorldSettings = new WorldSettings("TestRegion")
    };

    public GameState State;

    public void Start()
    {
        RNG.DateTimeInit();

        State.MainCamera = MainCamera;

        if (WipeAutosave && System.IO.File.Exists(Paths.Autosave))
        {
            System.IO.File.Delete(Paths.Autosave);
        }

        if(Paths.TryGetAutosave(out var json))
        {
            Debug.Log("Autosave exists!");
            State.UpdateFromJson(json);
        }
        else
        {
            var info = WanderingRoad.Procgen.Levelgen.LevelBuilder.BuildLevel(DateTime.Now.ToString());
            State.UpdateFromLevelInfo(info);
            State.Save(Paths.Autosave);                  
        }
    }
}

[Serializable]
public class GameSettings
{
    public Vector3 PlayerPosition;
    public WorldSettings WorldSettings;
}

[Serializable]
public readonly struct WorldSettings
{
    public readonly string Name;
    public string DataPath { get { return $"{Application.persistentDataPath}/{Name}/"; } }

    public WorldSettings(string name)
    {
        Name = name;
    }        
}
