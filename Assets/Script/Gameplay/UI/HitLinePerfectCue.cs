using UnityEngine;

public class HitLinePerfectCue : MonoBehaviour
{
    public OneKeyUISpawner spawner;
    public SpriteRenderer glow;

    public Color good  = new(1f,0.9f,0.4f,1f);
    public Color great = new(0.6f,0.8f,1f,1f);
    public Color perfect = new(0.6f,1f,0.6f,1f);

    [Header("Curva e intensidade")]
    public float maxAlpha = 0.95f;
    public AnimationCurve alphaByDiff = AnimationCurve.EaseInOut(0,1, 1,0);

    [Header("Largura (fator relativo)")]
    public Vector2 widthRange = new Vector2(1f, 1.18f); // 1 = tamanho base
    public float flashHardWindow = 0.015f;

    [Header("Editor")]
    public bool onlyAffectInPlayMode = true; // evita mexer no editor

    // --- base capturada no Awake/ContextMenu
    Vector3 _baseScale = Vector3.one;
    Vector2 _baseSize = Vector2.one;

    void Awake() {
        if (!glow) glow = GetComponentInChildren<SpriteRenderer>();
        if (!glow) return;
        _baseScale = glow.transform.localScale;
        _baseSize  = glow.size; // só tem efeito em Sliced/Tiled
    }

    [ContextMenu("Capture Base From Current")]
    void CaptureBase() {
        if (!glow) return;
        _baseScale = glow.transform.localScale;
        _baseSize  = glow.size;
    }

    void LateUpdate() {
        if (!glow || !spawner) return;
        if (onlyAffectInPlayMode && !Application.isPlaying) return;

        if (!spawner.TryGetNearestDiff(out var diff, out _)) {
            SetAlpha(0f);
            return;
        }

        float goodW = spawner.good, greatW = spawner.great, perfW = spawner.perfect;
        if (diff > goodW) { SetAlpha(0f); return; }

        // 0 longe -> 1 no hit
        float t = Mathf.InverseLerp(goodW, 0f, diff);

        // cor por faixa
        Color c = diff <= perfW ? perfect : (diff <= greatW ? great : good);
        float a = alphaByDiff.Evaluate(t) * maxAlpha;
        if (diff <= flashHardWindow) a = maxAlpha; // “estalo” no zero
        c.a = a;
        glow.color = c;

        // largura relativa ao tamanho que você definiu no editor
        float sx = Mathf.Lerp(widthRange.x, widthRange.y, t);

        if (glow.drawMode == SpriteDrawMode.Simple) {
            // preserva o que você setou no editor
            glow.transform.localScale = new Vector3(_baseScale.x * sx, _baseScale.y, _baseScale.z);
        } else {
            // Sliced/Tiled: use size, não scale
            glow.size = new Vector2(_baseSize.x * sx, _baseSize.y);
        }
    }

    void SetAlpha(float a) {
        var col = glow.color; col.a = a; glow.color = col;
    }
}