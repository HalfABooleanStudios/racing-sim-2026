using UnityEngine;
using System.Collections.Generic;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    
    public bool isOnGround { get; private set; }
    public bool isOnTrack { get; private set; }

    [Header("Ground Qualities")]
    public GroundSpeedModifier asphaltModifier = default;
    public GroundSpeedModifier gravelModifier = default;

    public GroundSpeedModifier currentModifier {
        get => isOnTrack ? asphaltModifier : gravelModifier;
    }


    private List<TurnTrackerScript> checkpoints = new();
    private List<TurnTrackerScript> checkpointsCrossed = new();
    private float lastTimeToPassStart = float.NaN;
    private float bestLapTime = float.NaN;
    private Transform playerCar;
    public Vector3 carSize;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find checkpoints
        Transform checkpointsParent = GameObject.FindGameObjectWithTag("Track")
            .transform.parent.Find("Checkpoints");
        for(int i = 0; i < checkpointsParent.childCount; i++)
        {
            TurnTrackerScript turn;
            bool foundComponent =
                checkpointsParent.GetChild(i).TryGetComponent(out turn);
            if (foundComponent) checkpoints.Add(turn);
        }
        playerCar = GameObject.FindGameObjectWithTag("Player").transform;
        carSize = playerCar.GetComponent<BoxCollider>().size;
    }

    float CalculateLapTime(float timeToPassFinish)
    {   // Checks which checkpoints have been crossed and what penalties are due
        float penalties = 0F;
        int numCheckpoints = checkpoints.Count;
        bool[] seenCheckpoints = new bool[numCheckpoints];

        // First mark which all checkpoints are seen
        foreach (TurnTrackerScript checkpointCrossed in checkpointsCrossed)
        {
            int idx = checkpoints.IndexOf(checkpointCrossed);
            if (idx == -1) continue;
            seenCheckpoints[idx] = true;
        }

        // Second tally penalties for unseen checkpoints
        for (int i=0; i < numCheckpoints; i++)
        {
            if (seenCheckpoints[i]) continue;
            if (checkpoints[i].isDNF) return float.PositiveInfinity;
            penalties += checkpoints[i].tiemPenalty;
        }

        return timeToPassFinish - lastTimeToPassStart + penalties;
    }

    public void FlagPlayerCrossedTurn(TurnTrackerScript turn)
    {   // Called by TurnTrackerScript (OnTriggerEnter) when player crosses a checkpoint
        if (!checkpoints.Contains(turn)) return;
        if (turn.isFinish && !float.IsNaN(lastTimeToPassStart))
        {
            float lapTime = CalculateLapTime(turn.lastCrossedTime);
            if (float.IsInfinity(lapTime)) Debug.Log("DNF'd this lap");
            if (float.IsNaN(bestLapTime) || lapTime < bestLapTime)
            {
                bestLapTime = lapTime;
            }
            Debug.Log(lapTime);
            checkpointsCrossed.Clear();
        }
        if (turn.isStart) lastTimeToPassStart = turn.lastCrossedTime;
        checkpointsCrossed.Add(turn);
    }

    void Update()
    {
        RaycastHit[] hits = Physics.BoxCastAll(playerCar.position, carSize/2, -playerCar.up, playerCar.rotation, carSize.y);
        isOnGround = hits.Length != 0;
        isOnTrack = false;
        foreach (RaycastHit hit in hits) {
            if (hit.transform.tag == "Track")
            {
                isOnTrack = true;
                break;
            }
        }
    }
}
