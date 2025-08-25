using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OneKeyUISpawner : MonoBehaviour
{
    [Header("Refs")]
    public RhythmConductor conductor;
    public BeatmapLoader beatmapLoader;
    public Transform enemiesParent;
    public EnemyNote enemyPrefab;
    public Transform hitLine;
    public ScoreManager score;
    public InputActionProperty hitAction;

    [Header("Beatmap")]
    public float approachTime = 1.1f;
    public float fallbackBpm = 120f;
    public int fallbackBeats = 32;

    [Header("Gameplay")]
    public float unitsPerSecond = 6f;
    public float laneY = 0f;
    public float perfect = 0.05f;
    public float great   = 0.10f;
    public float good    = 0.15f;

    [Header("Testing")]
    public bool noAudio = true;
    public bool autoPlayer = false;
    public float autoWindow = 0.02f;

    private Beatmap _map;
    private SimplePool<EnemyNote> _pool;
    private readonly List<EnemyNote> _active = new List<EnemyNote>();
    private int _nextIndex;
    private float _hitX;
    private bool _running;
    
    [Header("Auto – Distribuição de acertos")]
    public bool autoUseDistribution = true;
    [Range(0,100)] public int autoPerfectPct = 60;
    [Range(0,100)] public int autoGreatPct   = 25;
    [Range(0,100)] public int autoGoodPct    = 10;
    [Range(0,100)] public int autoMissPct    = 5;
    private EnemyNote _autoMissLock;

    void Awake()
    {
        _pool = new SimplePool<EnemyNote>(enemyPrefab, 64, enemiesParent);
    }

    void OnEnable()
    {
        hitAction.action.performed += OnHit;
        hitAction.action.Enable();
    }

    void OnDisable()
    {
        hitAction.action.performed -= OnHit;
        hitAction.action.Disable();
    }

    void Start()
    {
        StartChart();
    }

    public void StartChart()
    {
        _hitX = hitLine.position.x;
        laneY = hitLine.position.y;

        ClearActive();
        _nextIndex = 0;
        _running = false;

        _map = beatmapLoader ? beatmapLoader.Load() : null;
        if (_map == null) {
            _map = GenerateGrid(fallbackBpm, fallbackBeats, 1.0f);
            _map.approachTime = approachTime;
        }

        var cam = Camera.main;
        if (cam) {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            float leftEdge  = cam.transform.position.x - halfW;
            float rightEdge = cam.transform.position.x + halfW;

            float sign = Mathf.Sign(unitsPerSecond);
            float screenEdge   = sign >= 0f ? rightEdge : leftEdge;
            float spawnPadding = 1.0f;

            float minDistance  = Mathf.Abs(screenEdge - _hitX) + spawnPadding;
            float minApproach  = minDistance / Mathf.Max(Mathf.Abs(unitsPerSecond), 0.0001f);

            _map.approachTime = Mathf.Max(_map.approachTime, minApproach);
        }

        if (noAudio) conductor.StartNoAudio(0.8f);
        else         conductor.PlayScheduled(1.0f);

        _running = true;
    }

    void Update() {
        if (!_running || _map == null) return;

        float songTime = conductor.SongTimeSec;

        while (_nextIndex < _map.notes.Count) {
            var n = _map.notes[_nextIndex];
            
            if (songTime >= n.t - _map.approachTime) {
                var e = _pool.Get();
                e.transform.localScale = Vector3.one;
                e.Init(conductor, _hitX, laneY, n.t + _map.offset, _map.approachTime, unitsPerSecond);
                e.OnDespawn = Despawn;
                _active.Add(e);
                _nextIndex++;
            } else break;
        }

        if (autoPlayer) TryAuto(songTime);
    }
    
    private void OnHit(InputAction.CallbackContext ctx) {
        TryHitAt(conductor.SongTimeSec + (_map?.offset ?? 0f));
    }
    
    private void TryAuto(float songTime) {
        if (_active.Count == 0) return;

        EnemyNote best = null;
        float bestDiff = float.MaxValue;
        float tNowBase = songTime + (_map?.offset ?? 0f);

        foreach (var n in _active) {
            if (n.judged) continue;
            if (_autoMissLock == n) continue;

            float d = Mathf.Abs(n.targetTime - tNowBase);
            if (d < bestDiff) { best = n; bestDiff = d; }
        }
        if (best == null) return;

        if (bestDiff <= autoWindow) {

            if (!autoUseDistribution) {
                TryHitAt(tNowBase);
                return;
            }

            string bucket = PickAutoBucket();

            if (bucket == "MISS") {
                _autoMissLock = best;
                return;
            }

            float targetAbsDiff;
            if (bucket == "PERFECT") {
                targetAbsDiff = Random.Range(0f, perfect * 0.90f);
            } else if (bucket == "GREAT") {
                targetAbsDiff = Random.Range(perfect + 0.001f, great * 0.95f);
            } else {
                targetAbsDiff = Random.Range(great + 0.001f, good * 0.95f);
            }

            float sign = (Random.value < 0.5f) ? -1f : 1f;
            float tNow = tNowBase + sign * targetAbsDiff;

            TryHitAt(tNow);
        }
    }
    
    private void TryHitAt(float tNow) {
        EnemyNote best = null;
        float bestDiff = float.MaxValue;

        for (int i = 0; i < _active.Count; i++) {
            var n = _active[i];
            if (n.judged) continue;
            float d = Mathf.Abs(n.targetTime - tNow);
            if (d < bestDiff) { best = n; bestDiff = d; }
        }

        if (best == null) {
            score?.RegisterMiss();
            return;
        }

        if (bestDiff <= perfect) {
            best.judged = true; best.missed = false;
            score?.RegisterHit("PERFECT", bestDiff);
            Despawn(best);
        } else if (bestDiff <= great) {
            best.judged = true; best.missed = false;
            score?.RegisterHit("GREAT", bestDiff);
            Despawn(best);
        } else if (bestDiff <= good) {
            best.judged = true; best.missed = false;
            score?.RegisterHit("GOOD", bestDiff);
            Despawn(best);
        } else {
            score?.RegisterMiss();
        }
    }
    
    private void Despawn(EnemyNote n) {
        if (n == _autoMissLock) _autoMissLock = null;
        
        if (n.missed) score?.RegisterMiss();

        _active.Remove(n);
        _pool.Return(n);
    }
    
    private void ClearActive() {
        foreach (var n in _active) _pool.Return(n);
        _active.Clear();
    }
    
    private Beatmap GenerateGrid(float bpm, int beats, float startAt) {
        var map = new Beatmap { offset = 0f, approachTime = approachTime };
        float dt = 60f / bpm;
        float t = startAt;
        for (int i = 0; i < beats; i++) {
            map.notes.Add(new NoteData { t = t });
            t += dt;
        }
        return map;
    }
    
    private string PickAutoBucket() {
        int sum = autoPerfectPct + autoGreatPct + autoGoodPct + autoMissPct;
        if (sum <= 0) return "PERFECT";
        int r = Random.Range(1, sum + 1);

        if ((r -= autoPerfectPct) <= 0) return "PERFECT";
        if ((r -= autoGreatPct)   <= 0) return "GREAT";
        if ((r -= autoGoodPct)    <= 0) return "GOOD";
        return "MISS";
    }
}
