using UnityEngine;

public class SwitchCharacter : MonoBehaviour
{
    public MonoBehaviour boyMovementScript;
    public MotherCamera motherCameraScript;
    public CharacterUI characterUI;

    public AudioSource cameraSwitchSound;

    private bool isControllingBoy = true;

    void Start()
    {
        SetControl(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SetControl(!isControllingBoy);
            cameraSwitchSound.Play();
        }
    }

    void SetControl(bool controlBoy)
    {
        isControllingBoy = controlBoy;

        if (boyMovementScript != null)
            boyMovementScript.enabled = controlBoy;

        if (motherCameraScript != null)
            motherCameraScript.EnablePlayerCameraControl(!controlBoy);

        if (characterUI != null)
            characterUI.SetControlState(controlBoy);

        Debug.Log("Switched to: " + (controlBoy ? "Boy" : "Mother"));
    }
}
