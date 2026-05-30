using System.Collections;
using UnityEngine;
using TMPro;

public class NarrativeCard : MonoBehaviour
{
    [SerializeField] private string[] lines;         
    [SerializeField] private float charDelay = 0.04f;
    [SerializeField] private float linePause = 1f;   
    [SerializeField] private float holdAfter = 1.5f;
    [SerializeField] public bool isIntroCard = true;

    public IEnumerator Play(TextMeshProUGUI narrativeText)
    {
        narrativeText.text = "";
        narrativeText.gameObject.SetActive(true);

        foreach (string line in lines)
        {
            narrativeText.text = "";
            foreach (char c in line)
            {
                narrativeText.text += c;
                yield return new WaitForSeconds(charDelay);
            }
            yield return new WaitForSeconds(linePause);
        }

        yield return new WaitForSeconds(holdAfter);
        narrativeText.gameObject.SetActive(false);
    }
}