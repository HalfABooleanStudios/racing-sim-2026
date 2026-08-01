using UnityEngine;
using Unity.Entities;

public class PlayerGORefAuthoring : MonoBehaviour
{
    public class Baker : Baker<PlayerGORefAuthoring>
    {
        public override void Bake(PlayerGORefAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<PlayerGORef>(entity);
        }
    }
}

public struct PlayerGORef : IComponentData {}