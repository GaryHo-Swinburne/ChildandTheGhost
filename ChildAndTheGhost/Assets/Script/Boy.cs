using UnityEngine;
using TMPro;
using UnityEngine.Video;
using System.Collections.Generic;

public class Boy : MonoBehaviour
{
    public TMP_Text promptText;

    [Header("Delivery")]
    public Transform mother;
    public float giveDistance;

    [Header("Cutscene")]
    public VideoPlayer cutscenePlayer;
    public GameObject cutsceneScreen;
    public int itemsToTriggerCutscene = 4;

    [Header("Audio")]
    public AudioSource backgroundMusic;  // �� Add this line
    public AudioSource grabSound;
    public AudioSource walkSound;

    [Header("Item List")]
    public List<CollectibleItem> deliverableItems;

    [Header("UI")]
    public GameObject switchIndicator;
    public PackingListUI packingListUI;



    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float gravity;
    [SerializeField] private float walkSoundWaitTime;
    private float walkSoundTime = 0.0f;

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
    [SerializeField] private float interactTime;
    private Interactable heldObject;
    private bool startingInteract = false;
    private float interactFinishTime = -1.0f;
    private CollectibleItem currentItem;
    private int deliveredItemCount = 0;

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
        promptText.enabled = false;
        if (currentItem != null && Vector3.Distance(transform.position, mother.position) <= giveDistance)
        {
            promptText.text = "Press F to give to Mom";
            promptText.enabled = true;
        }
        Interactable interactable = CheckForCollectible();
        if (currentItem == null && interactable != null)
        {
            promptText.text = interactable.interactionPrompt;
            promptText.enabled = true;
        }


        Vector3 move = CheckInput();
        Vector3 velocity = MovePlayer(move);
        UpdateAnimations(velocity);
    }

    private bool IsInJump => jumpTime > 0.0f || isJumping || finishLandingTime > 0.0f;
    private bool IsGrounded => GetGroundDistance() < 0.1f;
    private bool FacingStraight => Mathf.Abs(transform.forward.x) < 0.01f || Mathf.Abs(transform.forward.z) < 0.01f;
    private bool Interacting => interactFinishTime > 0.0f && Time.time < interactFinishTime;
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
        // If the player has been blocked from using input (because of the mother/boy switching)
        if (!CanUseInput) return Vector3.zero;

        // Reset interactFinishTime if it has passed
        if (Time.time > interactFinishTime) interactFinishTime = -1.0f;

        // The player can start an action (interact, grab, jump) if they are grounded, not in a jump, not at a ledge and not interacting
        bool canStartAction = IsGrounded & !IsInJump && !AtLedge && !Interacting;

        // Check if the player should jump
        if (Input.GetButtonDown("Jump") && canStartAction && heldObject == null)
        {
            jumpTime = Time.time + jumpStartWaitTime;
            startingJump = true;
            return Vector3.zero;
        }

        // Check if the player is pressing the interact button
        if (Input.GetKeyDown(KeyCode.F) && canStartAction && heldObject == null)
        {
            TryInteraction();
        }

        // If the player is interacting, no grabbing or moving is allowed
        if (Interacting) return Vector3.zero;

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
        float x = -Input.GetAxis("Horizontal");
        float z = -Input.GetAxis("Vertical");

        return new Vector3(x, 0, z).normalized;
    }

    private void TryInteraction()
    {
        // Check for giving the object to mother
        if (currentItem != null)
        {
            if (Vector3.Distance(transform.position, mother.position) <= giveDistance)
            {
                if (deliverableItems.Contains(currentItem))
                {
                    Debug.Log("Item given to mom: " + currentItem.itemName);

                    deliveredItemCount++;
                    packingListUI?.MarkItemDelivered(currentItem.itemName);

                    CheckCutsceneTrigger();
                }
                else
                {
                    Debug.Log("Item not deliverable.");
                }

                Destroy(currentItem.gameObject);
                currentItem = null;
            }

            return;
        }

        // Check for interaction with an object
        CollectibleItem item = CheckForCollectible();
        if (item != null)
        {
            // Interact with the object
            item.Interact();
            currentItem = item;
            startingInteract = true;
            interactFinishTime = Time.time + interactTime;
            grabSound.Play();
        }
    }

    private CollectibleItem CheckForCollectible()
    {
        Ray ray = new Ray(eyePoint.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactRange))
        {
            CollectibleItem item = hit.collider.GetComponent<CollectibleItem>();
            if (item != null && item.canBeInteracted)
            {
                return item;
            }
        }

        return null;
    }

    void CheckCutsceneTrigger()
    {
        if (deliveredItemCount >= itemsToTriggerCutscene && !cutscenePlayer.isPlaying)
        {
            cutsceneScreen.SetActive(true);
            switchIndicator.SetActive(false);
            cutscenePlayer.Play();

            if (backgroundMusic != null)
                backgroundMusic.mute = true;
        }
    }

    private Interactable CheckForGrabbable()
    {
        Ray ray = new Ray(eyePoint.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactRange))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null && interactable.canBeGrabbed)
            {
                return interactable;
            }
        }
        return null;
    }

    private void TryToGrabObject()
    {
        Interactable interactable = CheckForGrabbable();
        if (interactable != null)
        {
            heldObject = interactable;
            heldObject.transform.SetParent(grabPoint);
            heldObject.transform.localPosition = Vector3.zero;
            grabSound.Play();
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

        // Play walking sound
        if (velocity.magnitude > 0.1f && IsGrounded)
        {
            if (Time.time > walkSoundTime)
            {
                walkSound.Play();
                walkSoundTime = Time.time + walkSoundWaitTime;
            }
        }

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

        if (startingInteract)
        {
            animator.SetTrigger("interact");
            startingInteract = false;
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
