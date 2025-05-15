using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Transform targetBody;              // The boy's body to rotate horizontally
    public float mouseSensitivity = 100f;
    public Vector2 pitchClamp = new Vector2(-45f, 45f); // Y-axis look limits

    private float yaw = 0f;
    private float pitch = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, pitchClamp.x, pitchClamp.y);

        // Rotate camera (this GameObject)
        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);

        // Rotate player body horizontally (yaw only)
        if (targetBody != null)
        {
            targetBody.rotation = Quaternion.Euler(0f, yaw, 0f);
        }
    }
}
