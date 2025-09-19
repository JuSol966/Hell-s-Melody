using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RhythmConductor : MonoBehaviour {
    public AudioClip song;
    [Range(-0.2f, 0.2f)] public float latencyOffset = 0f;

    AudioSource _src;
    double _songStartDsp;
    bool _playing;
    bool _paused;
    float _pausedAtSongTime;

    public float SongTimeSec { get; private set; }
    public bool IsPaused => _paused;

    void Awake() {
        _src = GetComponent<AudioSource>();
        _src.playOnAwake = false;
        if (song) _src.clip = song;
    }

    public void PlayScheduled(float leadInSeconds = 1.0f) {
        if (!song) { Debug.LogWarning("Sem AudioClip."); return; }
        _songStartDsp = AudioSettings.dspTime + leadInSeconds;
        _src.PlayScheduled(_songStartDsp);
        _playing = true;
        _paused = false;
    }

    public void StartNoAudio(float leadInSeconds = 0.5f) {
        _songStartDsp = AudioSettings.dspTime + leadInSeconds;
        _playing = true;
        _paused = false;
    }

    public void Pause() {
        if (!_playing || _paused) return;
        _pausedAtSongTime = (float)(AudioSettings.dspTime - _songStartDsp);
        if (_src && _src.isPlaying) _src.Pause();
        _paused = true;
        // congela o valor exposto
        SongTimeSec = _pausedAtSongTime + latencyOffset;
    }

    public void Resume() {
        if (!_paused) return;
        _paused = false;
        // reancora a origem para o tempo ficar contínuo
        _songStartDsp = AudioSettings.dspTime - _pausedAtSongTime;
        if (_src) _src.UnPause();
    }

    public void Restart(float leadInSeconds = 1.0f) {
        _paused = false; _playing = true;
        if (_src) { _src.Stop(); if (song) _src.clip = song; }
        _songStartDsp = AudioSettings.dspTime + leadInSeconds;
        if (song) _src.PlayScheduled(_songStartDsp);
    }

    void Update() {
        if (!_playing) return;
        if (_paused) { SongTimeSec = _pausedAtSongTime + latencyOffset; return; }
        SongTimeSec = (float)(AudioSettings.dspTime - _songStartDsp) + latencyOffset;
    }
}