using UnityEngine;

public class TurnTrackerScript : MonoBehaviour
{
    public bool isStart = false;
    public bool isFinish = false;

    public float lastCrossedTime = float.NaN;

    [Header("Penalties")]
    public bool isDNF = false;
    public float tiemPenalty = 5F;

    private RaceManager raceManager;

    void Start()
    {
        raceManager = RaceManager.Instance;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Player") return;
        lastCrossedTime = Time.time;
        raceManager.FlagPlayerCrossedTurn(this);
    }
}
