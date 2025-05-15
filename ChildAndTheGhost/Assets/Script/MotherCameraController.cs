using UnityEngine;

public class MotherCameraController : MonoBehaviour
{
    public Transform mother;
    public Transform boy;
    public float distance = 3f;
    public float height = 1.5f;
    public float mouseSensitivity = 100f;
    public float smoothSpeed = 0.12f;
    public Vector2 pitchLimits = new Vector2(-30f, 45f); // Look up/down range

    private float yaw = 0f;
    private float pitch = 10f;
    private Vector3 currentVelocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        if (!mother || !boy) return;

        // Get mouse movement
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);

        // Calculate direction
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 direction = rotation * Vector3.back;

        // Calculate target position
        Vector3 targetPosition = boy.position + direction * distance + Vector3.up * height;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothSpeed);

        transform.LookAt(boy.position + Vector3.up * 1f);
    }
}
