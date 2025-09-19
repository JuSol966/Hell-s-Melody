using UnityEngine;
using System.Reflection;

[RequireComponent(typeof(SpriteRenderer))]
public class NoteOutlineShaderDriver : MonoBehaviour
{
    [Header("Refs")] public EnemyNote note; // arraste o EnemyNote da própria nota
    public SpriteRenderer sr; // SpriteRenderer do corpo (auto no Reset)

    [Header("Ramp de intensidade (0=spawn, 1=hit)")]
    public AnimationCurve cueOverProgress = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Range(0, 1)] public float minCue = 0.0f; // força mínima ao nascer
    [Range(0, 1)] public float maxCue = 1.0f; // força máxima no hit

    [Header("Cor ao longo do progresso")] public Gradient colorOverProgress;

    [Header("Espessura ao longo do progresso (px)")]
    public int minWidthPx = 0;

    public int maxWidthPx = 2;

    [Header("Near-perfect flash (opcional)")]
    public bool flashNearPerfect = true;

    public float flashWindow = 0.030f; // +- segundos
    public float flashSpeedHz = 16f;
    [Range(0, 1)] public float flashAmp = 0.35f;

    [Header("Parâmetros do material")] [Range(0, 1)]
    public float alphaThreshold = 0.12f; // 0.10–0.20 bom

    MaterialPropertyBlock _mpb;
    Shader _shaderBI;
    FieldInfo _conductorFI; // acesso ao _conductor privado da nota

    void Reset()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Awake()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        _mpb = new MaterialPropertyBlock();

        // Garante shader Built-in aplicado
        _shaderBI = Shader.Find("Unlit/Outline");
        if (_shaderBI == null)
        {
            Debug.LogError("Shader 'Unlit/Outline' não encontrado. Crie-o e aplique em um Material.");
        }
        else if (sr.sharedMaterial == null || sr.sharedMaterial.shader != _shaderBI)
        {
            sr.material = new Material(_shaderBI);
        }

        // Gradiente padrão (se não setado no Inspector)
        if (colorOverProgress == null || colorOverProgress.colorKeys.Length == 0)
        {
            var g = new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.75f, 0.75f, 0.75f), 0f), // longe: cinza
                new GradientColorKey(new Color(1f, 0.9f, 0.4f), 0.6f), // perto: amarelo
                new GradientColorKey(new Color(0.6f, 0.8f, 1f), 0.85f), // bem perto: azul
                new GradientColorKey(new Color(0.6f, 1f, 0.6f), 1f), // hit: verde
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

        // tempo restante p/ o hit (positivo antes do hit, ~0 no hit)
        float songNow = note ? note.SongTimeNow() : 0f;
        float timeToHit = note.targetTime - songNow;

        // progresso desde o spawn (approach) até o hit
        float prog = Mathf.InverseLerp(note.approachTime, 0f, Mathf.Clamp(timeToHit, 0f, note.approachTime));
        
        prog = Mathf.Clamp01(prog);

        // curva -> força base
        float cueBase = cueOverProgress.Evaluate(prog);
        float cue = Mathf.Lerp(minCue, maxCue, cueBase);

        // flash perto do PERFECT (só quando ainda não julgada)
        if (flashNearPerfect && !note.judged && Mathf.Abs(timeToHit) <= flashWindow)
        {
            float pulse = (Mathf.Sin(Time.time * Mathf.PI * 2f * flashSpeedHz) * 0.5f + 0.5f) * flashAmp;
            cue = Mathf.Clamp01(cue + pulse);
        }

        // cor e largura por progresso
        Color col = colorOverProgress.Evaluate(prog);
        int width = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(minWidthPx, maxWidthPx, cueBase)), 0, 4);

        // se já julgada, apaga
        if (note.judged)
        {
            cue = 0f;
        }

        // envia para o material via PropertyBlock
        sr.GetPropertyBlock(_mpb);
        _mpb.SetFloat("_Cue", cue);
        _mpb.SetColor("_OutlineColor", col);
        _mpb.SetFloat("_OutlineWidth", width);
        _mpb.SetFloat("_AlphaThreshold", alphaThreshold);
        sr.SetPropertyBlock(_mpb);
    }
}