using TMPro;
using System;
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

    public TMP_Text speedText;
    private Rigidbody rb;
    private InputAction inputMove;
    private InputAction inputEBrake;

    private const float mps_to_kmph = 3.6F;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        turnRadiusV0 = Math.Abs(turnRadiusV0);
        turnAcclByFriction = Math.Abs(turnAcclByFriction);

        rb = gameObject.GetComponent<Rigidbody>();
        inputMove = InputSystem.actions.FindAction("Move");
        inputEBrake = InputSystem.actions.FindAction("EBrake");
    }

    void Update()
    {
        Vector3 localVelocity = transform.worldToLocalMatrix * rb.linearVelocity;
        speedText.text = Math.Round(localVelocity.z * mps_to_kmph, 1).ToString() + " km/h";
    }

    void FixedUpdate()
    {
        Vector2 moveCommand = inputMove.ReadValue<Vector2>();
        float brakeCommand = inputEBrake.ReadValue<float>();

        Vector3 localVelocity = transform.worldToLocalMatrix * rb.linearVelocity;
        Vector3 localAngular = transform.worldToLocalMatrix * rb.angularVelocity;

        Vector3 localAccl = Vector3.zero;

        if (brakeCommand == 1)
        {
            // Do some fun particles if also trying to move with WASD
            if (Math.Abs(localVelocity.z) < 0.5) localVelocity.z = 0;
            else localAccl.z = -decclBrake * Math.Sign(localVelocity.z);
        }
        else if (Math.Abs(moveCommand.y) > 0.2 && (
            (moveCommand.y < 0 && localVelocity.z > -maxSpeedNeg) ||
            (moveCommand.y > 0 && localVelocity.z < maxSpeedPos)
        ))
        {
            localAccl.z = ((moveCommand.y * localVelocity.z > 0) ? accl : -deccl) * Math.Sign(localVelocity.z);
        }

        float turnRadius = Math.Max(localVelocity.z * localVelocity.z / turnAcclByFriction, turnRadiusV0);
        localAngular.y = moveCommand.x * localVelocity.z / turnRadius;

        if (Math.Abs(localVelocity.x) < 0.5) localVelocity.x = 0;
        else localAccl.x = -turnAcclByFriction * Math.Sign(localVelocity.x);

        rb.angularVelocity = transform.localToWorldMatrix * localAngular;
        rb.linearVelocity = transform.localToWorldMatrix * localVelocity;
        rb.AddForce(rb.mass * (transform.localToWorldMatrix * localAccl));
    }
}
