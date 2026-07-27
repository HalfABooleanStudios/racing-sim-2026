using UnityEngine;
using Unity.Entities;

[DisallowMultipleComponent]
public class PlayerCarGhostAuthoring : MonoBehaviour
{
    public GameObject prefab;

    public class Baker : Baker<PlayerCarGhostAuthoring>
    {
        public override void Bake(PlayerCarGhostAuthoring author)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            Entity prefabEntity = GetEntity(author.prefab, TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerCarGhost() { prefab = prefabEntity });
        }
    }
}

public struct PlayerCarGhost : IComponentData
{
    public Entity prefab;
}