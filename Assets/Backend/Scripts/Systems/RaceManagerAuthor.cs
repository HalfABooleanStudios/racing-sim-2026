using UnityEngine;
using Unity.Entities;
using Unity.Collections;

class RaceManagerAuthor : MonoBehaviour
{
    public GroundSpeedModifier asphaltModifier;
    public GroundSpeedModifier gravelModifier;
    public CarProfile carProfile;

    public class Baker : Baker<RaceManagerAuthor>
    {
        public override void Bake(RaceManagerAuthor author)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            RaceConfig raceConfig = new RaceConfig{
                asphaltModifier = author.asphaltModifier.Convert(),
                gravelModifier = author.gravelModifier.Convert(),
                carProfile = author.carProfile.Convert()
            };
            AddComponent(entity, raceConfig);
            AddBuffer<CheckpointRef>(entity);
        }
    }
}