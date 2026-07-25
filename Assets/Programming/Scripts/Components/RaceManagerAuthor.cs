using UnityEngine;
using Unity.Entities;

public class RaceManagerAuthor : MonoBehaviour
{
    public GroundSpeedModifier asphaltModifier;
    public GroundSpeedModifier gravelModifier;
    public CarProfile carProfile;

    public class Baker : Baker<RaceManagerAuthor>
    {
        public override void Bake(RaceManagerAuthor author)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            RaceConfig raceConfig = new() {
                rmAuthor = author,
                asphaltModifier = author.asphaltModifier.Convert(),
                gravelModifier = author.gravelModifier.Convert(),
                carProfile = author.carProfile.Convert()
            };
            AddComponent(entity, raceConfig);
            AddComponent<TagRaceManager>(entity);
            AddBuffer<CheckpointRef>(entity);
        }
    }
}

public struct TagRaceManager : IComponentData {}

public struct RaceConfig : IComponentData
{
    public UnityObjectRef<RaceManagerAuthor> rmAuthor;
    public GroundSpeedModifierComponent asphaltModifier;
    public GroundSpeedModifierComponent gravelModifier;
    public CarProfileComponent carProfile;
}

public struct CheckpointRef : IBufferElementData
{ public Entity checkpoint; }