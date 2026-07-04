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
    private List<TurnTrackerScript> checkpoints = new();
    private List<TurnTrackerScript> checkpointsCrossed = new();
    private float lastTimeToPassStart = float.NaN;
    private float bestLapTime = float.NaN;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find checkpoints
        Transform checkpointsParent = GameObject.FindGameObjectWithTag("Track")
            .transform.Find("Checkpoints");
        for(int i = 0; i < checkpointsParent.childCount; i++)
        {
            TurnTrackerScript turn;
            bool foundComponent =
                checkpointsParent.GetChild(i).TryGetComponent(out turn);
            if (foundComponent) checkpoints.Add(turn);
        }
    }

    float CalculateLapTime(float timeToPassFinish)
    {
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
    {
        if (!checkpoints.Contains(turn)) return;
        checkpointsCrossed.Add(turn);
        if (turn.isFinish && !float.IsNaN(lastTimeToPassStart))
        {
            float lapTime = CalculateLapTime(turn.lastCrossedTime);
            if (float.IsInfinity(lapTime)) Debug.Log("DNF'd this lap");
            if (float.IsNaN(bestLapTime) || lapTime < bestLapTime)
            {
                bestLapTime = lapTime;
            }
            Debug.Log(lapTime);
        }
        if (turn.isStart) lastTimeToPassStart = turn.lastCrossedTime;
    }
}
