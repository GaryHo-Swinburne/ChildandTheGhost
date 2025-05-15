using UnityEngine;

public class PackingBox : Interactable
{
    private void Awake()
    {
        canBeGrabbed = true; // Set this manually or via Inspector
    }

    public override void Interact()
    {
        Debug.Log("Packing the box!");
    }
}
