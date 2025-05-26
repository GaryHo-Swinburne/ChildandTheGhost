using UnityEngine;

public class Boy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float gravity;

    // Movement state
    private float verticalVelocity;

    // Jumping state
    [Header("Jump Settings")]
    [SerializeField] private float jumpStartWaitTime;
    [SerializeField] private float jumpLandWaitTime;
    [SerializeField] private float jumpSlowTime;
    [SerializeField] private float jumpPauseTime;
    private float jumpTime = -1.0f;
    private float finishLandingTime = -1.0f;
    private bool startingJump = false;
    private bool isJumping = false;

    // Jumping down state
    [Header("Jump Down Settings")]
    [SerializeField] private Vector3 jumpDownStartOffset;
    [SerializeField] private Vector3 jumpDownLandOffset;
    [SerializeField] private Vector3 jumpDownEndOffset;
    [SerializeField] private float jumpDownAnimationTime;
    [SerializeField] private float jumpDownLandWaitTime;
    private float jumpDownLandTime = -1.0f;
    private float jumpDownEndTime = -1.0f;
    private bool startingJumpDown = false;

    // Interaction settings
    [Header("Interaction Settings")]
    [SerializeField] private float interactRange;
    private Interactable heldObject;

    // References
    [Header("References")]
    [SerializeField] private Transform feetPos;
    [SerializeField] private Transform grabPoint;
    [SerializeField] private Transform eyePoint;
    private Animator animator;
    private CharacterController controller;

    public bool CanUseInput = true;

    public void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    public void Update()
    {
        Vector3 move = CheckInput();
        Vector3 velocity = MovePlayer(move);
        UpdateAnimations(velocity);
    }

    private bool IsInJump => jumpTime > 0.0f || isJumping || finishLandingTime > 0.0f;
    private bool IsGrounded => GetGroundDistance() < 0.1f;
    private bool FacingStraight => Mathf.Abs(transform.forward.x) < 0.01f || Mathf.Abs(transform.forward.z) < 0.01f;
    private bool AtLedge
    {
        get
        {
            // Check if the player is at a ledge
            RaycastHit hit;
            Vector3 ledgeCheckPos = feetPos.position + transform.TransformDirection(new Vector3(0.0f, 0.0f, 0.5f));

            return !Physics.Raycast(ledgeCheckPos, Vector3.down, out hit, 1.0f);
        }
    }

    private Vector3 CheckInput()
    {
        if (!CanUseInput) return Vector3.zero;

        bool canStartAction = IsGrounded & !IsInJump && !AtLedge;

        // Check if the player should jump
        if (Input.GetButtonDown("Jump") && canStartAction && heldObject == null)
        {
            jumpTime = Time.time + jumpStartWaitTime;
            startingJump = true;
            return Vector3.zero;
        }

        // Check if the player is pressing the grab button
        if (Input.GetKeyDown(KeyCode.E) && FacingStraight && canStartAction)
        {
            if (heldObject == null)
            {
                TryToGrabObject();
            }
            else
            {
                ReleaseObject();
            }
        }

        // Handle movement input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        return new Vector3(x, 0, z).normalized;
    }

    private void TryToGrabObject()
    {
        Ray ray = new Ray(eyePoint.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactRange))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null && interactable.canBeGrabbed)
            {
                heldObject = interactable;
                heldObject.transform.SetParent(grabPoint);
                heldObject.transform.localPosition = Vector3.zero;
            }
        }
    }

    private void ReleaseObject()
    {
        heldObject.transform.SetParent(null);
        heldObject = null;
    }

    private Vector3 MovePlayer(Vector3 move)
    {
        CheckJumpLanding();
        CheckJumpDownLanding();

        if (AtLedge && IsGrounded && !IsInJump)
        {
            jumpDownEndTime = Time.time + jumpDownAnimationTime;
            jumpDownLandTime = Time.time + jumpDownLandWaitTime;
            controller.enabled = false;
            transform.position += transform.TransformDirection(jumpDownStartOffset);
            startingJumpDown = true;
        }

        if (jumpDownEndTime > 0.0f) return Vector3.zero;

        // Update velocity based on input
        Vector3 velocity = move * walkSpeed;

        // Modify move speed
        velocity = ModifyMoveSpeed(velocity);

        // Apply jump movement
        if (jumpTime > 0.0f && Time.time >= jumpTime)
        {
            verticalVelocity = jumpForce;
            jumpTime = -1.0f;
            isJumping = true;
        }

        // Apply gravity
        verticalVelocity += gravity * Time.deltaTime;
        if (IsGrounded && verticalVelocity < 0)
            verticalVelocity = -1.0f;

        // Apply movement
        if (velocity.magnitude > 0.0f && heldObject == null) transform.forward = velocity.normalized;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        return velocity;
    }

    private void CheckJumpLanding()
    {
        // Check if the player is landing
        if (isJumping && verticalVelocity < 0.0f && IsGrounded)
        {
            isJumping = false;
            finishLandingTime = Time.time + jumpLandWaitTime;
        }

        // Check if the player is finished landing
        if (finishLandingTime > 0.0f && Time.time >= finishLandingTime) finishLandingTime = -1.0f;
    }

    private void CheckJumpDownLanding()
    {
        if (jumpDownLandTime > 0.0f && Time.time > jumpDownLandTime)
        {
            jumpDownLandTime = -1.0f;
            transform.position += transform.TransformDirection(jumpDownLandOffset);
        }
        else if (jumpDownEndTime > 0.0f && Time.time > jumpDownEndTime)
        {
            jumpDownEndTime = -1.0f;
            transform.position += transform.TransformDirection(jumpDownEndOffset);
            controller.enabled = true;
        }
    }

    private Vector3 ModifyMoveSpeed(Vector3 velocity)
    {
        // If about to jump, do not move horizontally
        if (jumpTime > 0.0f) velocity *= 0.0f;
        // If landing from a jump, do not move horizontally
        if (finishLandingTime > 0.0f && Time.time < finishLandingTime) velocity *= 0.0f;

        // Restrict movement to straight or backwards if holding an object
        if (heldObject != null)
        {
            if (Mathf.Abs(Vector3.Dot(transform.forward, Vector3.forward)) > 0.99f)
                velocity.x = 0.0f;
            else
                velocity.z = 0.0f;
        }

        return velocity;
    }

    private void UpdateAnimations(Vector3 velocity)
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        if (startingJumpDown)
        {
            animator.SetTrigger("jumpDown");
            startingJumpDown = false;
        }

        bool isWalking = IsGrounded && (velocity.x != 0.0f || velocity.z != 0.0f) && !IsInJump && !AtLedge && heldObject == null;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0.0f, velocity.z).normalized;
        bool isPushing = heldObject != null;
        bool isPulling = heldObject != null && Vector3.Dot(horizontalVelocity, -transform.forward) > 0.9f;
        if (isPulling) isPushing = false;
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isPushing", isPushing);
        animator.SetBool("isPulling", isPulling);

        if (isPushing && horizontalVelocity.magnitude < 0.01f) animator.speed = 0.0f;
        else animator.speed = 1.0f;

        if (startingJump)
        {
            animator.SetTrigger("jump");
            startingJump = false;
        }
        if (state.IsName("Jumping"))
        {
            animator.speed = 1.0f;
            if (state.normalizedTime > jumpSlowTime) animator.speed = 0.2f;
            if (state.normalizedTime > jumpPauseTime) animator.speed = 0.0f;
            if (IsGrounded) animator.speed = 1.0f;
        }
    }

    private float GetGroundDistance()
    {
        RaycastHit hit;
        if (Physics.Raycast(feetPos.position, Vector3.down, out hit, 100.0f))
        {
            return hit.distance;
        }
        return float.MaxValue;
    }
}
