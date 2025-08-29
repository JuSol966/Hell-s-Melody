using UnityEngine;

public class EnemyNote : MonoBehaviour
{
    [Header("Runtime")] public float targetTime;
    public bool judged;
    public bool missed;
    public System.Action<EnemyNote> OnDespawn;

    [Header("Tuning")] public float unitsPerSecond = 6f;
    public float approachTime = 1.1f;
    public float missWindow = 0.15f;

    [Header("Lane/Refs")] public float laneY = 0f;
    public float hitlineX = 0f;

    private RhythmConductor _conductor;

    public void Init(RhythmConductor c, float hitX, float y, float t, float approach, float ups)
    {
        _conductor = c;
        hitlineX = hitX;
        laneY = y;
        targetTime = t;
        approachTime = approach;
        unitsPerSecond = ups;
        judged = false;
        missed = false;

        float songTime = _conductor.SongTimeSec;
        float timeToHit = targetTime - songTime;
        transform.position = new Vector3(hitlineX + timeToHit * unitsPerSecond, laneY, 0);
    }

    void Update()
    {
        if (_conductor == null) return;
        
        float songTime = _conductor.SongTimeSec;

        if (!judged && songTime >= targetTime + missWindow)
        {
            judged = true;
            missed = true;
            OnDespawn?.Invoke(this);
            return;
        }
        
        float timeToHit = targetTime - songTime;
        float x = hitlineX + timeToHit * unitsPerSecond;
        transform.position = new Vector3(x, laneY, 0);
    }
}
