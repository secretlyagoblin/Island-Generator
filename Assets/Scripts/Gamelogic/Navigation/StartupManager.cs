using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartupManager:MonoBehaviour
{
    GameSettings _settings = new GameSettings()
    {
        PlayerPosition = Vector3.zero,
        WorldSettings = new WorldSettings("TestRegion")
    };

    public void StartGame()
    {

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
