using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct PlayerPosRotTData
{
    public float3 position;
    public float4 rotation;
    public float time;
}

public class GhostReadManager : MonoBehaviour
{
    public static GhostReadManager Instance { get; private set; }
    public bool startComplete { get; private set; } = false;
    public float captureRate = 20f;
    private float lastCapture = float.NaN;
    public List<PlayerPosRotTData> recording { get; private set; } = new();
    public bool isRecording { get; private set; } = false;


    private Transform playerCarTransform;

    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    void RaceManagerReliantStart()
    {
        playerCarTransform = RaceManager.Instance.playerCar;
    }

    void Start()
    {
        if (RaceManager.Instance.startComplete)
            RaceManagerReliantStart();
        else return;
        startComplete = true;
    }


    void Update()
    {
        if (!startComplete) { RaceManagerReliantStart(); startComplete = true; }
        if (isRecording &&
            (float.IsNaN(lastCapture) || (Time.time - lastCapture) * captureRate > 1))
        {
            recording.Add(new PlayerPosRotTData
            {
                position = playerCarTransform.position,
                rotation = ((quaternion)playerCarTransform.rotation).value,
                time = Time.time
            });
            lastCapture = Time.time;
        }
    }
}
