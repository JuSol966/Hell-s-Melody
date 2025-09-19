using UnityEngine;

public class NoteApproachCue : MonoBehaviour
{
    public enum ScaleSpace { RelativeToParent, World }

    [Header("Refs")]
    public EnemyNote note;
    public SpriteRenderer ring;

    [Header("Escala (crescendo)")]
    public float startScale = 0.35f;
    public float endScale   = 1.00f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0,0, 1,1);

    [Header("Cor opcional")]
    public Gradient colorOverProgress;

    [Header("Após o hit")]
    public bool holdAtHit   = true;
    public float holdTime   = 0.03f;
    public bool fadeAfterHit = true;
    public float fadeTime   = 0.08f;

    [Header("Config")]
    public ScaleSpace scaleSpace = ScaleSpace.World;
    public bool disableWhenJudged = true;

    // estados
    Vector3 _baseLocal;
    Vector3 _baseWorld;
    float _holdLeft=-1f, _fadeLeft=-1f;

    void Reset() {
        if (!ring) ring = GetComponent<SpriteRenderer>();
        if (!note) note = GetComponentInParent<EnemyNote>();
    }

    void Awake() {
        if (!ring) return;
        _baseLocal = ring.transform.localScale;
        _baseWorld = ring.transform.lossyScale;
    }

    void OnEnable() {
        _holdLeft = _fadeLeft = -1f;
        if (ring) ring.enabled = true;
    }

    void LateUpdate() {
        if (!note || !ring) return;

        if (disableWhenJudged && note.judged) { ring.enabled = false; return; }
        
        float songNow   = note.SongTimeNow();
        float timeToHit = note.targetTime - songNow;

        if (timeToHit <= 0f) { PostHitSequence(); return; }
        
        float prog = Mathf.InverseLerp(
            note.approachTime, 0f,
            Mathf.Clamp(timeToHit, 0f, note.approachTime)
        );

        float k = scaleCurve.Evaluate(prog);
        float s = Mathf.Lerp(startScale, endScale, k);

        ApplyScale(s);

        if (colorOverProgress != null && colorOverProgress.colorKeys.Length > 0) {
            var col = colorOverProgress.Evaluate(prog);
            col.a = ring.color.a;
            ring.color = col;
        }   

        _holdLeft = _fadeLeft = -1f;
    }

    void ApplyScale(float factor) {
        if (scaleSpace == ScaleSpace.RelativeToParent) {
            ring.transform.localScale = _baseLocal * factor;
            return;
        }
        
        Vector3 desiredWorld = _baseWorld * factor;
        var p = ring.transform.parent;
        if (p == null) {
            ring.transform.localScale = desiredWorld;
        } else {
            Vector3 parentWorld = p.lossyScale;
            ring.transform.localScale = new Vector3(
                SafeDiv(desiredWorld.x, parentWorld.x),
                SafeDiv(desiredWorld.y, parentWorld.y),
                SafeDiv(desiredWorld.z, parentWorld.z)
            );
        }
    }

    static float SafeDiv(float a, float b) => b == 0f ? 0f : a / b;

    void PostHitSequence() {
        if (_holdLeft < 0f && _fadeLeft < 0f) {
            _holdLeft = holdAtHit ? holdTime : 0f;
            _fadeLeft = fadeAfterHit ? fadeTime : 0f;
        }
        if (_holdLeft > 0f) {
            _holdLeft -= Time.deltaTime;
            ApplyScale(endScale);
            return;
        }
        if (_fadeLeft > 0f) {
            _fadeLeft -= Time.deltaTime;
            float a = Mathf.Clamp01(_fadeLeft / Mathf.Max(0.0001f, fadeTime));
            var c = ring.color; c.a = a; ring.color = c;
            if (_fadeLeft <= 0f) ring.enabled = false;
        }
    }
}
