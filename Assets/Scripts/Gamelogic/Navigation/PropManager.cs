using System;
using UnityEngine;
using System.Linq;
using WanderingRoad;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using WanderingRoad.Procgen.RecursiveHex;
using WanderingRoad.Procgen.RecursiveHex.Json;
using WanderingRoad.Random;

public class PropManager : MonoBehaviour
{
    public GameState State;

    private Dictionary<Rect, Guid> _manifest;
    private readonly Queue<Vector3[]> _chunks = new Queue<Vector3[]>();
    private readonly Queue<Exception> _errors = new Queue<Exception>();

    private void Awake()
    {
        State.OnSeedChanged += BuildProps;
        State.OnTerrainLoaded += UpdateCells;
    }

    private void BuildProps(GameState state)
    {
        _manifest = Util.DeserialiseFile<Dictionary<Rect, Guid>>(Paths.GetHexGroupManifestPath(State.Seed), new ManifestSerialiser());

    }

    void UpdateCells(GameState state)
    {
        var pos = Vector3.zero;
        var rect = new Rect(new Vector2(pos.x, pos.z) - Vector2.one * 2, Vector2.one * 4);

        rect.DrawRect(Color.red, 100f);

        //_manifest.ToList().ForEach(x => x.Key.DrawRect(Color.blue, 100f));

        var cells = _manifest
            .Where(x => x.Key.Overlaps(rect)).ToList();

        foreach (var cell in cells)
        {
            Task.Run(() =>
            {
                try
                {
                    var hexGroup =
                     Util.DeserialiseFile<HexGroup>(
                        Paths.GetHexGroupPath(state.Seed, cell.Value.ToString()),
                        new HexGroupConverter());

                    //var subDivide = hexGroup.Subdivide(3, x => x.Code);

                    var subs = hexGroup
                    //.Subdivide(3, x => x.Code)
                    .GetHexes().Where(x =>x.Payload.EdgeDistance>0.05f).Select(x => x.Index.Position3d).ToArray();

                    //{ lock (_errors) { _errors.Enqueue("should be loading a fuckin chunk"); } }

                    {
                        lock (_chunks)
                        {
                            _chunks.Enqueue(subs);
                        }
                    }


                }
                catch (Exception ex)
                {
                    { lock (_errors) { _errors.Enqueue(ex); } }
                }
            });
        }
        
        //Debug.Log($"{cells.Count} overlapping cells");
        
        cells.ForEach(x => x.Key.DrawRect(Color.blue, 100f));
    }

    void Update()
    {
        RNG.DateTimeInit();

        while (_errors.Count > 0)
            Debug.LogError(_errors.Dequeue());

        if (_chunks.Count > 0)
        {
            var color = RNG.NextColorBright();
            var amount = _chunks.Dequeue();

            Debug.Log($"Building {amount.Length} props");

            for (int i = 0; i < amount.Length; i++)
            {
                Physics.Raycast(amount[i] * 8 + Vector3.up*50, Vector3.down, out var hit);

                Debug.DrawRay(hit.point, Vector3.up * 5, color,100f);
            }
        }

    }
}