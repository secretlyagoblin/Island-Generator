using System;
using UnityEngine;
using System.Linq;
using WanderingRoad;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Authoring;

internal class TerrainManager : MonoBehaviour
{
    public GameState State;

    private TerrainManifest _manifest;
    public List<Terrain> Terrains =  new List<Terrain>();
    private readonly Queue<string> _errors = new Queue<string>();

    private void Awake()
    {
        State.OnSeedChanged += BuildTerrain;
    }

    void BuildTerrain(GameState state)
    {       
        _manifest = Util.DeserialiseFile<TerrainManifest>(Paths.GetTerrainChunkPathManifestPath(State.Seed), new TerrainManifestSerialiser());

        UpdateCells(state);
    }

    private readonly Queue<ChunkThreadData> _chunks = new Queue<ChunkThreadData>();

    public int JobsRunning = 0;
    public bool ExpectingJobs = true;

    void UpdateCells(GameState state)
    {
        ExpectingJobs = true;

        var pos = Vector3.zero
            //- new Vector3(12, 0, 12)
            ;
        var rect = new Rect(new Vector2(pos.x, pos.z) - Vector2.one * 1, Vector2.one * 2);

        var cells = _manifest.Terrains
            .Where(x => x.Key.Overlaps(rect)).ToList();

        JobsRunning = cells.Count;

        foreach (var cell in cells)
        {
            Task.Delay(100).ContinueWith(x =>
            {
                try
                {
                    var terrainChunk =
                     Util.DeserialiseFile<TerrainChunk>(
                        Paths.GetTerrainChunkPath(state.Seed, cell.Value.ToString()),
                        new TerrainChunkConverter());

                    var map = terrainChunk.GetResizedHeightmap(1025, _manifest.MinHeight, _manifest.MaxHeight);

                    //{ lock (_errors) { _errors.Enqueue("should be loading a fuckin chunk"); } }

                    {
                        lock (_chunks)
                        {
                            _chunks.Enqueue(new ChunkThreadData()
                            {
                                Values = map,
                                Guid = cell.Value,
                                Size = terrainChunk.ScaledBounds.size,
                                Position = new Vector3(
                                    terrainChunk.Bounds.x * terrainChunk.Multiplier,
                                    0,
                                    terrainChunk.Bounds.y * terrainChunk.Multiplier)
                            });
                        }
                    }


                }
                catch (Exception ex)
                {
                    { lock (_errors) { _errors.Enqueue(ex.Message); } }
                }  
            });               
        }
    }

    void Update()
    {
        while(_errors.Count > 0)
            Debug.LogError(_errors.Dequeue());

        if(_chunks.Count > 0)
        {
            var chunk = _chunks.Dequeue();
            var data = TerrainBuilder.BuildTerrainData(chunk.Values, chunk.Size, _manifest);
            var terrain = TerrainBuilder.BuildTerrain(data, chunk.Position);
            terrain.name = chunk.Guid.ToString();
            terrain.gameObject.transform.parent = transform;
            Terrains.Add(terrain);

            GetTerrainColliderEntity(chunk, _manifest);

            JobsRunning--;
        }

        if(ExpectingJobs && JobsRunning == 0)
        {
            ExpectingJobs = false;
            State.TerrainLoaded();
        }           
    }

    private void GetTerrainColliderEntity(ChunkThreadData chunk, TerrainManifest manifest)
    {
        var values = chunk.Values;

        var heights = new NativeArray<float>(values.Length * values.Length, Allocator.Temp);

        var count = 0;

        var size = chunk.Size;

        var height = (manifest.MaxHeight - manifest.MinHeight);

        for (int x = 0; x < values.GetLength(0); x++)
        {
            for (int y = 0; y < values.GetLength(1); y++)
            {
                heights[count] = values[x, y] * height; //30;

                count++;                
            }
        }

        Debug.Log($"Count ended up at {count}, or {Mathf.Sqrt(count)}");

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var terrain = Unity.Physics.TerrainCollider.Create(
            heights, 
            new Unity.Mathematics.int2(1025, 1025), 
            new Unity.Mathematics.float3(size.x/1025, 1, size.y/1025), 
            Unity.Physics.TerrainCollider.CollisionMethod.VertexSamples,
            CollisionFilter.Default);

        heights.Dispose();

        var entity = entityManager.CreateEntity(typeof(Translation), typeof(PhysicsCollider), typeof(LocalToWorld), typeof(Rotation), typeof(PhysicsDebugDisplayData));
        entityManager.SetName(entity, $"Terrain {chunk.Position}");

        entityManager.SetComponentData(entity, new Translation() { Value = chunk.Position });
        entityManager.SetComponentData(entity, new PhysicsCollider() { Value = terrain });
        //entityManager.SetComponentData(entity, new PhysicsDebugDisplayData() { DrawColliders = 1 });
    }

    private class ChunkThreadData
    {
        public float[,] Values;
        public Vector2 Size;
        public Guid Guid;
        public Vector3 Position;
    }
}