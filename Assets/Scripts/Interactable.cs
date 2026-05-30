using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [SerializeField] private Sprite inspectSprite;
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private QuestManager.QuestAction postInspectQuest = QuestManager.QuestAction.None;
    [SerializeField] private bool useEndSequence = false;

    private bool playerInRange = false;
    private bool waiting = false;
    private bool dialogueEnd = false;
    private PlayerMovement playerMovement;                                                          

    private void Start()                                                                            
    {                                                                                               
        playerMovement = FindObjectOfType<PlayerMovement>();                                        
    }                                                                                               

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !waiting)
        {
            if (useEndSequence && !IsEndUnlocked()) return;

            waiting = true;

            if (useEndSequence)
            {
                NarrativeCard card = GetComponentInChildren<NarrativeCard>();
                StartCoroutine(SceneTransition.Instance.FadeToBlackAndEnd(card));
                return;
            }

            playerMovement?.SetMovementEnabled(false);                                             
            DialogueManager.Instance.StartInspect(inspectSprite, dialogueData);
        }

        if (!useEndSequence && waiting && !DialogueManager.Instance.IsDialogueOpen)
        {
            if (!dialogueEnd)
            {
                dialogueEnd = true;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                DialogueManager.Instance.EndInspect();
                waiting = false;
                dialogueEnd = false;
                playerMovement?.SetMovementEnabled(true);                                         
                QuestManager.Instance?.SetQuestFromAction(postInspectQuest);
            }
        }
    }

    bool IsEndUnlocked()
    {
        return QuestManager.Instance?.CurrentQuest == QuestManager.QuestAction.TakeSomeTimeOff;
    }
}