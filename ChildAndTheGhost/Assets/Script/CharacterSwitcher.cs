using UnityEngine;

public class CharacterSwitcher : MonoBehaviour
{
    [Header("Boy Settings")]
    public GameObject boy;
    public Camera boyCamera;
    public MonoBehaviour boyMovementScript; // Example: ThirdPersonMovement or custom movement script

    [Header("Mother Settings")]
    public Camera motherCamera;
    public MotherPOVCamera motherPOVCamera; // The FPV script on mother's camera

    private bool isInMotherView = false;

    void Start()
    {
        // Start in boy view
        isInMotherView = false;

        // Enable boy components
        boyCamera.enabled = true;
        if (boyMovementScript != null)
            boyMovementScript.enabled = true;

        // Disable mother components
        motherCamera.enabled = false;
        if (motherPOVCamera != null)
            motherPOVCamera.SetControllable(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleCharacterView();
        }
    }

    void ToggleCharacterView()
    {
        isInMotherView = !isInMotherView;

        // Camera toggle
        boyCamera.enabled = !isInMotherView;
        motherCamera.enabled = isInMotherView;

        // Movement toggle
        if (boyMovementScript != null)
            boyMovementScript.enabled = !isInMotherView;

        // Mother POV control toggle
        if (motherPOVCamera != null)
            motherPOVCamera.SetControllable(isInMotherView);

        // Debug
        Debug.Log("Switched to: " + (isInMotherView ? "Mother POV" : "Boy Control"));
    }
}
