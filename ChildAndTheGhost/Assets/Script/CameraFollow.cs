using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Parameters")]

    [Tooltip("Gameobject you want the camera to follow")]
        public Transform target = null;

    [SerializeField, Range(0.01f, 1f), Tooltip("How fast the camera follows the object")]
        private float smoothSpeed = 0.125f;

    [SerializeField, Tooltip("Camera offset from the transform target")]
        private Vector3 offset = new Vector3 (0f, 2.25f, -1.5f);

    private Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        Vector3 desiredPositon = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPositon,ref velocity, smoothSpeed);
    }

    public void CenterOnTarget()
    {
        transform.position = target.position + offset;
    }
}
