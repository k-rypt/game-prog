using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RhythmManager : MonoBehaviour
{
    public static RhythmManager Instance;
    public enum Quest { None, TakeAChance, TakeARest, TakeARisk, TakeSomeTimeOff }

    [Header("Chart")]
    public RhythmChart chart;

    [Header("Cheats")]  
    public bool autoStart = true;                  
    public bool skipMinigame = false;       
    public bool isTutorial = false; 

    [Header("Timing Windows")]
    public float perfect = 20f;
    public float good = 30f;

    [Header("Note Prefabs (one per lane, index 0-3)")]
    public GameObject[] notePrefabs;

    [Header("Lane X positions in HUD (local space)")]
    public float[] laneXPositions;

    [Header("Scroll & Spawn")]
    public float scrollSpeed = 300f;
    public float spawnY = 400f;
    public float hitZoneY = -300f;

    [Header("Health")]
    public float health = 0.5f;
    public float perfectHeal = 0.04f;
    public float goodHeal = 0.02f;
    public float missPenalty = 0.08f;

    [Header("References")]
    public RhythmHUD hud;
    public Transform noteParent;
    public DialogueData interruptDialogue;
    [SerializeField] private Quest postInterruptQuest = Quest.None;

    private readonly KeyCode[] _laneKeys = { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K };
    private float _songTime = -1f;
    private bool _running = false;
    private bool _interrupted = false;
    private int _nextNoteIndex = 0;
    private List<NoteObject> _activeNotes = new List<NoteObject>();
    private List<RhythmNote> _generatedNotes = new List<RhythmNote>();
    private int _score = 0;
    private int _combo = 0;
    private bool _failed = false;
    private System.Action<bool> _onFightComplete; 
    private PlayerMovement playerMovement; 


    void Awake() { Instance = this; }

    void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>(); 

        if (autoStart)
        {
            if (skipMinigame)
            {
                OnInterruptDialogueEnd();
                return;
            }
            playerMovement?.SetMovementEnabled(false); 
            StartGame();
        }
    }

    public void StartGame(RhythmChart overrideChart = null, System.Action<bool> onComplete = null, DialogueData overrideInterruptDialogue = null, bool hasDialogueOverride = false)
    {
        if (_running) return;
        if (overrideChart != null) chart = overrideChart;
        if (hasDialogueOverride) interruptDialogue = overrideInterruptDialogue;
        _onFightComplete = onComplete;
        QuestManager.Instance?.ClearQuest();

        if (chart.autoGenerate)
            GenerateNotes();

        _running = true;
        _songTime = -chart.offset;
        _nextNoteIndex = 0;
        health = 0.5f;
        _score = 0;
        _combo = 0;
        hud.Show();
        hud.UpdateHealth(health);
        hud.UpdateScore(_score, _combo);
        StartCoroutine(GameLoop());
    }

    public void ResetFight()
    {
        _running = false;
        _interrupted = false;
        _failed = false;        
        _songTime = -1f;
        _nextNoteIndex = 0;
        _score = 0;
        _combo = 0;
        health = 0.5f;
        foreach (NoteObject n in _activeNotes)
            if (n != null) Destroy(n.gameObject);
        _activeNotes.Clear();
        _generatedNotes.Clear();
    }                                       

    void GenerateNotes()
    {
        _generatedNotes.Clear();
        float secondsPerBeat = 60f / chart.bpm;
        float travelTime = (hitZoneY - spawnY) / -scrollSpeed;
        float currentTime = travelTime;

        while (currentTime < chart.interruptTime + travelTime)
        {
            RhythmNote note = ScriptableObject.CreateInstance<RhythmNote>();
            note.lane = Random.Range(0, 4);
            note.beatTime = currentTime;
            _generatedNotes.Add(note);
            currentTime += secondsPerBeat;
        }

        _generatedNotes.Sort((a, b) => a.beatTime.CompareTo(b.beatTime));
    }

    IEnumerator GameLoop()
    {
        List<RhythmNote> noteSource = chart.autoGenerate ? _generatedNotes : chart.notes;
        noteSource.Sort((a, b) => a.beatTime.CompareTo(b.beatTime));

        while (_running)
        {
            _songTime += Time.deltaTime;

            if (!_interrupted && _songTime >= chart.interruptTime)
            {
                _interrupted = true;
                StartCoroutine(Interrupt());
                yield break;
            }

            while (_nextNoteIndex < noteSource.Count)
            {
                RhythmNote note = noteSource[_nextNoteIndex];
                float timeUntilHit = note.beatTime - _songTime;
                float travelTime = (spawnY - hitZoneY) / scrollSpeed;

                if (timeUntilHit <= travelTime) { SpawnNote(note); _nextNoteIndex++; }
                else break;
            }

            for (int i = 0; i < 4; i++)
            {
                if (Input.GetKeyDown(_laneKeys[i]))
                {
                    TryHit(i);
                    hud.FlashLane(i);
                }
            }

            hud.UpdateHealth(health);
            yield return null;
        }
    }

    void SpawnNote(RhythmNote note)
    {
        GameObject prefab = notePrefabs[note.lane];
        GameObject obj = Instantiate(prefab, noteParent);
        NoteObject no = obj.GetComponent<NoteObject>();

        Vector3 pos = obj.transform.localPosition;
        pos.x = laneXPositions[note.lane];
        obj.transform.localPosition = pos;

        no.Init(note.lane, note.beatTime, scrollSpeed, spawnY, hitZoneY);
        _activeNotes.Add(no);
    }

    void TryHit(int lane)
    {
        NoteObject closest = null;
        float closestDistance = float.MaxValue;

        foreach (NoteObject note in _activeNotes)
        {
            if (note == null || note.lane != lane || note.IsMissed()) continue;
            float distance = note.GetVisualDistanceFromHitZone();
            if (distance < closestDistance) { closestDistance = distance; closest = note; }
        }

        if (closest == null) return;

        if (closestDistance <= perfect) RegisterHit(closest, true);
        else if (closestDistance <= good) RegisterHit(closest, false);
    }

    void RegisterHit(NoteObject note, bool isPerfect)
    {
        _activeNotes.Remove(note);
        Destroy(note.gameObject);

        if (isPerfect)
        {
            health = Mathf.Clamp01(health + perfectHeal);
            _combo++;
            _score += 300 * _combo;
            hud.ShowJudgement("PERFECT");
        }
        else
        {
            health = Mathf.Clamp01(health + goodHeal);
            _combo++;
            _score += 100 * _combo;
            hud.ShowJudgement("GOOD");
        }

        hud.UpdateHealth(health);
        hud.UpdateScore(_score, _combo);

        if (health <= 0f && !_failed && !isTutorial)
        {
            _failed = true;             
            StartCoroutine(Fail());
        }
    }

    public void RegisterMiss()
    {
        if (_failed) return;

        health = Mathf.Clamp01(health - missPenalty);
        _combo = 0;
        hud.ShowJudgement("MISS");
        hud.UpdateHealth(health);
        hud.UpdateScore(_score, _combo);

        if (health <= 0f && !isTutorial) // MODIFIED
        {
            _failed = true;    
            StartCoroutine(Fail());
        }
    }

    IEnumerator Interrupt()
    {
        _running = false;

        foreach (NoteObject n in _activeNotes)
            if (n != null) n.enabled = false;

        yield return new WaitForSeconds(1.5f);

        foreach (NoteObject n in _activeNotes)
            if (n != null) Destroy(n.gameObject);
        _activeNotes.Clear();

        hud.Hide();
        yield return new WaitForSeconds(0.3f);

        bool victory = health >= 0.5f;
        playerMovement?.SetMovementEnabled(true); // ADD
        if (interruptDialogue != null)
            DialogueManager.Instance.StartDialogue(interruptDialogue.lines, OnInterruptDialogueEnd);
        else
            _onFightComplete?.Invoke(victory);
    }

    IEnumerator Fail()
    {
        _running = false;

        foreach (NoteObject n in _activeNotes)
            if (n != null) n.enabled = false;

        hud.ShowResult(false);
        yield return new WaitForSeconds(2f);

        foreach (NoteObject n in _activeNotes)
            if (n != null) Destroy(n.gameObject);
        _activeNotes.Clear();

        hud.Hide();
        yield return new WaitForSeconds(0.3f);

        playerMovement?.SetMovementEnabled(true); // ADD
        if (interruptDialogue != null)
            DialogueManager.Instance.StartDialogue(interruptDialogue.lines, () => _onFightComplete?.Invoke(false));
        else
            _onFightComplete?.Invoke(false);
    }

    void OnInterruptDialogueEnd()
    {
        switch (postInterruptQuest)
        {
            case Quest.TakeAChance:     QuestManager.Instance?.SetQuest_TakeAChance(); break;
            case Quest.TakeARisk:       QuestManager.Instance?.SetQuest_TakeARisk(); break;
            case Quest.TakeARest:       QuestManager.Instance?.SetQuest_TakeARest(); break;
            case Quest.TakeSomeTimeOff: QuestManager.Instance?.SetQuest_TakeSomeTimeOff(); break;
            case Quest.None: break;
        }
        _onFightComplete?.Invoke(true);
    }

}