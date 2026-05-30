using UnityEngine;
using UnityEngine.UI;

public class FightTrigger : MonoBehaviour
{
    [Header("Dialogues")]
    [SerializeField] private DialogueData winDialogue;
    [SerializeField] private DialogueData momDialogue;
    [SerializeField] private DialogueData loseDialogue;

    [Header("Prompt")]
    [SerializeField] private DialogueData confirmDialogue;

    [Header("Confirmation Buttons")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Fight")]
    [SerializeField] private RhythmChart chart;

    private bool playerInRange = false;
    private bool triggered = false;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        confirmPanel.SetActive(false);

        yesButton.onClick.AddListener(OnYes);
        noButton.onClick.AddListener(OnNo);
    }

    private void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(OnYes);
        noButton.onClick.AddListener(OnNo);
    }

    private PlayerMovement GetPlayer()
    {
        if (playerMovement == null)
            playerMovement = FindObjectOfType<PlayerMovement>();
        return playerMovement;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !triggered)
        {
            if (DialogueManager.Instance == null) return;
            triggered = true;
            GetPlayer()?.SetMovementEnabled(false);
            DialogueManager.Instance.StartDialogue(confirmDialogue.lines, OnPromptEnd, suppressMovementRestore: true);
        }
    }

    private void OnPromptEnd()
    {
        confirmPanel.SetActive(true);
    }

    private void OnYes()
    {
        confirmPanel.SetActive(false);
        GetPlayer()?.SetMovementEnabled(false);
        RhythmManager.Instance.StartGame(chart, OnFightComplete, null, true);
    }

    private void OnNo()
    {
        confirmPanel.SetActive(false);
        triggered = false;
        GetPlayer()?.SetMovementEnabled(true);
    }

    private void OnFightComplete(bool victory)
    {
        triggered = false;

        if (victory)
        {
            DialogueManager.Instance.StartDialogue(winDialogue.lines, OnWinDialogueEnd, suppressMovementRestore: true);
        }
        else
        {
            if (loseDialogue != null)
                DialogueManager.Instance.StartDialogue(loseDialogue.lines, OnLoseDialogueEnd);
            else
                GetPlayer()?.SetMovementEnabled(true);
        }
    }

    private void OnWinDialogueEnd()
    {
        Debug.Log("[FightTrigger] OnMomDialogueEnd called");
        QuestManager.Instance.ClearQuest();
        DialogueManager.Instance.StartDialogue(momDialogue.lines, OnMomDialogueEnd, suppressMovementRestore: true);
        Debug.Log($"[FightTrigger] playerMovement enabled: {GetPlayer()?.enabled}");
    }

    private void OnMomDialogueEnd()
    {
        QuestManager.Instance?.SetQuest_TakeSomeTimeOff();
        GetPlayer()?.SetMovementEnabled(true);
    }

    private void OnLoseDialogueEnd()
    {
        GetPlayer()?.SetMovementEnabled(true);
    }
}