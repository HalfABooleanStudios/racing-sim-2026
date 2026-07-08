using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

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

    [Header("Car & Track Qualities")]
    public CarProfile carProfile = default;
    public GroundSpeedModifier asphaltModifier = default;
    public GroundSpeedModifier gravelModifier = default;

    public GroundSpeedModifier currentModifier {
        get => isOnTrack ? asphaltModifier : gravelModifier;
    }

    public float bestLapTime {get; private set;} = float.NaN;
    public List<float> lapTimes {get; private set;} = new();


    private List<TurnTrackerScript> checkpoints = new();
    private List<TurnTrackerScript> checkpointsCrossed = new();
    private float lastTimeToPassStart = float.NaN;
    private Transform playerCar;
    private Vector3 playerInitialPos;
    private Quaternion playerInitialRot;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCar = GameObject.FindGameObjectWithTag("Player").transform;
        playerCar.GetComponent<BoxCollider>().size = carProfile.size;
        playerInitialPos = playerCar.position;
        playerInitialRot = playerCar.rotation;

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
            if (!float.IsInfinity(lapTime)) {
                lapTimes.Add(lapTime);
                if (lapTimes.Count > 5) lapTimes.RemoveAt(0);
                if (float.IsNaN(bestLapTime) || lapTime < bestLapTime) bestLapTime = lapTime;
            }
            checkpointsCrossed.Clear();
        }
        if (turn.isStart) lastTimeToPassStart = turn.lastCrossedTime;
        checkpointsCrossed.Add(turn);
    }

    void Update()
    {
        // Reset logic
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            playerCar.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            playerCar.position = playerInitialPos;
            playerCar.rotation = playerInitialRot;
            checkpointsCrossed.Clear();
        }
        // Check if car is on ground
        RaycastHit[] hits = Physics.BoxCastAll(
            playerCar.position, carProfile.size/2, -playerCar.up,
            playerCar.rotation, carProfile.size.y);
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
