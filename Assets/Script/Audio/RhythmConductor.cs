using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RhythmConductor : MonoBehaviour {
    public AudioClip song;
    [Range(-0.2f, 0.2f)] public float latencyOffset = 0f;

    private AudioSource _src;
    private double _songStartDsp;
    private bool _playing;

    private bool _paused;
    private float _frozenTime;

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

    void Update() {
        if (!_playing) return;

        if (_paused) {
            SongTimeSec = _frozenTime;
            return;
        }

        SongTimeSec = (float)(AudioSettings.dspTime - _songStartDsp) + latencyOffset;
    }

    public void Pause() {
        if (_paused) return;
        _paused = true;
        _frozenTime = SongTimeSec;
        if (_src && _src.isPlaying) _src.Pause();
    }

    public void Resume() {
        if (!_paused) return;
        _paused = false;
        _songStartDsp = AudioSettings.dspTime - (_frozenTime - latencyOffset);
        if (_src && _src.clip) _src.UnPause();
    }

    public void Restart(float leadInSeconds = 0.8f) {
        _paused = false;
        _playing = false;
        if (_src) _src.Stop();
        if (_src && _src.clip) PlayScheduled(leadInSeconds);
        else StartNoAudio(leadInSeconds);
    }
}
