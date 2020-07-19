using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public delegate void RegionChanged(WorldSettings settings);
    public event RegionChanged OnWorldSettingsChanged;

    public WorldSettings WorldSettings { get { return _worldSettings; } set { _worldSettings=  value; OnWorldSettingsChanged(_worldSettings); } }
    private WorldSettings _worldSettings;

    public Rect RenderRect { get { return _renderRect; } set { _renderRect = value; _renderZoneNeedsUpdating = true; } }
    private Rect _renderRect;

    private bool _renderZoneNeedsUpdating = true;

    //private PropManager _propManager;
    private TerrainManager _terrainManager;

    private void Awake()
    {
        OnWorldSettingsChanged += LoadRegion;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void LoadRegion(WorldSettings settings)
    {
        if (!RegionExistsOnDisk(settings))
        {
            CreateRegion(settings);
        }

        //_terrainManager.LoadRegion(settings);
    }

    private void CreateRegion(WorldSettings settings)
    {
        throw new NotImplementedException();
    }

    private bool RegionExistsOnDisk(WorldSettings settings)
    {
        throw new NotImplementedException();
    }
}
