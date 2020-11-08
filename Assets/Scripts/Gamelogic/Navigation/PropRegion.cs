using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using WanderingRoad;
using WanderingRoad.Procgen.RecursiveHex;
using WanderingRoad.Procgen.RecursiveHex.Json;

public class PropRegion
{
    private Vector2 _center;
    private Rect _bounds;
    private Guid _guid;
    private GameState _state;
    private const int MULTIPLIER = 8;
    private PropRegionState _propState = PropRegionState.Unloaded;


    public PropRegion(GameState state, Rect bounds, Guid id)
    {
        _state = state;
        _bounds = bounds;
        _guid = id;

        _center = bounds.center * MULTIPLIER;

    }

    private enum PropRegionState
    {
        Unloaded = 0,
        Loading = 5,
        Loaded = 10,
        Displayed = 20
    }


    public void Update(Vector2 sample)
    {
        if (_propState == PropRegionState.Loading)
            return;

        var distance = Vector3.Distance(sample, _center);
        PropRegionState targetState;


        if (distance > 50)
        {
            switch (_propState)
            {
                case PropRegionState.Unloaded:
                    break;
                case PropRegionState.Loaded:
                    break;
                case PropRegionState.Displayed:
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (_propState)
            {
                case PropRegionState.Unloaded:
                    targetState = PropRegionState.Loading;
                    _runningTask = Task.Run(LoadFromFile);


                    break;
                case PropRegionState.Loaded:
                    break;
                case PropRegionState.Displayed:
                    break;
                default:
                    break;
            }
        }
    }

    private Task _runningTask;

    private void LoadFromFile()
    {
        var hexGroup = Util.DeserialiseFile<HexGroup>(
             Paths.GetHexGroupPath(_state.Seed, _guid.ToString()),
            new HexGroupConverter());

        //var subDivide = hexGroup.Subdivide(3, x => x.Code);

        var divisions = 1;

        var inverseMatrix = HexIndex.GetInverseMultiplicationMatrix(divisions);

        var subs = hexGroup
        .Subdivide(divisions, x => x.Code)
        .GetHexes()
        .Where(x => x.Payload.EdgeDistance > 0.5f && x.Payload.EdgeDistance < 3)
        //.Select(x=> x.Index.Position3d*8)
        .Select(x => inverseMatrix.MultiplyPoint(x.Index.Position3d) * MULTIPLIER)
        .Select(x => new float3(x))
        .Chunk(800)
        .ToList()

        ;

        //{ lock (_errors) { _errors.Enqueue("should be loading a fuckin chunk"); } }

        {
            lock (_chunks)
            {
                subs.ForEach(x => _chunks.Enqueue(x.ToArray()));
                //_chunks.Enqueue(subs);
            }
        }

        //                catch (Exception ex)
        //{
        //    { lock (_errors) { _errors.Enqueue(ex); } }
        //}
    }
}
