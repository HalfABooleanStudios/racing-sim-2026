using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CamControl : MonoBehaviour
{
    public List<GameObject> cameraParents;
    private int currentCamIndex = 0;

    void Start()
    {
        Camera[] allCameras =
            FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (Camera camera in allCameras)
        {
            GameObject cameraParent = camera.transform.parent.gameObject;
            if (cameraParent.tag != "CamParent") continue;
            cameraParent.SetActive(false);
            cameraParents.Add(cameraParent);
        }
        cameraParents[currentCamIndex].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            cameraParents[currentCamIndex].SetActive(false);
            currentCamIndex = (currentCamIndex+1) % cameraParents.Count;
            cameraParents[currentCamIndex].SetActive(true);
        }
    }
}
