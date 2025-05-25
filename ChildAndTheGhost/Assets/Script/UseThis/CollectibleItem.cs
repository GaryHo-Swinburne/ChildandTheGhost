using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    public string itemName = "Collectible";
    public string interactionPrompt = "Press E to collect";

    [HideInInspector] public bool isCollected = false;

    // Optional: helper for interacting
    public void OnCollect()
    {
        isCollected = true;
        gameObject.SetActive(false); // Hide the object once picked up
        Debug.Log("[Item] Collected: " + itemName);
    }
}
