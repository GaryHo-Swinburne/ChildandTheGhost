using UnityEngine;

public class CollectibleItem : Interactable
{
    public string itemName = "Collectible";
    public bool canGiveToMother = true;

    [HideInInspector] public bool isCollected = false;

    private void Awake()
    {
        //interactionPrompt = "Press F to collect";
        canBeInteracted = true;
    }

    // Optional: helper for interacting
    public override void Interact()
    {
        isCollected = true;
        gameObject.SetActive(false); // Hide the object once picked up
        Debug.Log("[Item] Collected: " + itemName);
    }
}
