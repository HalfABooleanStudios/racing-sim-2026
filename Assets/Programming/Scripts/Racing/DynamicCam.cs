using UnityEngine;

public class DynamicCam : MonoBehaviour
{
    public float referenceSpeedKmph = 360;
    public float maxZShift = 2;
    public float maxFovShift = 15;
    public float rotShiftMax = 15;

    private Rigidbody playerCarRB;
    private Transform camParent;
    private Camera thisCam;
    private const float mps_to_kmph = 3.6F;
    private float initialCamZ;
    private float initialCamFov;
    private float referenceSpeedMps
        { get => referenceSpeedKmph / mps_to_kmph; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCarRB = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
        camParent = transform.parent.GetComponent<Transform>();
        thisCam = GetComponent<Camera>();

        initialCamZ = transform.localPosition.z;
        initialCamFov = thisCam.fieldOfView;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 localPos = transform.localPosition;
        Vector3 playerLocalVelocity = playerCarRB.transform.worldToLocalMatrix
                                      * playerCarRB.linearVelocity;
        Vector3 playerLocalAngular = transform.worldToLocalMatrix
                                     * playerCarRB.angularVelocity;

        localPos.z = initialCamZ - (maxZShift * playerLocalVelocity.z / referenceSpeedMps);
        transform.localPosition = localPos;

        thisCam.fieldOfView = initialCamFov + (maxFovShift * playerLocalVelocity.z / referenceSpeedMps);
        
        camParent.localRotation = 
            Quaternion.Slerp(camParent.localRotation, 
                             Quaternion.Euler(0, rotShiftMax * playerLocalAngular.y / (0.5F * Mathf.PI), 0), 
                             Time.deltaTime);

    }
}
