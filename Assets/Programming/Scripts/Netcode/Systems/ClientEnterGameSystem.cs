using Unity.Entities;
using Unity.NetCode;

public partial struct ClientEnterGameSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new(Unity.Collections.Allocator.Temp);
        foreach ((RefRO<NetworkId> netID, Entity entity)
            in SystemAPI.Query<RefRO<NetworkId>>()
                .WithNone<NetworkStreamInGame>()
                .WithEntityAccess())
        {
            ecb.AddComponent<NetworkStreamInGame>(entity);

            Entity rpc; // TODO: this
        }
        ecb.Playback(state.EntityManager);
    }
}

public struct GoInGameRPC : IRpcCommand {}