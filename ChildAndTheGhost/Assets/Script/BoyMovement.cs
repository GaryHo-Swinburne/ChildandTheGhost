using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class BoyMovement : MonoBehaviour
{
    public float walkSpeed;
    public float runSpeed;
    public float jumpForce;
    public float gravity;

    private CharacterController controller;
    public Vector3 velocity;
    public bool isGrounded;

    private Animator animator;
    private float jumpTime = -1.0f;
    public float jumpStartWaitTime;
    public float jumpSlowTime;
    public float jumpPauseTime;
    public float jumpLandMovableTime;

    private BoyInteraction boyInteraction;

    public Transform nearlyJumpDownPoint;
    public Transform jumpDownPoint;
    public float jumpDownWaitTime;
    public float jumpDownFinishTime = -1.0f;
    public float groundLevel;
    public Vector3 jumpDownStartOffset;
    public Vector3 jumpDownFinishOffset;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        boyInteraction = GetComponent<BoyInteraction>();
    }

    void Update()
    {
        // Check ground
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -0.1f;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        // Input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Direction for movement
        Vector3 move = Vector3.zero;
        if (boyInteraction.isGrabbing)
        {
            move = transform.forward * z;
            if (Mathf.Abs(move.x) > Mathf.Abs(move.z))
                move = Vector3.right * move.x;
            else
                move = Vector3.forward * move.z;

            move.Normalize();
        }
        else
        {
            move = new Vector3(x, 0.0f, z);
            move.Normalize();
            if (move.magnitude > 0.0f) transform.rotation = Quaternion.LookRotation(move);
        }

        // Speed control
            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        // If in the air
        if (!isGrounded) speed *= 0.5f;
        // If about to jump
        if (isGrounded && state.IsName("Jumping") && jumpTime > 0.0f) speed *= 0.5f;
        // If landing from a jump
        if (isGrounded && state.IsName("Jumping") && jumpTime < 0.0f && state.normalizedTime < jumpLandMovableTime) speed = 0.0f;
        // If pushing or pulling an object
        if (boyInteraction.isGrabbing && Vector3.Dot(move, transform.forward) > 0.0f) speed *= 0.5f;
        if (boyInteraction.isGrabbing && Vector3.Dot(move, transform.forward) < 0.0f) speed *= 0.5f;
        // If jumping down
        if (jumpDownFinishTime > 0.0f && Time.time < jumpDownFinishTime) speed = 0.0f;

        // If finished jumping down, reset the jump down finish time
        if (jumpDownFinishTime > 0.0f && Time.time > jumpDownFinishTime)
        {
            jumpDownFinishTime = -1.0f;
            transform.position += transform.TransformDirection(jumpDownFinishOffset);
            controller.enabled = true;
        }
        // If jumping down, prevent all other movement
        if (jumpDownFinishTime > 0.0f) return;

        if (
            transform.position.y > groundLevel &&
            isGrounded &&
            !Physics.CheckSphere(nearlyJumpDownPoint.position, 0.1f) &&
            Vector3.Dot(move, transform.forward) > 0.0f
        )
        {
            // If the player is about to jump down, slow down
            speed *= 0.5f;
            // Jump down
            if (!Physics.CheckSphere(jumpDownPoint.position, 0.1f))
            {
                animator.SetTrigger("jumpDown");
                jumpDownFinishTime = Time.time + jumpDownWaitTime;
                controller.enabled = false;
                transform.position += transform.TransformDirection(jumpDownStartOffset);
                return;
            }
        }

        // Apply movement
        controller.Move(move * speed * Time.deltaTime);

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity);

        // Start jump animation when the player presses the jump button
        if (Input.GetButtonDown("Jump") && isGrounded && !state.IsName("Jumping"))
        {
            animator.SetTrigger("jump");
            jumpTime = Time.time + jumpStartWaitTime;
        }

        if (jumpTime > 0.0f && Time.time > jumpTime)
        {
            jumpTime = -1.0f;
            velocity.y = jumpForce;
        }

        if (state.IsName("Jumping"))
        {
            if (Time.time > jumpTime && isGrounded)
            {
                animator.speed = 1.0f;
            }
            else
            {
                if (state.normalizedTime > jumpSlowTime)
                {
                    animator.speed = 0.2f;
                }
                else if (state.normalizedTime > jumpPauseTime)
                {
                    animator.speed = 0.0f;
                }
            }
        }

        // Animation
        if (boyInteraction.isGrabbing)
        {
            if (move.magnitude == 0.0f)
            {
                animator.SetBool("isPushing", false);
                animator.SetBool("isPulling", true);
                animator.SetBool("isWalking", false);
                animator.speed = 0.0f;
            }
            else
            {
                animator.SetBool("isPushing", Vector3.Dot(move, transform.forward) > 0.0f);
                animator.SetBool("isPulling", Vector3.Dot(move, transform.forward) < 0.0f);
                animator.speed = 1.0f;
            }
        }
        else
        {
            animator.SetBool("isPushing", false);
            animator.SetBool("isPulling", false);
            animator.SetBool("isWalking", move.magnitude > 0.0f);
        }
    }
}
