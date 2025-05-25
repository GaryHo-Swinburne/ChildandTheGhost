using UnityEngine;

public class MotherCamera : MonoBehaviour
{
    [Header("View Settings")]
    public Transform mother;
    public Vector3 offset = new Vector3(0f, 1.6f, 0.1f);
    public float mouseSensitivity = 100f;
    public Vector2 pitchLimits = new Vector2(-30f, 30f);

    private float yaw;
    private float pitch;

    [Header("Drift Settings")]
    public float driftCheckInterval = 90f;
    public float driftSpeed = 1.5f;
    public Transform[] distractionTargets;
    public Transform boy;

    private float nextDriftCheckTime;
    private bool isDrifting = false;
    private Quaternion driftTargetRotation;

    private bool isPlayerControllingCamera = false;

    void Start()
    {
        transform.position = mother.position + mother.TransformDirection(offset);
        Vector3 angles = transform.localEulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        ResetDriftTimer();
    }

    void Update()
    {
        // Always stay at mother's position
        transform.position = mother.position + mother.TransformDirection(offset);

        // Trigger distraction drift (only if idle)
        if (Time.time >= nextDriftCheckTime && !isDrifting && !isPlayerControllingCamera)
        {
            TryStartDrift();
        }

        // Perform drift movement (until it finishes)
        if (isDrifting && !isPlayerControllingCamera)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, driftTargetRotation, Time.deltaTime * driftSpeed);

            if (Quaternion.Angle(transform.rotation, driftTargetRotation) < 1f)
            {
                StopDrift(); // Ends drift and resets timer
            }

            return;
        }

        // Handle player camera input
        if (!isPlayerControllingCamera) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    public void EnablePlayerCameraControl(bool canControl)
    {
        isPlayerControllingCamera = canControl;

        if (canControl)
        {
            isDrifting = false;
            transform.position = mother.position + mother.TransformDirection(offset);

            // Calculate yaw/pitch from current view to avoid snap
            Vector3 forward = transform.forward;
            yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            pitch = -Mathf.Asin(forward.y) * Mathf.Rad2Deg;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            ResetDriftTimer();
            Debug.Log("[Control] Player took over mother camera.");
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    bool TryStartDrift()
    {
        if (distractionTargets == null || distractionTargets.Length == 0) return false;
        if (Random.value > 0.5f) return false;

        Transform target = distractionTargets[Random.Range(0, distractionTargets.Length)];
        Vector3 dir = target.position - transform.position;
        driftTargetRotation = Quaternion.LookRotation(dir);
        isDrifting = true;

        Debug.Log("[Drift] Mother camera is drifting toward: " + target.name);
        return true;
    }

    public void StopDrift()
    {
        isDrifting = false;
        ResetDriftTimer();
        Debug.Log("[Drift] Drift ended and timer reset.");
    }

    public void ResetDriftTimer()
    {
        nextDriftCheckTime = Time.time + driftCheckInterval;
        Debug.Log("[Drift] Drift timer reset.");
    }
}
