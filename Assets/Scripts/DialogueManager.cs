using System;
using System.Collections;
using TMPro;
using UnityEngine;

[System.Serializable]
public struct DialogueLine
{
    public string name;
    public string text;
}

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject dialogueBox;
    [Header("Inspect")]
    [SerializeField] private GameObject inspectPanel;
    [SerializeField] private UnityEngine.UI.Image posterImage;
    [Header("Player")]
    private PlayerMovement playerMovement;

    public static DialogueManager Instance;
    public TextMeshProUGUI characterNameComponent;
    public TextMeshProUGUI dialogueTextComponent;
    public float textSpeed = 0.05f;
    public TMP_FontAsset font;
    private DialogueLine[] currentLines;
    private int index;
    private bool isTyping = false;
    private Action onDialogueEnd;
    private bool _suppressMovementRestore = false;
    public bool IsDialogueOpen => dialogueBox.activeSelf;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        dialogueBox.SetActive(false);
    }

    void Start()
    {
        dialogueTextComponent.font = font;
        characterNameComponent.font = font;
    }

    void Update()
    {
        if (!dialogueBox.activeSelf) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueTextComponent.text = currentLines[index].text;
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    public void StartDialogue(DialogueLine[] lines, Action onEnd = null, bool suppressMovementRestore = false)
    {
        currentLines = lines;
        index = 0;
        onDialogueEnd = onEnd;
        _suppressMovementRestore = suppressMovementRestore;
        dialogueBox.SetActive(true);

        if (playerMovement == null) playerMovement = FindObjectOfType<PlayerMovement>();
        playerMovement?.SetMovementEnabled(false);

        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueTextComponent.text = string.Empty;
        characterNameComponent.text = currentLines[index].name;
        foreach (char c in currentLines[index].text.ToCharArray())
        {
            dialogueTextComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
        isTyping = false;
    }

    void NextLine()
    {
        if (index < currentLines.Length - 1)
        {
            index++;
            StartCoroutine(TypeLine());
        }
        else
        {
            dialogueTextComponent.text = string.Empty;
            dialogueBox.SetActive(false);
            if (!_suppressMovementRestore)
                playerMovement?.SetMovementEnabled(true);
            Debug.Log($"[DialogueManager] dialogue ended, suppress={_suppressMovementRestore}, invoking callback: {onDialogueEnd != null}");
            onDialogueEnd?.Invoke();
        }
    }

    public void StartInspect(Sprite sprite, DialogueData dialogue, Action onDismiss = null)
    {
        inspectPanel.SetActive(true);
        posterImage.sprite = sprite;
        StartDialogue(dialogue.lines, onDismiss);
    }

    public void EndInspect()
    {
        inspectPanel.SetActive(false);
    }
}