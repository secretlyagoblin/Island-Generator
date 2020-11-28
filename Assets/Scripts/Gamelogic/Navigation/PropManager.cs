using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using WanderingRoad;
using WanderingRoad.Procgen.RecursiveHex;
using WanderingRoad.Procgen.RecursiveHex.Json;
using WanderingRoad.Random;

using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Jobs;

public class PropManager : MonoBehaviour
{
    public GameState State;

    private Dictionary<Rect, Guid> _manifest;

    Entity Prefab;

    private List<PropRegion> _regions = new List<PropRegion>();


    private void Awake()
    {
        State.OnSeedChanged += BuildProps;
        State.OnTerrainLoaded += UpdateCells;
        
    }

    private void Start()
    {
        Prefab = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(PrefabLibrary)).GetSingleton<PrefabLibrary>().Sphere;
    }

    private void Update()
    {
        var pos = State.MainCamera.transform.position;

        var pos2d = new Vector2(pos.x, pos.z);

        foreach (var region in _regions)
        {
            region.Update(pos2d);
        }
    }

    private void BuildProps(GameState state)
    {
        _manifest = Util.DeserialiseFile<Dictionary<Rect, Guid>>(Paths.GetHexGroupManifestPath(State.Seed), new ManifestSerialiser());
    }

    void UpdateCells(GameState state)
    {

        _regions = _manifest.Select(x => new PropRegion(state, x.Key, x.Value)).ToList();

    }

    private void OnDestroy()
    {
        //foreach (var region in _regions)
        //{
        //    region.Dispose();
        //}
    }


}