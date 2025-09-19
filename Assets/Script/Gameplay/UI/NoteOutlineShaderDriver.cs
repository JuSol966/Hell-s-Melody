using UnityEngine;
using System.Reflection;

[RequireComponent(typeof(SpriteRenderer))]
public class NoteOutlineShaderDriver : MonoBehaviour
{
    [Header("Refs")] public EnemyNote note;
    public SpriteRenderer sr;

    [Header("Ramp de intensidade (0=spawn, 1=hit)")]
    public AnimationCurve cueOverProgress = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Range(0, 1)] public float minCue = 0.0f;
    [Range(0, 1)] public float maxCue = 1.0f;

    [Header("Cor ao longo do progresso")] public Gradient colorOverProgress;

    [Header("Espessura ao longo do progresso (px)")]
    public int minWidthPx = 0;

    public int maxWidthPx = 2;

    [Header("Near-perfect flash (opcional)")]
    public bool flashNearPerfect = true;

    public float flashWindow = 0.030f;
    public float flashSpeedHz = 16f;
    [Range(0, 1)] public float flashAmp = 0.35f;

    [Header("Parâmetros do material")] [Range(0, 1)]
    public float alphaThreshold = 0.12f;

    MaterialPropertyBlock _mpb;
    Shader _shaderBI;
    FieldInfo _conductorFI;

    void Reset()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Awake()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        _mpb = new MaterialPropertyBlock();

        _shaderBI = Shader.Find("Unlit/Outline");
        if (_shaderBI == null)
        {
            Debug.LogError("Shader 'Unlit/Outline' não encontrado. Crie-o e aplique em um Material.");
        }
        else if (sr.sharedMaterial == null || sr.sharedMaterial.shader != _shaderBI)
        {
            sr.material = new Material(_shaderBI);
        }

        if (colorOverProgress == null || colorOverProgress.colorKeys.Length == 0)
        {
            var g = new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.75f, 0.75f, 0.75f), 0f),
                new GradientColorKey(new Color(1f, 0.9f, 0.4f), 0.6f),
                new GradientColorKey(new Color(0.6f, 0.8f, 1f), 0.85f),
                new GradientColorKey(new Color(0.6f, 1f, 0.6f), 1f),
            };
            var a = new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f)
            };
            colorOverProgress = new Gradient();
            colorOverProgress.SetKeys(g, a);
        }

        _conductorFI = typeof(EnemyNote).GetField("_conductor",
            BindingFlags.NonPublic | BindingFlags.Instance);
    }

    void LateUpdate()
    {
        if (!note || !sr || sr.sharedMaterial == null) return;

        float songNow = note ? note.SongTimeNow() : 0f;
        float timeToHit = note.targetTime - songNow;

        float prog = Mathf.InverseLerp(note.approachTime, 0f, Mathf.Clamp(timeToHit, 0f, note.approachTime));
        
        prog = Mathf.Clamp01(prog);

        float cueBase = cueOverProgress.Evaluate(prog);
        float cue = Mathf.Lerp(minCue, maxCue, cueBase);

        if (flashNearPerfect && !note.judged && Mathf.Abs(timeToHit) <= flashWindow)
        {
            float pulse = (Mathf.Sin(Time.time * Mathf.PI * 2f * flashSpeedHz) * 0.5f + 0.5f) * flashAmp;
            cue = Mathf.Clamp01(cue + pulse);
        }

        Color col = colorOverProgress.Evaluate(prog);
        int width = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(minWidthPx, maxWidthPx, cueBase)), 0, 4);

        if (note.judged)
        {
            cue = 0f;
        }

        sr.GetPropertyBlock(_mpb);
        _mpb.SetFloat("_Cue", cue);
        _mpb.SetColor("_OutlineColor", col);
        _mpb.SetFloat("_OutlineWidth", width);
        _mpb.SetFloat("_AlphaThreshold", alphaThreshold);
        sr.SetPropertyBlock(_mpb);
    }
}