using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class PlayerCarControl : MonoBehaviour
{
    private EntityManager entityManager;
    private RaceConfig raceConfig;
    private EntityQuery query;

    private Dictionary<Entity, GameObject> entityObjectMap;

    void LinkEntitiesWithObjects()
    {
        query = new EntityQueryBuilder()
            .WithAll<TagCar>()
            .WithNone<TagHasLinkedGO>()
            .Build(entityManager);
        
        NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
        for (int i = 0; i < entities.Length; i++)
        {
            Entity e = entities[i];
            if (entityObjectMap.ContainsKey(e)) Destroy(entityObjectMap[e]);
            GameObject go = raceConfig.rmAuthor.Value.carProfile.prefab;
            go = Instantiate(go, transform);
            entityObjectMap[e] = go;
        }
    }

    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        query = entityManager.CreateEntityQuery(typeof(RaceConfig));
        raceConfig = query.GetSingleton<RaceConfig>();

        LinkEntitiesWithObjects();
    }

    void Update()
    {
        foreach (Entity e in entityObjectMap.Keys)
        {
            GameObject go = entityObjectMap[e];
            if (!entityManager.Exists(e))
            {
                entityObjectMap.Remove(e);
                Destroy(go);
                continue;
            }
            LocalTransform lt = entityManager.GetComponentData<LocalTransform>(e);
            go.transform.position = lt.Position;
            go.transform.rotation = lt.Rotation;
        }
    }
}
