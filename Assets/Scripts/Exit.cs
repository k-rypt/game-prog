using UnityEngine;

public class Exit : MonoBehaviour
{
    [SerializeField] private string targetScene;
    [SerializeField] private string targetSpawnID;
    [SerializeField] private KeyCode enterKey = KeyCode.E;

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(enterKey))
        {
            SceneTransition.Instance.TransitionToScene(targetScene, targetSpawnID);
        }
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