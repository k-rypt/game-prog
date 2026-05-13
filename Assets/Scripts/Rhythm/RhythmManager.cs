using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RhythmManager : MonoBehaviour
{
    public static RhythmManager Instance;

    [Header("Chart")]
    public RhythmChart chart;

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

    private readonly KeyCode[] _laneKeys = { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K };

    private float _songTime = -1f;
    private bool _running = false;
    private bool _interrupted = false;
    private int _nextNoteIndex = 0;
    private List<NoteObject> _activeNotes = new List<NoteObject>();
    private List<RhythmNote> _generatedNotes = new List<RhythmNote>();
    private int _score = 0;
    private int _combo = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        FindObjectOfType<PlayerMovement>().enabled = false;
        StartGame();
    }

    public void StartGame()
    {
        if (_running) return;

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

                if (timeUntilHit <= travelTime)
                {
                    SpawnNote(note);
                    _nextNoteIndex++;
                }
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
            if (note == null) continue;
            if (note.lane != lane) continue;
            if (note.IsMissed()) continue;  // added
            float distance = note.GetVisualDistanceFromHitZone();
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = note;
            }
        }

        if (closest == null) return;

        if (closestDistance <= perfect)
            RegisterHit(closest, true);
        else if (closestDistance <= good)
            RegisterHit(closest, false);
    }

    void RegisterHit(NoteObject note, bool perfect)
    {
        _activeNotes.Remove(note);
        Destroy(note.gameObject);

        if (perfect)
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

        if (health <= 0f) StartCoroutine(Fail());
    }

    public void RegisterMiss()
    {
        health = Mathf.Clamp01(health - missPenalty);
        _combo = 0;
        hud.ShowJudgement("MISS");
        hud.UpdateHealth(health);
        hud.UpdateScore(_score, _combo);

        if (health <= 0f) StartCoroutine(Fail());
    }

    IEnumerator Interrupt(){
        _running = false;

        foreach (NoteObject n in _activeNotes)
        {
            if (n != null)
            {
                n.enabled = false;
            }
        }

        yield return new WaitForSeconds(1.5f); 

        foreach (NoteObject n in _activeNotes) 
            if (n != null) Destroy(n.gameObject); 
        _activeNotes.Clear(); 

        hud.Hide();

        yield return new WaitForSeconds(0.3f);

        DialogueManager.Instance.StartDialogue(interruptDialogue.lines, OnInterruptDialogueEnd);
    }

    IEnumerator Fail()
    {
        _running = false;
        hud.ShowResult(false);
        yield return new WaitForSeconds(2f);
        hud.Hide();
    }

    void OnInterruptDialogueEnd()
    {
        FindObjectOfType<PlayerMovement>().enabled = true;
    }
}