using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RhythmConductor : MonoBehaviour
{
    public AudioClip song;
    [Range(-0.25f, 0.25f)] public float latencyOffset = 0f;

    private AudioSource _src;
    private double _songStartDsp;
    private bool _playing;

    public float SongTimeSec { get; private set; }

    void Awake() {
        _src = GetComponent<AudioSource>();
        _src.playOnAwake = false;
        if (song != null) _src.clip = song;
    }

    public void PlayScheduled(float leadInSeconds = 1.0f) {
        if (song == null) {
            Debug.LogWarning("Sem AudioClip. Use StartNoAudio() para testar sem música.");
            return;
        }
        _songStartDsp = AudioSettings.dspTime + leadInSeconds;
        _src.PlayScheduled(_songStartDsp);
        _playing = true;
    }

    public void StartNoAudio(float leadInSeconds = 0.5f) {
        _songStartDsp = AudioSettings.dspTime + leadInSeconds;
        _playing = true;
    }

    void Update() {
        if (!_playing) return;
        SongTimeSec = (float)(AudioSettings.dspTime - _songStartDsp) + latencyOffset;
    }
}
