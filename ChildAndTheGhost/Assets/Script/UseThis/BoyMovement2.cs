using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class BoyMovement2 : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float jumpHeight = 1.2f;
    public float gravity = -20f;

    [Header("Debug")]
    public float interactionRayLength = 2f;
    public Vector3 interactionRayOffset = new Vector3(0f, 1.2f, 0f);

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Input
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(h, 0, v).normalized;

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Movement
        Vector3 move = inputDir;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Face movement direction
        if (move != Vector3.zero)
        {
            transform.forward = move;
            // Smooth version (optional):
            // transform.forward = Vector3.Slerp(transform.forward, move, Time.deltaTime * 10f);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position + interactionRayOffset;
        Vector3 direction = transform.forward;
        Gizmos.DrawRay(origin, direction * interactionRayLength);
    }
}
