using UnityEngine;

public class MotherPOVCamera : MonoBehaviour
{
    [Header("References")]
    public Transform boy;                // Target to potentially look at
    public Transform[] distractionTargets; // List of points of interest

    [Header("Camera Settings")]
    //public Vector3 cameraOffset = new Vector3(0f, 1.6f, 0.1f); // Offset from mother's position
    public float mouseSensitivity = 100f;
    public Vector2 pitchLimits = new Vector2(-30f, 30f);

    [Header("Drift Settings")]
    public float driftDelayMin = 10f;
    public float driftDelayMax = 20f;
    public float driftSpeed = 1.5f;
    private bool isDrifting = false;

    private float yaw;
    private float pitch;
    private float nextDriftTime;
    private Quaternion driftTargetRotation;

    public bool CanUseInput = false;

    void Start()
    {
        Vector3 initialRot = transform.localEulerAngles;
        yaw = initialRot.y;
        pitch = initialRot.x;
        CanUseInput = true;

        ScheduleNextDrift();
    }

    void Update()
    {
        if (CanUseInput)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            isDrifting = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (Time.time >= nextDriftTime && !isDrifting)
            {
                StartDrift();
            }

            if (isDrifting)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, driftTargetRotation, Time.deltaTime * driftSpeed);
            }
        }
    }

    void ScheduleNextDrift()
    {
        nextDriftTime = Time.time + Random.Range(driftDelayMin, driftDelayMax);
    }

    void StartDrift()
    {
        if (distractionTargets == null || distractionTargets.Length == 0) return;

        Transform target = distractionTargets[Random.Range(0, distractionTargets.Length)];
        Vector3 direction = target.position - transform.position;
        driftTargetRotation = Quaternion.LookRotation(direction);
        isDrifting = true;

        ScheduleNextDrift();
    }

    public bool IsLookingAtBoy()
    {
        Vector3 toBoy = (boy.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, toBoy);
        return angle < 15f;
    }
}
