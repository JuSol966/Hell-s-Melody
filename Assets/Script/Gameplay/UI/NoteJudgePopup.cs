using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class NoteJudgePopup : MonoBehaviour
{
    public TMP_Text text;
    public float rise = 0.6f;
    public float life = 0.45f;
    public float fade = 0.20f;

    public Color perfect = new Color(0.6f, 1f, 0.6f, 1f);
    public Color great   = new Color(0.6f, 0.8f, 1f, 1f);
    public Color good    = new Color(1f, 0.95f, 0.6f, 1f);
    public Color miss    = new Color(1f, 0.5f, 0.5f, 1f);

    private Vector3 _startLocal;
    private float _t;

    public float TotalTime => life + fade + 0.05f;

    void Reset()
    {
        text = GetComponent<TMP_Text>();
    }

    void OnEnable() { _t = 0f; }

    void Update()
    {
        if (!gameObject.activeSelf) return;
        _t += Time.deltaTime;

        // sobe em local space
        float y = Mathf.SmoothStep(0f, rise, Mathf.Clamp01(_t / life));
        transform.localPosition = _startLocal + new Vector3(0, y, 0);

        // fade
        if (_t > life)
        {
            float u = Mathf.InverseLerp(life, life + fade, _t);
            var c = text.color; c.a = 1f - u; text.color = c;
            if (_t >= life + fade)
            {
                transform.localPosition = _startLocal;
                gameObject.SetActive(false);
            }
        }
    }

    public void Play(string label)
    {
        if (!text) text = GetComponent<TMP_Text>();
        gameObject.SetActive(true);
        _startLocal = transform.localPosition;
        text.text   = label;
        text.color  = ColorFor(label);
        _t = 0f;
    }

    public void SetSortingFromRenderer(Renderer reference, int orderOffset = 1)
    {
        var myRenderer = GetComponent<Renderer>();
        if (!myRenderer || !reference) return;

        myRenderer.sortingLayerID = reference.sortingLayerID;
        myRenderer.sortingOrder   = reference.sortingOrder + orderOffset;
    }

    public void PlayAtWorld(string label, Transform worldParent, Vector3 worldPosition, bool resetLocalScaleToOne = true)
    {
        if (worldParent) transform.SetParent(worldParent, true);
        else             transform.SetParent(null, true);

        transform.position = worldPosition;
        transform.localRotation = Quaternion.identity;
        if (resetLocalScaleToOne) transform.localScale = Vector3.one;

        _startLocal = transform.localPosition;

        Play(label);
    }


    private Color ColorFor(string label) =>
        label == "PERFECT" ? perfect :
        label == "GREAT"   ? great   :
        label == "GOOD"    ? good    : miss;
}
