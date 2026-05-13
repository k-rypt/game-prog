using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;
    [SerializeField] private Image fadeOverlay;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private TextMeshProUGUI narrativeText;
    public event Action OnFadeComplete;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void TransitionToScene(string sceneName, string targetSpawnID)
    {
        Debug.Log($"TransitionToScene called. Scene: {sceneName}, SpawnID: {targetSpawnID}, Instance: {Instance}"); // ADDED
        StartCoroutine(DoTransition(sceneName, targetSpawnID));
    }

    private IEnumerator DoTransition(string sceneName, string targetSpawnID)
    {
        yield return StartCoroutine(Fade(0f, 1f));
        Debug.Log($"Setting spawnPointID to: {targetSpawnID}"); // ADDED
        SpawnManager.Instance.spawnPointID = targetSpawnID;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f) yield return null;

        op.allowSceneActivation = true;
        yield return null;
        yield return null;

        NarrativeCard card = FindObjectOfType<NarrativeCard>();
        if (card != null) yield return StartCoroutine(card.Play(narrativeText));

        yield return StartCoroutine(Fade(1f, 0f));
        OnFadeComplete?.Invoke();
    }

    private IEnumerator Fade(float from, float to)
    {
        fadeOverlay.gameObject.SetActive(true);
        float elapsed = 0f;
        Color c = fadeOverlay.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadeOverlay.color = c;
            yield return null;
        }

        c.a = to;
        fadeOverlay.color = c;

        if (to == 0f)
            fadeOverlay.gameObject.SetActive(false);
    }
}