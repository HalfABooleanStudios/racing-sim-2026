using UnityEngine;

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
}
