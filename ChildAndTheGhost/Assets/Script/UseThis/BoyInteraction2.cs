using UnityEngine;
using TMPro;
using UnityEngine.Video;

[RequireComponent(typeof(CharacterController))]
public class BoyInteraction2 : MonoBehaviour
{
    [Header("Interaction")]
    public float interactRange = 2f;
    public LayerMask interactionLayer;
    public Transform holdPoint;
    public TMP_Text promptText;

    [Header("Debug")]
    public Vector3 eyeOffset = new Vector3(0f, 1.2f, 0f);

    [Header("Delivery")]
    public Transform mother;
    public float giveDistance = 2f;

    [Header("Cutscene")]
    public VideoPlayer cutscenePlayer;
    public GameObject cutsceneScreen;
    public int itemsToTriggerCutscene = 4;

    [Header("Audio")]
    public AudioSource backgroundMusic;  // ¡ö Add this line

    [Header("Item List")]
    public CollectibleItem[] deliverableItems;

    private GameObject heldObject;
    private CollectibleItem currentItem;
    private CharacterController controller;

    private int deliveredItemCount = 0;

    [Header("UI Control")]
    public GameObject switchIndicator;

    [Header("Mission UI")]
    public PackingListUI packingListUI;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (heldObject == null)
        {
            CheckForInteractable();

            if (currentItem != null && Input.GetKeyDown(KeyCode.E))
            {
                PickUp(currentItem.gameObject);
            }
        }
        else
        {
            float distanceToMom = Vector3.Distance(transform.position, mother.position);

            if (distanceToMom <= giveDistance)
            {
                promptText.text = "Press E to give to Mom";
                promptText.enabled = true;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    GiveToMom();
                }
            }
            else
            {
                if (Input.GetKeyUp(KeyCode.E))
                {
                    TryDropItem();
                }
            }
        }
    }

    void CheckForInteractable()
    {
        currentItem = null;
        promptText.enabled = false;

        Vector3 origin = transform.position + eyeOffset;
        Vector3 direction = transform.forward;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, interactRange, interactionLayer))
        {
            Debug.DrawRay(origin, direction * interactRange, Color.green);
            CollectibleItem collectible = hit.collider.GetComponent<CollectibleItem>();
            if (collectible != null && heldObject == null)
            {
                currentItem = collectible;
                promptText.text = collectible.interactionPrompt;
                promptText.enabled = true;
            }
        }
        else
        {
            Debug.DrawRay(origin, direction * interactRange, Color.red);
        }
    }

    void PickUp(GameObject item)
    {
        if (heldObject != null) return;

        heldObject = item;
        heldObject.transform.SetParent(holdPoint);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider col = heldObject.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        promptText.enabled = false;
        Debug.Log("Picked up: " + heldObject.name);
    }

    void TryDropItem()
    {
        if (!controller.isGrounded)
        {
            Debug.Log("Can't drop while in the air.");
            return;
        }

        DropItem();
    }

    void DropItem()
    {
        if (heldObject == null) return;

        heldObject.transform.SetParent(null);

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        Collider col = heldObject.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        heldObject = null;
        Debug.Log("Dropped item.");
    }

    void GiveToMom()
    {
        if (heldObject == null) return;

        CollectibleItem delivered = heldObject.GetComponent<CollectibleItem>();
        if (delivered != null && IsDeliverable(delivered))
        {
            Debug.Log("Item given to mom: " + delivered.itemName);

            deliveredItemCount++;
            packingListUI?.MarkItemDelivered(delivered.itemName);

            CheckCutsceneTrigger();
        }
        else
        {
            Debug.Log("Item not deliverable.");
        }

        Destroy(heldObject);
        heldObject = null;
        promptText.enabled = false;
    }


    bool IsDeliverable(CollectibleItem item)
    {
        foreach (CollectibleItem target in deliverableItems)
        {
            if (target != null && item.itemName == target.itemName)
                return true;
        }
        return false;
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



    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position + eyeOffset;
        Gizmos.DrawRay(origin, transform.forward * interactRange);
    }
}
