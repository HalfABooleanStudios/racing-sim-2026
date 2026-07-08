using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TMP_Text fastestLapText;
    public List<TMP_Text> lapTimesTexts = new();

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
        fastestLapText.text = "No Laps Set";
        for (int i = 0; i < lapTimesTexts.Count; i++)
        {
            lapTimesTexts[i].text = "Lap not set";
        }
    }

    // Update is called once per frame
    void Update()
    {
        fastestLapText.text = float.IsNaN(RaceManager.Instance.bestLapTime) ? 
            "No Laps Set" : floatToString(RaceManager.Instance.bestLapTime);
        for (int i=0; i < RaceManager.Instance.lapTimes.Count; i++)
        {
            lapTimesTexts[i].text = floatToString(RaceManager.Instance.lapTimes
                                    [RaceManager.Instance.lapTimes.Count - 1 - i]);
        }
    }
}
