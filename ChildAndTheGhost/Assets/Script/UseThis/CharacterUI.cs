using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    public Sprite motherSprite;
    public Sprite childSprite;
    public Image indicatorImage;

    public void SetControlState(bool isControllingBoy)
    {
        if (indicatorImage == null) return;
        indicatorImage.sprite = isControllingBoy ? childSprite : motherSprite;
    }
}
