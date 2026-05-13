using System.Collections;
using UnityEngine;

public class SceneDialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private float delay = 0.5f;

    void Start()
    {
        SceneTransition.Instance.OnFadeComplete += OnFadeInComplete;
    }

    void OnDestroy()
    {
        // clean up the subscription when the scene unloads
        if (SceneTransition.Instance != null)
            SceneTransition.Instance.OnFadeComplete -= OnFadeInComplete;
    }

    private void OnFadeInComplete()
    {
        StartCoroutine(TriggerAfterDelay());
    }

    private IEnumerator TriggerAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        DialogueManager.Instance.StartDialogue(dialogueData.lines);
    }
}
