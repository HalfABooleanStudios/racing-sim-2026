using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.NetCode;

public class PlayerPositonerAuthoring : MonoBehaviour
{
    public class Baker : Baker<PlayerPositonerAuthoring>
    {
        public override void Bake(PlayerPositonerAuthoring author)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<PlayerInput>(entity);
        }
    }
}

public struct PlayerInput : IInputComponentData
{
    // One day we shall use moveCommand directly; but i have not the patience for that
    public float3 commandedPos;
    public float4 commandedRot;
}