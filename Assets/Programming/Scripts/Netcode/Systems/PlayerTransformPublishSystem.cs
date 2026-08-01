using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct PlayerTransformPublishSystem : ISystem
{
    Entity playerEntity;

    void FindPlayerGORefEntity()
    {
        SystemAPI.TryGetSingletonEntity<PlayerGORef>(out playerEntity);
    }

    public void OnCreate(ref SystemState state)
    {
        FindPlayerGORefEntity();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!state.EntityManager.Exists(playerEntity)) {FindPlayerGORefEntity(); return; };

        LocalTransform playerTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);

        foreach (
            (RefRO<GhostOwnerIsLocal> tag, RefRW<LocalTransform> ghostTransform) in 
            SystemAPI.Query<RefRO<GhostOwnerIsLocal>, RefRW<LocalTransform>>())
        {
            ghostTransform.ValueRW.Position = playerTransform.Position;
            ghostTransform.ValueRW.Rotation = playerTransform.Rotation;
        }
    }
}