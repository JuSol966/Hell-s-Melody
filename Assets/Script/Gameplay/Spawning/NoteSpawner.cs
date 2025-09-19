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

    [Header("Visual")]
    [Range(0.1f, 3f)] public float noteScale = 0.6f;

    [Header("Audio (opcional)")]
    [Tooltip("Se ligado, o Spawner inicia o Conductor ao começar o chart. Por padrão, deixe OFF e deixe o GameFlow cuidar disso.")]
    public bool startConductorOnStartChart = false;
    public bool noAudio = true;
    public float leadInSeconds = 0.8f;

    private Beatmap _map;
    private SimplePool<EnemyNote> _pool;
    private readonly List<EnemyNote> _active = new List<EnemyNote>();
    private int _nextIndex;
    private float _hitX;
    private bool _running;

    void Awake() {
        _pool = new SimplePool<EnemyNote>(enemyPrefab, 64, enemiesParent);
    }

    void Update() {
        if (!_running || _map == null) return;

        float songTime = conductor ? conductor.SongTimeSec : 0f;

        // Spawn quando faltar approachTime para cada nota
        while (_nextIndex < _map.notes.Count) {
            var n = _map.notes[_nextIndex];
            if (songTime >= n.t - _map.approachTime) {
                var e = _pool.Get();
                e.transform.localScale = Vector3.one * noteScale;
                e.Init(conductor, _hitX, laneY, n.t + _map.offset, _map.approachTime, unitsPerSecond);
                e.OnDespawn = Despawn;
                _active.Add(e);
                _nextIndex++;
            } else break;
        }
    }

    public void StartChart() {
        if (!conductor) { Debug.LogError("[NoteSpawner] Conductor não atribuído."); enabled = false; return; }
        if (!enemyPrefab) { Debug.LogError("[NoteSpawner] Enemy prefab não atribuído."); enabled = false; return; }
        if (!hitLine) { Debug.LogError("[NoteSpawner] HitLine não atribuído."); enabled = false; return; }

        _hitX = hitLine.position.x;
        laneY = hitLine.position.y;

        ClearActive();
        _nextIndex = 0;
        _running = false;

        _map = beatmapLoader ? beatmapLoader.Load() : null;
        if (_map == null || _map.notes == null || _map.notes.Count == 0) {
            Debug.LogError("[NoteSpawner] Beatmap obrigatório e com notas."); enabled = false; return;
        }

        // ordenar por tempo (seguro)
        _map.notes.Sort((a, b) => a.t.CompareTo(b.t));

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

        // opcional: se quiser que o spawner inicie o áudio (por padrão deixe OFF)
        if (startConductorOnStartChart) {
            if (noAudio) conductor.StartNoAudio(leadInSeconds);
            else         conductor.PlayScheduled(leadInSeconds);
        }

        _running = true;
    }

    private void Despawn(EnemyNote n) {
        if (n.missed) score?.RegisterMiss();
        _active.Remove(n);
        _pool.Return(n);
    }

    private void ClearActive() {
        for (int i = 0; i < _active.Count; i++) _pool.Return(_active[i]);
        _active.Clear();
    }

    // Acesso para o Judge
    public IReadOnlyList<EnemyNote> ActiveNotes => _active;
}
