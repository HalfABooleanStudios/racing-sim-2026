using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;
using Unity.Mathematics;

public class Control : MonoBehaviour
{
    private Rigidbody rb;
    public InputActionAsset controls;
    private InputAction inputMove;
    private InputAction inputEBrake;
    private Vector3 moveCommand;

    private EntityManager entityManager;
    [SerializeField] private Entity ghostEntity;

    private CarProfile carProfile {
        get => RaceManager.Instance.carProfile;
    }
    private GroundSpeedModifier currentModifier {
        get => RaceManager.Instance.currentModifier;
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

    private void SetGhostEntity()
    {
        EntityQuery query = entityManager.CreateEntityQuery(typeof(GhostOwnerIsLocal), typeof(Simulate));
        NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
        if (entities.Length > 0) { Debug.Log(entities.Length); ghostEntity = entities[0]; }
        entities.Dispose();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        inputMove = controls.FindAction("Move");
        inputEBrake = controls.FindAction("EBrake");

        foreach (World world in World.All)
        {
            if (world.IsClient() && !world.IsThinClient())
            {
                entityManager = world.EntityManager;
                break;
            }
        }
    }

    void OnEnable()
    {
        if (controls != null) controls.Enable();
    }

    void Update()
    {
        if (entityManager.Exists(ghostEntity))
        {
            entityManager.SetComponentData(ghostEntity, new PlayerInput
            {
                commandedPos = transform.position,
                commandedRot = ((quaternion) transform.rotation).value
            });
        }
        else SetGhostEntity();

        moveCommand = inputMove.ReadValue<Vector2>();
        moveCommand.z = inputEBrake.ReadValue<float>();
        // moveCommand: x::turn, y::accl, z::(e)brake
    }

    void MoveFB(Vector3 moveCommand, ref Vector3 localVelocity, ref Vector3 localAccl)
    {
        if (moveCommand.z == 1)
        {
            // Do some fun particles if also trying to move with WASD
            if (Math.Abs(localVelocity.z) < 0.5) localVelocity.z = 0;
            else localAccl.z = GetAccl(-2) * Math.Sign(localVelocity.z);
        }
        else if (Math.Abs(moveCommand.y) > 0.2 && (
            (moveCommand.y < 0 && localVelocity.z > GetMaxSpeed(-1)) ||
            (moveCommand.y > 0 && localVelocity.z < GetMaxSpeed(1))
        ))
        {
            localAccl.z = ((moveCommand.y * localVelocity.z >= 0) ? GetAccl(1) : GetAccl(-1))
                * Math.Sign(localVelocity.z + float.Epsilon) * Math.Abs(moveCommand.y);
        } else
        {
            if (Math.Abs(localVelocity.z) < 0.5) localVelocity.z = 0;
            else localAccl.z = GetAccl(0) * Math.Sign(localVelocity.z);
        }
    }

    void Turn(Vector3 moveCommand, ref Vector3 localVelocity,
              ref Vector3 localAngular, ref Vector3 localAccl)
    {
        float turnRadius = RaceManager.Instance.useSmartSteer?
            Math.Max(localVelocity.z * localVelocity.z / GetTurnAccl(), carProfile.turnRadiusV0)
            : carProfile.turnRadiusV0;
        localAngular.y = moveCommand.x * localVelocity.z / turnRadius;

        if (Math.Abs(localVelocity.x) < 0.5) localVelocity.x = 0;
        else localAccl.x = -GetTurnAccl() * Math.Sign(localVelocity.x);
    }

    void FixedUpdate()
    {
        if (!RaceManager.Instance.isOnGround) return;

        Vector3 localVelocity = transform.worldToLocalMatrix * rb.linearVelocity;
        Vector3 localAngular = transform.worldToLocalMatrix * rb.angularVelocity;

        Vector3 localAccl = Vector3.zero;

        MoveFB(moveCommand, ref localVelocity, ref localAccl);
        Turn(moveCommand, ref localVelocity, ref localAngular, ref localAccl);

        rb.angularVelocity = transform.localToWorldMatrix * localAngular;
        rb.linearVelocity = transform.localToWorldMatrix * localVelocity;
        rb.AddForce(rb.mass * (transform.localToWorldMatrix * localAccl));
    }
}
