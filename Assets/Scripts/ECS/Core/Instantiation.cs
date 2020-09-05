using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class Instantiation : ComponentSystem
{
    public Transform PlayerTransform;

    private float spawnTimer;

    private int stepx = 0;
    private int stepy = 0;

    protected override void OnUpdate()
    {
        spawnTimer -= Time.DeltaTime;

        //if(spawnTimer <= 0f)
        //{
            stepx++;

            if(stepx>200)
            {
                stepy++;
                stepx = 0;
            }

            //spawnTimer = .1f; ;

            Entities.ForEach((ref PrefabEntityComponent x) =>
            {
                var spawnedEntity = EntityManager.Instantiate(x.prefabEntity);
                EntityManager.SetComponentData(spawnedEntity, new Translation
                {
                    Value = new float3(stepx, stepy, 0)
                });
            });
        //}
    }
}