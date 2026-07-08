using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TMP_Text speedText;
    public TMP_Text fastestLapText;
    public List<TMP_Text> lapTimesTexts;
    public GameObject currentLapTimePanel;
    private TMP_Text laptimeText;
    private GameObject checkpointFlagsParent;
    public GameObject checkpointFlagsPrefab;
    public float spaceBetweenFlags;
    
    private List<GameObject> checkpointFlags = new();
    private Rigidbody playerCarRB;
    private const float mps_to_kmph = 3.6F;
    public bool startComplete { get; private set; } = false;

    private string LaptimeFloatToString(float lapTime)
    {
        string str = "";
        bool showMinutes = lapTime > 60;
        if (showMinutes)
        {
            str += Mathf.FloorToInt(lapTime / 60).ToString() + ":";
        }
        str += (lapTime % 60).ToString(showMinutes ? "00.000" : "0.000");
        return str;
    }

    void RaceManagerReliantStart()
    {
        int numCheckpoints = RaceManager.Instance.checkpoints.Count;
        float xForFlag0 = -spaceBetweenFlags * (numCheckpoints-1) / 2F;
        for (int i = 0; i < numCheckpoints; i++)
        {
            GameObject go = Instantiate(checkpointFlagsPrefab, checkpointFlagsParent.transform);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(xForFlag0 + i*spaceBetweenFlags, 0);
            checkpointFlags.Add(go);
        }
    }

    void Start()
    {
        playerCarRB = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
        fastestLapText.text = "No Laps Set";
        for (int i = 0; i < lapTimesTexts.Count; i++)
        {
            lapTimesTexts[i].text = "Lap not set";
        }
        laptimeText = currentLapTimePanel.transform.Find("Laptime").GetComponent<TMP_Text>();
        checkpointFlagsParent = currentLapTimePanel.transform.Find("Checkpoints").gameObject;
        if (!RaceManager.Instance.startComplete) return;
        RaceManagerReliantStart();
        startComplete = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!startComplete) { RaceManagerReliantStart(); startComplete = true; }

        for (int i = 0; i < RaceManager.Instance.checkpoints.Count; i++)
        {
            TurnTrackerScript checkpoint = RaceManager.Instance.checkpoints[i];
            bool crossedCheckpoint = RaceManager.Instance.checkpointsCrossed.Contains(checkpoint);
            checkpointFlags[i].GetComponent<RawImage>().color = crossedCheckpoint ?
                Color.green : (checkpoint.isDNF ? Color.red : Color.yellow);
        }

        laptimeText.text = LaptimeFloatToString(Time.time - RaceManager.Instance.lastTimeToPassStart);

        Vector3 localVelocity = playerCarRB.transform.worldToLocalMatrix * playerCarRB.linearVelocity;
        speedText.text = Math.Round(localVelocity.z * mps_to_kmph, 1).ToString() + " km/h";

        fastestLapText.text = float.IsNaN(RaceManager.Instance.bestLapTime) ? 
            "No Laps Set" : LaptimeFloatToString(RaceManager.Instance.bestLapTime);
        for (int i=0; i < RaceManager.Instance.lapTimes.Count; i++)
        {
            lapTimesTexts[i].text = LaptimeFloatToString(RaceManager.Instance.lapTimes
                                    [RaceManager.Instance.lapTimes.Count - 1 - i]);
        }
    }
}
