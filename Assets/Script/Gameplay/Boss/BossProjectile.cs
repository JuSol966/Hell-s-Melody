using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [Header("Move")]
    public float speed = 7f;

    public Vector3 source;
    public Vector3 impactPoint;
    public Vector3 finalPoint;

    [Header("Visual")]
    public NoteJudgePopup judge;
    public Renderer[]     hideOnJudge;
    public NoteOutlineShaderDriver outline;

    [Header("Judge popup world")]
    public Transform judgeWorldParent;
    public Vector3   judgeWorldOffset = new(0f, 0.5f, 0f);
    public bool      scaleJudgeOffset = true;

    [Header("Timing")]
    public float overshootUnits = 0f;
    public float lateMissWindow = 0.08f;

    // runtime
    private RhythmConductor _conductor;
    private bool   _done;
    private float  _spawnSongTime;

    public float   spawnSongTime   => _spawnSongTime;
    public float   approachTime    { get; private set; }
    public float   impactTime      { get; private set; }
    public bool    IsResolved      => _done;
    public RhythmConductor Conductor => _conductor;

    public System.Action<BossProjectile,bool> onDone;

    public void Init(RhythmConductor c, Vector3 from, Vector3 to, float spawnTime, float spd, float scale)
    {
        _conductor = c;
        source = from;
        impactPoint = to;
        transform.position = from;
        transform.localScale = Vector3.one * scale;
        speed  = spd;
        _spawnSongTime = spawnTime;

        finalPoint = impactPoint;

        float distToImpact = Vector3.Distance(from, impactPoint);
        approachTime = Mathf.Max(0.0001f, distToImpact / Mathf.Max(0.0001f, speed));
        impactTime   = spawnTime + approachTime;

        _done = false;
        if (judge) judge.gameObject.SetActive(false);

        gameObject.SetActive(true);
    }

    void Update()
    {
        if (_done) return;
        if (_conductor != null && _conductor.IsPaused) return;

        UpdateStartTargets();

        Vector3 dir = (finalPoint - transform.position);
        float step = speed * Time.deltaTime;

        if (dir.magnitude <= step) {
            transform.position = finalPoint;
            Finish(false);
            return;
        }
        transform.position += dir.normalized * step;
    }

    void UpdateStartTargets()
    {
        Vector3 dirImpact = (impactPoint - source).normalized;
        finalPoint = impactPoint + dirImpact * Mathf.Max(0f, overshootUnits);
    }

    public void Parry(string label)
    {
        if (_done) return;

        if (hideOnJudge != null) foreach (var r in hideOnJudge) if (r) r.enabled = false;

        if (judge)
        {
            Transform parent = judgeWorldParent ? judgeWorldParent : null;
            judge.transform.SetParent(parent, true);

            Vector3 off = judgeWorldOffset;
            if (scaleJudgeOffset) off *= transform.lossyScale.x;

            judge.transform.position      = transform.position + off;
            judge.transform.localRotation = Quaternion.identity;
            judge.transform.localScale    = Vector3.one;

            judge.Play(label);
        }

        Finish(true);
    }

    void Finish(bool parried) {
        if (_done) return;
        _done = true;
        onDone?.Invoke(this, parried);
        Destroy(gameObject);
    }
}
