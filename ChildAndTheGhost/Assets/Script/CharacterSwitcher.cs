using UnityEngine;

public class CharacterSwitcher : MonoBehaviour
{
    [Header("Boy Settings")]
    public Boy boy;

    [Header("Mother Settings")]
    public MotherPOVCamera mother;

    private bool isInMotherView = false;

    void Start()
    {
        // Start in boy view
        isInMotherView = false;

        boy.CanUseInput = true;
        mother.CanUseInput = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleCharacterView();
    }

    void ToggleCharacterView()
    {
        isInMotherView = !isInMotherView;

        // Movement toggle
        boy.CanUseInput = !isInMotherView;
        mother.CanUseInput = isInMotherView;
    }
}
