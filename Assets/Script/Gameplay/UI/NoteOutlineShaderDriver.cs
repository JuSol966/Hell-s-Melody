using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class NoteOutlineShaderDriver : MonoBehaviour
{
    [Header("Fonte (preencha UMA)")]
    public EnemyNote          note;
    public BossProjectile     projectile;

    [Header("Manual (p/ boss clash)")]
    public bool               manualMode = false;
    public RhythmConductor    manualConductor;
    public float              manualTargetTime;
    public float              manualApproachTime;
    public bool               manualResolved;

    [Header("Renderer alvo")]
    public SpriteRenderer sr;

    [Header("Ramp de intensidade (0=spawn, 1=hit)")]
    public AnimationCurve cueOverProgress = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Range(0, 1)] public float minCue = 0.0f;
    [Range(0, 1)] public float maxCue = 1.0f;

    [Header("Cor ao longo do progresso")]
    public Gradient colorOverProgress;

    [Header("Espessura ao longo do progresso (px)")]
    public int minWidthPx = 0;
    public int maxWidthPx = 2;

    [Header("Near-perfect flash")]
    public bool  flashNearPerfect = true;
    public float flashWindow      = 0.030f;
    public float flashSpeedHz     = 16f;
    [Range(0, 1)] public float flashAmp = 0.35f;

    [Header("Timing Windows (opcional p/ flash)")]
    public TimingWindows windows;
    public bool          useWindowsForFlash = true;

    [Header("Parâmetros do material")]
    [Range(0, 1)] public float alphaThreshold = 0.12f;

    MaterialPropertyBlock _mpb;
    Shader _shader;

    void Reset() {
        sr = GetComponent<SpriteRenderer>();
    }

    void Awake()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        _mpb = new MaterialPropertyBlock();

        _shader = Shader.Find("Unlit/Outline");
        if (_shader == null) {
            Debug.LogError("Shader 'Unlit/Outline' não encontrado. Crie-o e aplique em um Material.");
        } else if (sr.sharedMaterial == null || sr.sharedMaterial.shader != _shader) {
            sr.material = new Material(_shader);
        }

        if (colorOverProgress == null || colorOverProgress.colorKeys.Length == 0) {
            var g = new GradientColorKey[] {
                new GradientColorKey(new Color(0.75f, 0.75f, 0.75f), 0f),
                new GradientColorKey(new Color(1f,    0.9f,  0.4f),  0.6f),
                new GradientColorKey(new Color(0.6f,  0.8f,  1f),    0.85f),
                new GradientColorKey(new Color(0.6f,  1f,    0.6f),  1f),
            };
            var a = new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f)
            };
            colorOverProgress = new Gradient();
            colorOverProgress.SetKeys(g, a);
        }
    }

    void LateUpdate()
    {
        if (!sr || sr.sharedMaterial == null) return;

        // fonte de tempo
        var conductor = GetConductor();
        if (conductor == null) return;

        float songNow     = conductor.SongTimeSec;
        float targetTime  = GetTargetTime();
        float apTime      = GetApproachTime();
        bool  isResolved  = GetIsResolved();

        if (apTime <= 0.0001f) return;
        float timeToHit = targetTime - songNow;

        // progress 0..1 entre “spawn→hit”
        float prog = Mathf.InverseLerp(apTime, 0f, Mathf.Clamp(timeToHit, 0f, apTime));
        prog = Mathf.Clamp01(prog);

        float cueBase = cueOverProgress.Evaluate(prog);
        float cue     = Mathf.Lerp(minCue, maxCue, cueBase);

        // flash na janela “perfeita”
        float flashWin = (useWindowsForFlash && windows != null) ? windows.perfect : flashWindow;
        if (flashNearPerfect && !isResolved && Mathf.Abs(timeToHit) <= flashWin) {
            float pulse = (Mathf.Sin(Time.time * Mathf.PI * 2f * flashSpeedHz) * 0.5f + 0.5f) * flashAmp;
            cue = Mathf.Clamp01(cue + pulse);
        }

        if (isResolved) cue = 0f;

        Color col = colorOverProgress.Evaluate(prog);
        int   width = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(minWidthPx, maxWidthPx, cueBase)), 0, 4);

        sr.GetPropertyBlock(_mpb);
        _mpb.SetFloat("_Cue", cue);
        _mpb.SetColor("_OutlineColor", col);
        _mpb.SetFloat("_OutlineWidth", width);
        _mpb.SetFloat("_AlphaThreshold", alphaThreshold);
        sr.SetPropertyBlock(_mpb);
    }

    RhythmConductor GetConductor() {
        if (manualMode && manualConductor) return manualConductor;
        if (note && note != null)          return note.SongConductor;
        if (projectile && projectile != null) return projectile.Conductor;
        return null;
    }
    float GetTargetTime() {
        if (manualMode)                    return manualTargetTime;
        if (note && note != null)          return note.targetTime;
        if (projectile && projectile != null) return projectile.impactTime;
        return 0f;
    }
    float GetApproachTime() {
        if (manualMode)                    return manualApproachTime;
        if (note && note != null)          return note.approachTime;
        if (projectile && projectile != null) return projectile.approachTime;
        return 0f;
    }
    bool GetIsResolved() {
        if (manualMode)                    return manualResolved;
        if (note && note != null)          return note.judged;
        if (projectile && projectile != null) return projectile.IsResolved;
        return false;
    }

    public void ManualArm(RhythmConductor c, float targetTime, float approach)
    {
        manualMode         = true;
        manualConductor    = c;
        manualTargetTime   = targetTime;
        manualApproachTime = Mathf.Max(0.0001f, approach);
        manualResolved     = false;
    }
    public void ManualResolve() { manualResolved = true; }
}
