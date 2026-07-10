using System;
using System.Collections.Generic;
using UnityEngine;


public class DynamicCam : MonoBehaviour
{
    public float camShiftMax = 2;
    public float fovShiftMax = 15;
    public float rotShiftMax = 15;

    private float camShift;
    private Rigidbody playerCarRB;
    private Transform camParent;
    private const float mps_to_kmph = 3.6F;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCarRB = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
        camParent = GameObject.FindGameObjectWithTag("CamParent").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 localVelocity = playerCarRB.transform.worldToLocalMatrix * playerCarRB.linearVelocity;
        Vector3 localAngular = transform.worldToLocalMatrix * playerCarRB.angularVelocity;
        camShift = (float)(-4.5 - (camShiftMax * localVelocity.z * mps_to_kmph / 360));
        Vector3 pos = transform.localPosition;
        pos.z = camShift;
        transform.localPosition = pos;

        Camera cam = GetComponent<Camera>();
        cam.fieldOfView = 60 + (fovShiftMax * localVelocity.z * mps_to_kmph / 360);
        camParent.localRotation = 
            Quaternion.Slerp(camParent.localRotation, 
                             Quaternion.Euler(0, rotShiftMax * localAngular.y / (0.5F * Mathf.PI), 0), 
                             Time.deltaTime);

    }
}
