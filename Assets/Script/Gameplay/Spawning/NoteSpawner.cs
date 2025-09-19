using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    [Header("Refs")]
    public RhythmConductor conductor;
    public BeatmapLoader beatmapLoader;
    public Transform enemiesParent;
    public EnemyNote enemyPrefab;
    public Transform hitLine;
    public ScoreManager score;

    [Header("Gameplay")]
    public float unitsPerSecond = 6f;
    public float laneY = 0f;

    [Header("Testing")]
    public bool noAudio = true;

    [Header("Visual")]
    [Range(0.1f, 3f)] public float noteScale = 0.6f;

    private Beatmap _map;
    private SimplePool<EnemyNote> _pool;
    private readonly List<EnemyNote> _active = new List<EnemyNote>();
    private int _nextIndex;
    private float _hitX;
    private bool _running;

    void Awake() {
        _pool = new SimplePool<EnemyNote>(enemyPrefab, 64, enemiesParent);
    }

    void Start() {
        StartChart();
    }

    void Update() {
        if (!_running || _map == null) return;

        float songTime = conductor.SongTimeSec;

        // Spawn quando faltar "approachTime"
        while (_nextIndex < _map.notes.Count) {
            var n = _map.notes[_nextIndex];
            if (songTime >= n.t - _map.approachTime) {
                var e = _pool.Get();
                e.transform.localScale = Vector3.one * noteScale;
                e.Init(conductor, _hitX, laneY, n.t + _map.offset, _map.approachTime, unitsPerSecond);
                e.OnDespawn  = Despawn;
                _active.Add(e);
                _nextIndex++;
            } else break;
        }
        // Nenhum julgamento aqui — tudo é feito no HitLineJudge
    }

    public void StartChart() {
        _hitX = hitLine.position.x;
        laneY = hitLine.position.y;

        ClearActive();
        _nextIndex = 0;
        _running = false;

        _map = beatmapLoader ? beatmapLoader.Load() : null;
        if (_map == null || _map.notes == null || _map.notes.Count == 0) {
            Debug.LogError("[NoteSpawner] Beatmap obrigatório. Atribua um TextAsset válido no BeatmapLoader.");
            enabled = false;
            return;
        }

        // ordenar por tempo (seguro)
        _map.notes.Sort((a,b) => a.t.CompareTo(b.t));

        // clamp de approach p/ nascer fora da tela
        var cam = Camera.main;
        if (cam) {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            float rightEdge = cam.transform.position.x + halfW;
            float spawnPadding = 1.0f;
            float minDistance = Mathf.Abs(rightEdge - _hitX) + spawnPadding;
            float minApproach = minDistance / Mathf.Max(Mathf.Abs(unitsPerSecond), 0.0001f);
            _map.approachTime = Mathf.Max(_map.approachTime, minApproach);
        }

        if (noAudio) conductor.StartNoAudio(0.8f);
        else         conductor.PlayScheduled(1.0f);

        _running = true;
    }

    private void Despawn(EnemyNote n) {
        if (n.missed) score?.RegisterMiss(); // miss por tempo é contabilizado aqui
        _active.Remove(n);
        _pool.Return(n);
    }

    private void ClearActive() {
        foreach (var n in _active) _pool.Return(n);
        _active.Clear();
    }

    // Para o HitLineJudge
    public System.Collections.Generic.IReadOnlyList<EnemyNote> ActiveNotes => _active;
}
