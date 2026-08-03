using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct PlayerTransformPublishSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (
            (RefRO<PlayerInput> input, RefRW<LocalTransform> ghostTransform) in 
            SystemAPI.Query<RefRO<PlayerInput>, RefRW<LocalTransform>>().WithAll<GhostOwnerIsLocal>())
        {
            ghostTransform.ValueRW.Position = input.ValueRO.commandedPos;
            ghostTransform.ValueRW.Rotation = input.ValueRO.commandedRot;
        }
    }
}