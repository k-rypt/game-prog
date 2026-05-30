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
        StartCoroutine(DoTransition(sceneName, targetSpawnID));
    }

    private IEnumerator DoTransition(string sceneName, string targetSpawnID)
    {
        if (QuestManager.Instance != null)
            StartCoroutine(FadeQuestText(1f, 0f));
        yield return StartCoroutine(Fade(0f, 1f));

        SpawnManager.Instance.spawnPointID = targetSpawnID;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;
        while (op.progress < 0.9f) yield return null;
        op.allowSceneActivation = true;
        yield return null;
        yield return null;

        NarrativeCard card = System.Array.Find(
        FindObjectsOfType<NarrativeCard>(),
        c => c.isIntroCard);

        if (card != null) yield return StartCoroutine(card.Play(narrativeText));
        if (QuestManager.Instance != null)
            StartCoroutine(FadeQuestText(0f, 1f));
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

    private IEnumerator FadeQuestText(float from, float to)
    {
        var tmp = QuestManager.Instance.QuestText;
        if (tmp == null || !tmp.gameObject.activeSelf) yield break;

        float elapsed = 0f;
        Color c = tmp.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / fadeDuration);
            tmp.color = c;
            yield return null;
        }
        c.a = to;
        tmp.color = c;
    }

    public IEnumerator FadeToBlackAndEnd(NarrativeCard card) 
    {
        if (QuestManager.Instance != null)
            StartCoroutine(FadeQuestText(1f, 0f));
        yield return StartCoroutine(Fade(0f, 1f));

        if (card != null) yield return StartCoroutine(card.Play(narrativeText));
    }
}