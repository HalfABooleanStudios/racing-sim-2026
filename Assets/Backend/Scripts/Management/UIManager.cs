using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TMP_Text speedText;
    public TMP_Text fastestLapText;
    public List<TMP_Text> lapTimesTexts;

    private Rigidbody playerCarRB;
    private const float mps_to_kmph = 3.6F;


    private string floatToString(float lapTime)
    {
        string str = "";
        if (lapTime > 60)
        {
            str += Mathf.FloorToInt(lapTime / 60).ToString() + ":";
        }
        str += (lapTime % 60).ToString("0.000");
        return str;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCarRB = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
        fastestLapText.text = "No Laps Set";
        for (int i = 0; i < lapTimesTexts.Count; i++)
        {
            lapTimesTexts[i].text = "Lap not set";
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 localVelocity = playerCarRB.transform.worldToLocalMatrix * playerCarRB.linearVelocity;
        speedText.text = Math.Round(localVelocity.z * mps_to_kmph, 1).ToString() + " km/h";

        fastestLapText.text = float.IsNaN(RaceManager.Instance.bestLapTime) ? 
            "No Laps Set" : floatToString(RaceManager.Instance.bestLapTime);
        for (int i=0; i < RaceManager.Instance.lapTimes.Count; i++)
        {
            lapTimesTexts[i].text = floatToString(RaceManager.Instance.lapTimes
                                    [RaceManager.Instance.lapTimes.Count - 1 - i]);
        }
    }
}
