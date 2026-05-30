using System.Collections;
using UnityEngine;

public class SceneDialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private float delay = 0.5f;
    [SerializeField] private QuestManager.QuestAction postDialogueQuest = QuestManager.QuestAction.None; 

    void Start()
    {
        SceneTransition.Instance.OnFadeComplete += OnFadeInComplete;
    }

    void OnDestroy()
    {
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
        FindObjectOfType<PlayerMovement>().enabled = false;
        DialogueManager.Instance.StartDialogue(dialogueData.lines, OnSceneDialogueEnd);
    }

    private void OnSceneDialogueEnd()
    {
        FindObjectOfType<PlayerMovement>().enabled = true;
        Debug.Log($"[SceneDialogueTrigger] OnSceneDialogueEnd fired. QuestManager null? {QuestManager.Instance == null}. postDialogueQuest: {postDialogueQuest}");
        QuestManager.Instance?.SetQuestFromAction(postDialogueQuest);
    }
}