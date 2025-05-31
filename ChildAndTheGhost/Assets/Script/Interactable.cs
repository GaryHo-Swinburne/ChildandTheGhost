using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string interactionPrompt = "Press F to interact";

    // NEW: Optional grab flag
    public bool canBeGrabbed = false;
    public bool canBeInteracted = false;

    public virtual void Interact()
    {
        Debug.Log("Interacted with: " + gameObject.name);
    }
}
