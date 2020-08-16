using System;
using UnityEngine;
using System.Linq;
using WanderingRoad;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

internal class TerrainManager : MonoBehaviour
{
    public GameState State;

    private TerrainManifest _manifest;
    private Dictionary<Guid, Terrain> _loadedCells;

    public List<Terrain> Terrains =  new List<Terrain>();

    private Queue<string> _errors = new Queue<string>();

    //private Queue<float[,]>)



    private void Awake()
    {
        State.OnSeedChanged += BuildTerrain;
    }

    void BuildTerrain(GameState state)
    {
       
        _manifest = Util.DeserialiseFile<TerrainManifest>(Paths.GetTerrainChunkPathManifestPath(State.Seed), new TerrainManifestSerialiser());

        UpdateCells(state);
    }

    private Queue<ChunkThreadData> _chunks = new Queue<ChunkThreadData>();



    void UpdateCells(GameState state)
    {
        //throw new NotImplementedException();
        //var pos = state.MainCamera.transform.position;
        var pos = Vector3.zero;
       var rect = new Rect(new Vector2(pos.x, pos.z) - Vector2.one * 200, Vector2.one * 400);
       // _loadedCells = _manifest.Terrains
       //     .Where(x => x.Key.Overlaps(rect))
       //     .ToDictionary(
       //         x => x.Value,
       //         x => TerrainBuilder.BuildTerrain(
       //             TerrainBuilder.BuildTerrainData(
       //             Util.DeserialiseFile<TerrainChunk>(
       //                 Paths.GetTerrainChunkPath(state.Seed,x.Value.ToString()),
       //                 new TerrainChunkConverter()),
       //             _manifest)));

        var cells = _manifest.Terrains
            .Where(x => x.Key.Overlaps(rect));

        foreach (var cell in cells)
        {
            //Task.Run(() => { lock (_messages) { _messages.Enqueue("hi"); } });


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

                    { lock (_chunks) {
                            _chunks.Enqueue(new ChunkThreadData()
                            {
                                Values = map,
                                Guid = cell.Value,
                                Size = terrainChunk.ScaledBounds.size,
                                Position = new Vector3(
            terrainChunk.Bounds.x * terrainChunk.Multiplier,
            0,
            terrainChunk.Bounds.y * terrainChunk.Multiplier)
                    }); } 
                    }

                    
                }
                catch (Exception ex)
                {
                    { lock (_errors) { _errors.Enqueue(ex.Message); } }
                }




                //     U);
                // })
                //     //.ContinueWith(x => x.Result.GetResizedHeightmap(1025, 0, 1))
                //     .ContinueWith(x => { lock (_messages) { _messages.Enqueue($"Returned {x.Result} at {Time.realtimeSinceStartup} seconds. {x.Id}"); } });
            });//.ContinueWith(x => x.Result.GetResizedHeightmap(1025, 0, 1))
                    //.ContinueWith(x => { lock (_messages) { _messages.Enqueue($"Returned {x.Result} at  seconds. {x.Id}"); } });
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
        }
            
        
    }

    private class ChunkThreadData
    {
        public float[,] Values;
        public Vector2 Size;
        public Guid Guid;
        public Vector3 Position;
    }
}