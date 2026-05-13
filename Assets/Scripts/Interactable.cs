using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private Sprite inspectSprite;
    [SerializeField] private DialogueData dialogueData;
    private bool playerInRange = false;
    private bool waiting = false;
    private bool dialogueEnd = false;

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
        if(playerInRange && Input.GetKeyDown(KeyCode.E) && !waiting)
        {
            waiting = true;
            DialogueManager.Instance.StartInspect(inspectSprite, dialogueData); 
        }

        if(waiting && !DialogueManager.Instance.IsDialogueOpen)
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
            }
        }
    }
}
