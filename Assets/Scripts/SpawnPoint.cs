using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] public string spawnID;

    void Start()
    {
        Debug.Log($"SpawnPoint firing. ID: {spawnID}, Manager ID: {SpawnManager.Instance?.spawnPointID}, Prefab: {SpawnManager.Instance?.playerPrefab}");

        if (SpawnManager.Instance == null) return;
        if (SpawnManager.Instance.spawnPointID == spawnID)
        {
            GameObject player = GameObject.FindWithTag("Player");
            Debug.Log($"Player found: {player}");
            if (player != null)
            {
                player.transform.position = transform.position;
                AssignCinemachineTarget(player.transform); 
            }
            else
            {
                GameObject spawned = Instantiate(SpawnManager.Instance.playerPrefab, transform.position, Quaternion.identity); 
                AssignCinemachineTarget(spawned.transform); 
            }
        }
    }

    void AssignCinemachineTarget(Transform target) 
    { 
        CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>(); 
        if (vcam == null) return; 
        vcam.Follow = target; 
        vcam.LookAt = target; 
    } 
}