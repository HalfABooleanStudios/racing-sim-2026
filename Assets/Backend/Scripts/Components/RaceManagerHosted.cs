using Unity.Entities;

public struct TagRaceManager : IComponentData {}

public struct RaceConfig : IComponentData
{
    public GroundSpeedModifierComponent asphaltModifier;
    public GroundSpeedModifierComponent gravelModifier;
    public CarProfileComponent carProfile;
}

public struct CheckpointRef : IBufferElementData
{ public Entity checkpoint; }