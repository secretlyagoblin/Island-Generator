using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

public class Spawner : MonoBehaviour
{
    public int X;
    public int Y;

    public GameObject Obj;


    public Transform Transform;

    public float Distance;



    private TurnOffAtDistance _turner;

    // Start is called before the first frame update
    void Start()
    {
        

        var turner = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<TurnOffAtDistance>();

        


        turner.Distance = Distance;
        turner.Pos = Transform.position;

        _turner = turner;

        var count = 0;
        var chunkCouny = 0;
        var fullCount = 0;
        var cachedPos = new float3();

        //var entity = new Entity();
        //entity.Ad

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        entityManager.Instantiate(Obj);

        var sphere = entityManager.CreateEntityQuery(typeof(PrefabEntity)).GetSingleton<PrefabEntity>().SpherePrefab;
        entityManager.AddComponent<GroupData>(sphere);
        entityManager.AddComponent<DistanceFromGuyMarkup>(sphere);

        //var fab = mananger.GetComponentData<PrefabEntityComponent>(PrefabEntity).prefabEntity;







        entityManager.Instantiate(sphere);

        //return;

        //mananger.Conv

        var entities = new NativeArray<Entity>(X * Y, Allocator.Temp);

        //entityManager.

        entityManager.Instantiate(sphere, entities);

        for (int x = 0; x < X; x++)
        {
            for (int y = 0; y < Y; y++)
            {
                entityManager.SetComponentData(entities[fullCount],new Translation() { Value = new float3(x, 0, y) });
                entityManager.SetSharedComponentData(entities[fullCount], new GroupData()
                {
                        Id = chunkCouny,
                        Position = cachedPos                    
                }) ;
                
                //obj.transform.position = new Vector3(x, 0, y);

                count++;
                fullCount++;

                if (count > 30)
                {
                    count = 0;
                    chunkCouny++;
                    cachedPos = new float3(x, 0, y);
                }
            };
        }

        entities.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        _turner.Distance = Distance;
        _turner.Pos = Transform.position;
    }
}
