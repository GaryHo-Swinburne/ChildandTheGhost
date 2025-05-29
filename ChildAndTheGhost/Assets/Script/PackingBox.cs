using UnityEngine;

public class PackingBox : Interactable
{
    private void Awake()
    {
        canBeGrabbed = true;
    }

    public override void Interact()
    {
        Debug.Log("Packing the box!");
    }
}
