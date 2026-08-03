using UnityEngine;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ServerEnterGameSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new(Unity.Collections.Allocator.Temp);
        foreach ((RefRO<ReceiveRpcCommandRequest> rpcRequest, Entity entity)
            in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>()
                .WithAll<GoInGameRPC>()
                .WithEntityAccess())
        {
            NetworkId networkId = SystemAPI.GetComponent<NetworkId>(rpcRequest.ValueRO.SourceConnection);

            Entity player = ecb.Instantiate(SystemAPI.GetSingleton<PlayerCarGhost>().prefab);
            ecb.AddComponent(player, new GhostOwner() { NetworkId = networkId.Value });
            
            Debug.Log("Client connected to server");
            ecb.AddComponent<NetworkStreamInGame>(rpcRequest.ValueRO.SourceConnection);
            ecb.DestroyEntity(entity);

            ecb.AppendToBuffer(rpcRequest.ValueRO.SourceConnection,
                new LinkedEntityGroup {Value = player});
        }
        ecb.Playback(state.EntityManager);
    }
}