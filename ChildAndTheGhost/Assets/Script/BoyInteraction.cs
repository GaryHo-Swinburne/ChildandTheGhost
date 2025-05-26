using UnityEngine;
using TMPro;

public class BoyInteraction : MonoBehaviour
{
    [Header("References")]
    public Transform eyePoint;              // Empty GameObject positioned at eye level
    public TextMeshProUGUI promptText;      // UI element for interaction prompt
    public Transform holdPoint;             // Where objects are held

    [Header("Settings")]
    public float interactRange = 2f;

    private Interactable currentInteractable;
    private Rigidbody heldObjectRb;
    private Transform heldObject;

    private float holdStartTime;
    public bool isGrabbing = false;

    void Update()
    {
        CheckForInteractable();

        // Handle grab start
        if (Input.GetKeyDown(KeyCode.E))
        {
            holdStartTime = Time.time;
        }

        // While holding E
        if (Input.GetKey(KeyCode.E))
        {
            // Only grab if not already holding something
            if (!isGrabbing && heldObject == null && currentInteractable != null && currentInteractable.canBeGrabbed)
            {
                GrabObject(currentInteractable.transform);
                isGrabbing = true;
            }
        }

        // On release
        if (Input.GetKeyUp(KeyCode.E))
        {
            DropObject();
            isGrabbing = false;
        }
    }

    void CheckForInteractable()
    {
        Vector3 origin = eyePoint.position;

        Ray ray = new Ray(origin, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();

            if (interactable != null)
            {
                currentInteractable = interactable;
                promptText.text = interactable.interactionPrompt;
                promptText.enabled = true;
                return;
            }
        }

        currentInteractable = null;
        promptText.enabled = false;
    }

    void GrabObject(Transform obj)
    {
        heldObject = obj;
        heldObjectRb = obj.GetComponent<Rigidbody>();

        if (heldObjectRb != null)
        {
            heldObjectRb.isKinematic = true;
        }

        heldObject.SetParent(holdPoint);
        heldObject.localPosition = Vector3.zero;
        heldObject.localRotation = Quaternion.identity;

        Debug.Log("Grabbed object: " + heldObject.name);
    }

    void DropObject()
    {
        if (heldObject == null) return;

        heldObject.SetParent(null);

        if (heldObjectRb != null)
        {
            heldObjectRb.isKinematic = false;
            heldObjectRb = null;
        }

        Debug.Log("Dropped object: " + heldObject.name);
        heldObject = null;
    }
}
