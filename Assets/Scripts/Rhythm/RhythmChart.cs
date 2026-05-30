using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewChart", menuName = "Rhythm/Chart")]
public class RhythmChart : ScriptableObject
{
    public float bpm;
    public float offset;            // lead-in seconds before first note spawns
    public float interruptTime;     // seconds at which the song gets interrupted
    public bool autoGenerate;       // if true, ignores notes list and generates randomly
    public List<RhythmNote> notes;  // used only if autoGenerate is false
}