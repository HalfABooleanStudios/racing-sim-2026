using UnityEngine;
using Unity.Entities;

class PlayerAuthor : MonoBehaviour
{
    public class Baker : Baker<PlayerAuthor>
    {
        public override void Bake(PlayerAuthor author)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<TagCar>(entity);
            AddComponent<TagPlayerCar>(entity);
            
            AddBuffer<CheckpointCarData>(entity);
            AddBuffer<LapTime>(entity);

            AddComponent(entity, CarState.Default);
        }
    }
}