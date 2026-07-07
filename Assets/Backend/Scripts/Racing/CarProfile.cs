using UnityEngine;

[CreateAssetMenu(fileName = "CarProfile", menuName = "Scriptable Objects/CarProfile")]
public class CarProfile : ScriptableObject
{
    public Vector3 size;

    [Header("Accelerations")]
    public float accl;
    public float deccl;
    public float decclIdle;
    public float decclBrake;

    [Header("Speeds")]
    public float maxSpeedPos;
    public float maxSpeedNeg;

    [Header("Turning")]
    [Tooltip("in m; least turning radius, usually L * cot(theta) + 1m")]
    public float turnRadiusV0 = 0F;
    [Tooltip("in m/s^2; equal to mu * g")]
    public float turnAcclByFriction = 9.81F;
}
