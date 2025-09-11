using UnityEngine;
using TMPro;

public class NoteJudgeLabel : MonoBehaviour
{
    public TMP_Text text;                 // arraste o TMP deste GO
    public float rise = 0.6f;             // quanto sobe
    public float life = 0.45f;            // tempo visível
    public float fade = 0.20f;            // fade out

    public Color perfect = new Color(0.6f, 1f, 0.6f, 1f);
    public Color great   = new Color(0.6f, 0.8f, 1f, 1f);
    public Color good    = new Color(1f, 0.95f, 0.6f, 1f);
    public Color miss    = new Color(1f, 0.5f, 0.5f, 1f);

    private Vector3 _startLocal;
    private float _t;

    public float TotalTime => life + fade + 0.05f; // útil pra agendar despawn

    public void Play(string label) {
        if (!text) text = GetComponent<TMP_Text>();
        gameObject.SetActive(true);
        _startLocal = transform.localPosition;
        text.text = label;
        text.color = ColorFor(label);
        _t = 0f;
    }

    void OnEnable() { _t = 0f; }

    void Update() {
        if (!gameObject.activeSelf) return;
        _t += Time.deltaTime;

        // sobe suavemente em local Y
        float y = Mathf.SmoothStep(0f, rise, Mathf.Clamp01(_t / life));
        transform.localPosition = _startLocal + new Vector3(0, y, 0);

        // fade
        if (_t > life) {
            float u = Mathf.InverseLerp(life, life + fade, _t);
            var c = text.color; c.a = 1f - u; text.color = c;
            if (_t >= life + fade) {
                // reset e esconde para reutilizar no pool
                transform.localPosition = _startLocal;
                gameObject.SetActive(false);
            }
        }
    }

    private Color ColorFor(string label) =>
        label == "PERFECT" ? perfect :
        label == "GREAT"   ? great   :
        label == "GOOD"    ? good    : miss;
}
