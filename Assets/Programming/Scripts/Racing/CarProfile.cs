using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
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

    public GameObject prefab;

    public CarProfileComponent Convert()
    {
        return new() {
            size = size,
            accl = accl,
            deccl = deccl,
            decclIdle = decclIdle,
            decclBrake = decclBrake,
            maxSpeedPos = maxSpeedPos,
            maxSpeedNeg = maxSpeedNeg,
            turnRadiusV0 = turnRadiusV0,
            turnAcclByFriction = turnAcclByFriction,
        };
    }
}

public struct CarProfileComponent : IComponentData
{
    public float3 size;

    public float accl;
    public float deccl;
    public float decclIdle;
    public float decclBrake;

    public float maxSpeedPos;
    public float maxSpeedNeg;

    public float turnRadiusV0;
    public float turnAcclByFriction;
}
