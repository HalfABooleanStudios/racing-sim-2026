using System;
using Unity.Mathematics;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;

public class Control : MonoBehaviour
{
    [Header("Motion")]
    public float accl;
    public float deccl;
    public float decclBrake;
    public float maxSpeedPos;
    public float maxSpeedNeg;

    [Header("Turning")]
    [Tooltip("in m; least turning radius, usually L * cot(theta) + 1m")]
    public float turnRadiusV0 = 0F;
    [Tooltip("in m/s^2; equal to mu * g")]
    public float turnAcclByFriction = 9.81F;

    private Rigidbody rb;
    private InputAction inputMove;
    private InputAction inputEBrake;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        turnRadiusV0 = Math.Abs(turnRadiusV0);
        turnAcclByFriction = Math.Abs(turnAcclByFriction);

        rb = gameObject.GetComponent<Rigidbody>();
        inputMove = InputSystem.actions.FindAction("Move");
        inputEBrake = InputSystem.actions.FindAction("EBrake");
    }

    bool TryMoveFB(int direction)
    {
        // int direction = 1 [accl], 0 [deccl], -1 [brake];
        float vfwd = Vector3.Dot(rb.linearVelocity, transform.forward);
        bool canMove = direction == -1 ?
            (Math.Abs(vfwd) > 0.01) :
            (direction == 0 ?
                vfwd > maxSpeedNeg :
                vfwd < maxSpeedPos
            );
        if (!canMove) return false;
        rb.AddForce(transform.forward *
            (direction == -1 ? Math.Sign(vfwd) * decclBrake : (direction == 0 ? deccl : accl)));
        return true;
    }

    void Turn(float direction)
    {
        float vfwd = Vector3.Dot(rb.linearVelocity, transform.forward);
        float vlr = Vector3.Dot(rb.linearVelocity - transform.forward * vfwd, transform.right);

        float turnRadius = Math.Max(vfwd * vfwd / turnAcclByFriction, turnRadiusV0);

        Vector3 omegaLocal = transform.worldToLocalMatrix * rb.angularVelocity;
        omegaLocal = new Vector3(omegaLocal.x, direction * vfwd / turnRadius, omegaLocal.z);

        rb.angularVelocity = transform.localToWorldMatrix * omegaLocal;
        if (Math.Abs(vlr) > 0.01)
            rb.AddForce(-transform.right * rb.mass * turnAcclByFriction * Math.Sign(vlr));
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveCommand = inputMove.ReadValue<Vector2>();
        float brakeCommand = inputEBrake.ReadValue<float>();

        if (brakeCommand == 1)
        {
            // Do some fun particles if also trying to move with WASD
            TryMoveFB(-1);
        }
        else if (moveCommand.y > 0.2) TryMoveFB(1);
        else if (moveCommand.y < -0.2) TryMoveFB(0);

        Turn(moveCommand.x);
    }
}
