using UnityEngine;
using TMPro;

public class FloatingJudge2D : MonoBehaviour
{
    public TMP_Text text;
    public float rise = 0.8f;      // quanto sobe
    public float life = 0.5f;      // tempo visível
    public float fade = 0.25f;     // tempo de fade
    public Color perfectColor = new Color(0.6f, 1f, 0.6f, 1f);
    public Color greatColor   = new Color(0.6f, 0.8f, 1f, 1f);
    public Color goodColor    = new Color(1f, 0.95f, 0.6f, 1f);
    public Color missColor    = new Color(1f, 0.5f, 0.5f, 1f);

    private Vector3 _start;
    private float _t;
    
    public void Show(string label, Vector3 worldPos) {
        if (!text) text = GetComponentInChildren<TMP_Text>();
        _start = worldPos;
        transform.position = _start;
        text.text = label;
        text.color = ColorFor(label);
        _t = 0f;
        gameObject.SetActive(true);
    }

    void OnEnable()
    {
        _t = 0f;
    }

    void Update() {
        _t += Time.deltaTime;
        // sobe
        float y = Mathf.SmoothStep(0f, rise, Mathf.Clamp01(_t / life));
        transform.position = _start + new Vector3(0, y, 0);
        // fade
        if (_t > life) {
            float u = Mathf.InverseLerp(life, life + fade, _t);
            var c = text.color; c.a = 1f - u; text.color = c;
            if (_t >= life + fade) gameObject.SetActive(false);
        }
    }

    private Color ColorFor(string label) {
        switch (label) {
            case "PERFECT": return perfectColor;
            case "GREAT":   return greatColor;
            case "GOOD":    return goodColor;
            default:        return missColor;
        }
    }
}
