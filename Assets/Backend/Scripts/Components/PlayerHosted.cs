using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

public struct TagCar : IComponentData {}
public struct TagPlayerCar : IComponentData {}
public struct TagGhostCar : IComponentData {}

public struct TagCheckpoint : IComponentData {}
public struct TagTrack : IComponentData {}





// Any entity with TagCar will have a number of these CheckpointData elements
public struct CheckpointCarData : IBufferElementData
{
    public Entity checkpoint;
    public bool crossedThisLap;
    public float lastCrossedTime;
}

[InternalBufferCapacity(5)]
public struct LapTime : IBufferElementData
{ public float lapTime; }

public struct CarCrossedStartRecently : IComponentData
{ public float when; }

public struct CarCrossedFinishRecently : IComponentData
{ public float when; }



public struct CarState : IInputComponentData
{
    public bool isOnGround;
    public bool isOnTrack;
    public bool useSmartSteer;
    // x::turn, y::accl, z::(e)brake
    public float3 moveCommand;

    public static readonly CarState Default = new()
    {
        isOnGround = true,
        isOnTrack = true,
        moveCommand = float3.zero
    };
}