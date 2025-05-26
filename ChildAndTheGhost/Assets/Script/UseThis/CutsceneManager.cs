using UnityEngine;
using UnityEngine.Video;

public class CutsceneManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject cutsceneUI; // RawImage or whole canvas
    public int requiredItemCount = 4;

    private int currentItemCount = 0;

    public void ItemDeliveredToMom()
    {
        currentItemCount++;

        if (currentItemCount >= requiredItemCount)
        {
            PlayCutscene();
        }
    }

    void PlayCutscene()
    {
        cutsceneUI.SetActive(true);
        videoPlayer.Play();

        // Optional: listen for video end
        videoPlayer.loopPointReached += OnCutsceneFinished;
    }

    void OnCutsceneFinished(VideoPlayer vp)
    {
        cutsceneUI.SetActive(false);
        videoPlayer.loopPointReached -= OnCutsceneFinished;

        // Continue game logic here
        Debug.Log("Cutscene ended.");
    }
}
