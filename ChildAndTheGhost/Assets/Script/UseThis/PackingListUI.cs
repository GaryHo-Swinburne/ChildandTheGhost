using UnityEngine;

public class PackingListUI : MonoBehaviour
{
    public GameObject missionPanel;
    public GameObject tickSoccer, tickLego, tickWater, tickLetter;
    public GameObject strikeSoccer, strikeLego, strikeWater, strikeLetter;

    [Header("Sound Effect")]
    public AudioSource openSound;
    public AudioSource itemDeliveredSound;

    private bool isVisible = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            isVisible = !isVisible;
            missionPanel.SetActive(isVisible);

            if (isVisible && openSound != null)
            {
                openSound.Play();
            }
        }
    }

    public void MarkItemDelivered(string itemName)
    {
        Debug.Log("Calling MarkItemDelivered with: " + itemName);

        if (itemDeliveredSound != null)
            itemDeliveredSound.Play();

        switch (itemName)
        {
            case "Soccer Trophy":
                tickSoccer?.SetActive(true);
                strikeSoccer?.SetActive(true);
                break;
            case "New Lego Set":
                tickLego?.SetActive(true);
                strikeLego?.SetActive(true);
                break;
            case "Water Pistols":
                tickWater?.SetActive(true);
                strikeWater?.SetActive(true);
                break;
            case "Old Santa Letter":
                tickLetter?.SetActive(true);
                strikeLetter?.SetActive(true);
                break;
            default:
                Debug.LogWarning("Unrecognized item delivered: " + itemName);
                break;
        }
    }
}
