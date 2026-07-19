using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

public partial class ControlSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<RaceConfig>();
    }

    protected override void OnUpdate()
    {
        ControlSystemJob controlJob = new ControlSystemJob
        {
            config = SystemAPI.GetSingleton<RaceConfig>(),
            deltaTime = SystemAPI.Time.DeltaTime
        };
        controlJob.ScheduleParallel();
    }
}

public partial struct ControlSystemJob : IJobEntity
{
    public RaceConfig config;
    public float deltaTime;

    private CarProfileComponent carProfile;
    private GroundSpeedModifierComponent currentModifier;

    public void Execute(
        ref PhysicsVelocity velocity,
        in CarState carState,
        in LocalToWorld localToWorld)
    {
        UnityEngine.Debug.Log("Tried to move!");
        if (!carState.isOnGround) return;

        carProfile = config.carProfile;
        currentModifier = carState.isOnTrack ?
            config.asphaltModifier : config.gravelModifier;

        float3 localLinear =
            math.mul(math.inverse(localToWorld.Rotation), velocity.Linear);
        float3 localAngular =
            math.mul(math.inverse(localToWorld.Rotation), velocity.Angular);
        float3 localAccl = float3.zero;

        MoveFB(carState, ref localLinear, ref localAccl);
        Turn(carState, ref localLinear, ref localAngular, ref localAccl);

        velocity.Angular = math.mul(localToWorld.Rotation, localAngular);
        velocity.Linear = math.mul(localToWorld.Rotation, localLinear + localAccl * deltaTime);
    }



    private float GetMaxSpeed(int direction)
    {
        if (direction == 1) return carProfile.maxSpeedPos * currentModifier.maxSpeedPosMul;
        if (direction == -1) return -carProfile.maxSpeedNeg * currentModifier.maxSpeedNegMul;
        return 0;
    }

    private float GetAccl(int magnitude)
    {
        switch (magnitude)
        {
            case 1:
                return carProfile.accl * currentModifier.acclMul;
            case 0:
                return -carProfile.decclIdle * currentModifier.decclIdleMul;
            case -1:
                return -carProfile.deccl * currentModifier.decclMul;
            case -2:
                return -carProfile.decclBrake * currentModifier.decclBrakeMul;
            default:
                return 0;
        }
    }
    
    private float GetTurnAccl()
    {
        return carProfile.turnAcclByFriction * currentModifier.turnAcclByFrictionMul;
    }

    private void MoveFB(in CarState carState, ref float3 localVelocity, ref float3 localAccl)
    {
        float3 moveCommand = carState.moveCommand;

        if (moveCommand.z == 1)
        {
            // Do some fun particles if also trying to move with WASD
            if (math.abs(localVelocity.z) < 0.5) localVelocity.z = 0;
            else localAccl.z = GetAccl(-2) * math.sign(localVelocity.z);
        }
        else if (math.abs(moveCommand.y) > 0.2 && (
            (moveCommand.y < 0 && localVelocity.z > GetMaxSpeed(-1)) ||
            (moveCommand.y > 0 && localVelocity.z < GetMaxSpeed(1))
        ))
        {
            localAccl.z = ((moveCommand.y * localVelocity.z >= 0) ? GetAccl(1) : GetAccl(-1))
                * math.sign(localVelocity.z + float.Epsilon) * math.abs(moveCommand.y);
        } else
        {
            if (math.abs(localVelocity.z) < 0.5) localVelocity.z = 0;
            else localAccl.z = GetAccl(0) * math.sign(localVelocity.z);
        }
    }
    
    void Turn(CarState carState, ref float3 localVelocity,
              ref float3 localAngular, ref float3 localAccl)
    {
        float3 moveCommand = carState.moveCommand;

        float turnRadius = carState.useSmartSteer ?
            math.max(localVelocity.z * localVelocity.z / GetTurnAccl(), carProfile.turnRadiusV0)
            : carProfile.turnRadiusV0;
        localAngular.y = moveCommand.x * localVelocity.z / turnRadius;

        if (math.abs(localVelocity.x) < 0.5) localVelocity.x = 0;
        else localAccl.x = -GetTurnAccl() * math.sign(localVelocity.x);
    }

}
