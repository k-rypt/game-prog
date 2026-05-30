using UnityEngine;

public class NoteObject : MonoBehaviour
{
    public int lane;
    public float targetBeatTime;

    private float _scrollSpeed;
    private float _spawnY;
    private float _hitZoneY;
    private bool _missed = false;

    public void Init(int lane, float targetBeatTime, float scrollSpeed, float spawnY, float hitZoneY)
    {
        this.lane = lane;
        this.targetBeatTime = targetBeatTime;
        _scrollSpeed = scrollSpeed;
        _spawnY = spawnY;
        _hitZoneY = hitZoneY;

        Vector3 pos = transform.localPosition;
        pos.y = _spawnY;
        transform.localPosition = pos;
    }

    void Update()
    {
        if (!_missed)
            transform.localPosition += Vector3.up * _scrollSpeed * Time.deltaTime;
        
        if (transform.localPosition.y > _hitZoneY + 300f && !_missed)
        {
            _missed = true;
            RhythmManager.Instance.RegisterMiss();
            Destroy(gameObject);
        }
    }
    
    public float GetVisualDistanceFromHitZone()  
    {
        return Mathf.Abs(transform.localPosition.y - _hitZoneY);
    }

    public bool IsMissed() => _missed;  
}