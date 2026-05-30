// RhythmHUD.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RhythmHUD : MonoBehaviour
{
    [Header("Lane Flash Images (index 0-3)")]
    public Image[] laneFlashImages;

    [Header("Hit Zone Images (index 0-3)")] 
    public Image[] hitZoneImages;          

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
    public void UpdateHealth(float value) { }
    public void UpdateScore(int score, int combo) { }
    public void ShowJudgement(string text) { }
    public void ShowResult(bool victory) { }

    public void FlashLane(int lane) { StartCoroutine(LaneFlash(lane)); }

    IEnumerator LaneFlash(int lane)
    {
        if (laneFlashImages[lane] == null) yield break;
        hitZoneImages[lane].gameObject.SetActive(false);      
        laneFlashImages[lane].color = new Color(1f, 1f, 1f, 0.4f);
        yield return new WaitForSeconds(0.1f);
        laneFlashImages[lane].color = new Color(1f, 1f, 1f, 0f);
        hitZoneImages[lane].gameObject.SetActive(true);       
    }
}