using UnityEngine;

public class IntroSequence : MonoBehaviour
{
    public DialogueData introDialogue;
    public PlayerMovement playerMovement;

    void Start()
    {
        Debug.Log("Starting intro sequence");
        if (SpawnManager.Instance.introPlayed) 
        {
            playerMovement.enabled = true; 
            return;                        
        }

        SpawnManager.Instance.introPlayed = true; 
        playerMovement.enabled = false;
        DialogueManager.Instance.StartDialogue(introDialogue.lines, onIntroEnd);
    }

    void onIntroEnd()
    {
        Debug.Log("Intro done, movement enabled");
        playerMovement.enabled = true;
    }
}