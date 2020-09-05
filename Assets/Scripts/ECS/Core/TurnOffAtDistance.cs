using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class TurnOffAtDistance : ComponentSystem
{
    public Vector3 Pos;
    public float Distance;

    private Vector3 _storedPos;
    private float _storedDistance;
    private CollisionWorld _collisionWorld;

    protected override void OnCreate()
    {
        base.OnCreate();

        _collisionWorld = World.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld.CollisionWorld;
    }

    protected override void OnUpdate()
    {
        if (_storedDistance == Distance && _storedPos == Pos)
            return;

        _storedPos = Pos;
        _storedDistance = Distance;


        var pos = new float3(Pos);

        //var uniques = new List<GroupData>();

        //EntityManager.GetAllUniqueSharedComponentData(uniques);

        //Debug.Log($"Uniques = {uniques.Count}");

        

        //for (int i = 0; i < uniques.Count; i++)
        //{
        //    var x = uniques[i];
        //
        //    var dist = Vector3.Distance(pos, x.Position);
        //
        //    //Entities.Where
        //
        //    //Debug.Log($"Dist = {dist}");
        //
        //    var query = EntityManager.CreateEntityQuery(typeof(GroupData));
        //
        //
        //    query.SetSharedComponentFilter(x);
        //
        //
        //    Entities.With(query)
        //        .ForEach(y =>
        //        {
        //            if (dist > Distance)
        //            {
        //                EntityManager.AddComponent<FrozenRenderSceneTag>(y);
        //            }
        //            else
        //            {
        //                EntityManager.RemoveComponent<FrozenRenderSceneTag>(y);
        //            }
        //        });
        //
        //
        //
        //        //x.Distance = dist;
        //    };

        Entities.WithAll(typeof(DistanceFromGuyMarkup), typeof(Unity.Transforms.Translation))
        
        
            .ForEach(x =>
        {
            var innerPos = EntityManager.GetComponentData<Translation>(x).Value;

            var distance = Vector3.Distance(pos, innerPos);

            if (distance > Distance)
            {
                EntityManager.AddComponent<FrozenRenderSceneTag>(x);
            }
            else
            {
                EntityManager.RemoveComponent<FrozenRenderSceneTag>(x);
            }
        
        
        });
    }

    float3 adjustForGroundLevel(CollisionWorld collisionWorld, float3 poss)
    {
        float3 aimDir = new float3(0f, -1f, 0f);
        float beamDistance = 100f;


        var staticGeomFilter = CollisionFilter.Default;
        staticGeomFilter.CollidesWith = 1 << 0;

        var closestHit = new Unity.Physics.RaycastHit();
        RaycastInput castInput = new RaycastInput
        {
            Start = poss,
            End = poss + aimDir * beamDistance,
            Filter = CollisionFilter.Default
        };


        if (collisionWorld.CastRay(castInput, out closestHit))
        {
            //Dose not hit here unit a few hundred loops
            return closestHit.Position;
        }

        return poss;
    }
}
