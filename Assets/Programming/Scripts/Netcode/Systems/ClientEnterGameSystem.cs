using UnityEngine;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
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

            Debug.Log("Sending connect RPC");

            Entity rpc = ecb.CreateEntity();
            ecb.AddComponent<GoInGameRPC>(rpc);
            ecb.AddComponent<SendRpcCommandRequest>(rpc);
        }
        ecb.Playback(state.EntityManager);
    }
}

public struct GoInGameRPC : IRpcCommand {}