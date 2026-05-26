using UnityEngine;

public class Exit : MonoBehaviour
{
    [SerializeField] private string targetScene;
    [SerializeField] private string targetSpawnID;
    [SerializeField] private KeyCode enterKey = KeyCode.E;
    [SerializeField] private QuestManager.QuestAction requiredQuest = QuestManager.QuestAction.None; // ADDED

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(enterKey))
        {
            if (!IsUnlocked()) return; 
            SceneTransition.Instance.TransitionToScene(targetScene, targetSpawnID);
        }
    }

    bool IsUnlocked() 
    {
        if (requiredQuest == QuestManager.QuestAction.None) return true;
        return QuestManager.Instance?.CurrentQuest == requiredQuest;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }
}