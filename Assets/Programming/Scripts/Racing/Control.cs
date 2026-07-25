using TMPro;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Control : MonoBehaviour
{
    private Rigidbody rb;
    private InputAction inputMove;
    private InputAction inputEBrake;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        inputMove = InputSystem.actions.FindAction("Move");
        inputEBrake = InputSystem.actions.FindAction("EBrake");
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

        Vector3 moveCommand = inputMove.ReadValue<Vector2>();
        moveCommand.z = inputEBrake.ReadValue<float>();
        // moveCommand: x::turn, y::accl, z::(e)brake

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
