using UnityEngine;

[CreateAssetMenu(fileName = "NewNote", menuName = "Rhythm/Note")]
public class RhythmNote : ScriptableObject
{
    [Range(0, 3)] public int lane; // 0=D, 1=F, 2=J, 3=K
    public float beatTime;         // seconds from song start when this note should be hit
}