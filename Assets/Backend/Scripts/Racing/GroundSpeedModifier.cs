using Unity.Entities;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "GroundSpeedModifier", menuName = "Scriptable Objects/GroundSpeedModifier")]
public class GroundSpeedModifier : ScriptableObject
{
    public float acclMul = 1F;
    public float decclMul = 1F;
    public float decclIdleMul = 1F;
    public float decclBrakeMul = 1F;

    public float maxSpeedPosMul = 1F;
    public float maxSpeedNegMul = 1F;

    public float turnAcclByFrictionMul = 1F;

    public GroundSpeedModifierComponent Convert()
    {
        return new()
        {
            acclMul = acclMul,
            decclMul = decclMul,
            decclIdleMul = decclIdleMul,
            decclBrakeMul = decclBrakeMul,
            maxSpeedPosMul = maxSpeedPosMul,
            maxSpeedNegMul = maxSpeedNegMul,
            turnAcclByFrictionMul = turnAcclByFrictionMul
        };
    }
}

public struct GroundSpeedModifierComponent : IComponentData
{
    public float acclMul;
    public float decclMul;
    public float decclIdleMul;
    public float decclBrakeMul;

    public float maxSpeedPosMul;
    public float maxSpeedNegMul;

    public float turnAcclByFrictionMul;
}
