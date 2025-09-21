using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BossDirector : MonoBehaviour
{
    [Header("Refs")]
    public RhythmConductor     conductor;
    public PlayerHealth        player;
    public BossHealth          boss;
    public NoteSpawner         spawner;
    public InputActionProperty hitAction;
    public TimingWindows       windows;
    public ScoreManager        score;

    [Header("Visual/Posições")]
    public Transform      bossMouth;
    public Transform      playerCenter;
    public BossProjectile projectilePrefab;
    public float          projectileSpeed = 7f;

    [Header("Clash – Movimento/Animação")]
    public Transform visualRoot; 
    public float preStepDistance  = 0.20f;
    public float lungeDistance    = 1.50f;
    public float lungeDuration    = 0.12f;
    public float lingerDuration   = 0.10f;
    public float retreatDuration  = 0.20f;
    public float recoilDistance   = 0.60f;
    public float recoilDuration   = 0.15f;
    public float recoilReturnTime = 0.20f;

    [Header("Clash – Parry Tuning")]
    [Range(0f,1f)]   public float parryFollowThrough = 0.35f;
    [Range(0.01f, .25f)] public float parryFollowTime = 0.08f;
    [Range(0f, .15f)] public float parryHitStop = 0.06f;
    [Header("Projectile Overrides")]
    [Range(0f, 3f)] public float projectileScale = 0.5f;
    public Transform judgeWorldParent;
    public Vector3   judgeWorldOffset = new(0f, 0.5f, 0f);
    public bool      scaleJudgeOffset = true;

    [Header("Timeline")]
    public List<BossEvent> timeline = new();
    int _nextEvent;

    [Header("Clash – Feedback Visual")]
    public NoteOutlineShaderDriver clashOutline;
    public NoteJudgePopup           clashJudge;
    public Transform                clashJudgeParent;
    public Vector3                  clashJudgeWorldOffset = new(0f, 0.7f, 0f);

    readonly List<BossProjectile> _active = new();
    bool      _clashActive;
    bool      _clashParried;
    float     _clashTime;
    Vector3   _clashStart;
    Vector3   _clashDir;
    Coroutine _clashCo;
    Coroutine _recoilCo;
    Coroutine _parryCo;

    Transform Mover => visualRoot ? visualRoot : transform;
    float EaseOutQuad(float x)    => 1f - (1f - x) * (1f - x);
    float EaseInOutQuad(float x)  => x < 0.5f ? 2f*x*x : 1f - Mathf.Pow(-2f*x + 2f, 2f)/2f;

    void OnEnable()  { BindInput(); }
    void OnDisable() { UnbindInput(); }

    void Start() {
        timeline.Sort((a,b) => a.t.CompareTo(b.t));
        _nextEvent = 0;
    }

    void Update() {
        if (!conductor) return;
        float t = conductor.SongTimeSec;

        while (_nextEvent < timeline.Count && t >= timeline[_nextEvent].t) {
            var e = timeline[_nextEvent++];
            switch (e.type) {
                case BossEventType.SpikeVolley: StartCoroutine(Volley(e)); break;
                case BossEventType.Clash:       if (_clashCo != null) StopCoroutine(_clashCo);
                                                _clashCo = StartCoroutine(Clash(e)); break;
            }
        }
    }

    public void WireSceneRefs(
        RhythmConductor c, PlayerHealth p, NoteSpawner s,
        TimingWindows tw, InputActionProperty hit, Transform center, ScoreManager sm)
    {
        conductor    = c;
        player       = p;
        spawner      = s;
        windows      = tw;
        playerCenter = center;
        score        = sm;

        UnbindInput();
        hitAction = hit;
        BindInput();
    }

    void BindInput() {
        var act = hitAction.action;
        if (act == null) return;
        act.performed += OnHit;
        act.Enable();
    }
    void UnbindInput() {
        var act = hitAction.action;
        if (act == null) return;
        act.performed -= OnHit;
        act.Disable();
    }

    IEnumerator Volley(BossEvent e) {
        for (int i = 0; i < e.count; i++) {
            SpawnProjectile();
            yield return new WaitForSeconds(e.interval);
        }
    }

    void SpawnProjectile() {
        var mouth = bossMouth ? bossMouth.position : transform.position;
        var hitPt = playerCenter ? playerCenter.position : mouth + Vector3.left * 5f;

        var p = Instantiate(projectilePrefab);

        p.judgeWorldParent = judgeWorldParent;
        p.judgeWorldOffset = judgeWorldOffset;
        p.scaleJudgeOffset = scaleJudgeOffset;

        p.lateMissWindow = (windows != null) ? windows.good : 0.08f;

        p.Init(conductor, mouth, hitPt, conductor.SongTimeSec, projectileSpeed, projectileScale);

        p.overshootUnits = projectileSpeed * p.lateMissWindow;

        if (p.outline) p.outline.projectile = p;

        p.onDone = OnProjectileDone;
        _active.Add(p);
    }

    void OnProjectileDone(BossProjectile p, bool parried)
    {
        _active.Remove(p);
        if (!parried) {
            player?.TakeHit();
            score?.RegisterMiss();
        }
    }

    void OnHit(InputAction.CallbackContext _)
    {
        if (!conductor || windows == null) return;
        float tNow = conductor.SongTimeSec;

        if (_clashActive)
        {
            float diffSigned = tNow - _clashTime;
            float absDiff    = Mathf.Abs(diffSigned);

            if (diffSigned < -windows.good) return;

            string label = null; int dmg = 0;
            if      (absDiff <= windows.perfect) { label = "PERFECT"; dmg = 3; }
            else if (absDiff <= windows.great)   { label = "GREAT";   dmg = 2; }
            else if (absDiff <= windows.good)    { label = "GOOD";    dmg = 1; }

            _clashActive = false;

            if (label != null)
            {
                _clashParried = true;
                boss?.TakeHit(dmg);
                score?.RegisterHit(label, absDiff);
                if (clashOutline) clashOutline.ManualResolve();
                ShowClashJudge(label);

                if (_parryCo  != null) StopCoroutine(_parryCo);
                if (_recoilCo != null) StopCoroutine(_recoilCo);
                _parryCo = StartCoroutine(ClashParrySequence());

                if (spawner) spawner.enabled = true;
            }
            else
            {
                player?.TakeHit();
                score?.RegisterMiss();
                if (clashOutline) clashOutline.ManualResolve();
                ShowClashJudge("MISS");
            }
            return;
        }

        if (_active == null || _active.Count == 0) return;

        BossProjectile best = null; float bestDiff = float.MaxValue;
        for (int i = 0; i < _active.Count; i++)
        {
            var pr = _active[i];
            if (pr == null || pr.IsResolved) continue;
            float d = Mathf.Abs(pr.impactTime - tNow);
            if (d < bestDiff) { best = pr; bestDiff = d; }
        }
        if (best == null) return;

        string lbl = null;
        if      (bestDiff <= windows.perfect) lbl = "PERFECT";
        else if (bestDiff <= windows.great)   lbl = "GREAT";
        else if (bestDiff <= windows.good)    lbl = "GOOD";

        if (lbl != null) {
            score?.RegisterHit(lbl, bestDiff);
            best.Parry(lbl);
        }
    }


    IEnumerator Clash(BossEvent e)
    {
        if (spawner) spawner.enabled = false;

        var mover = Mover;
        _clashParried = false;
        _clashStart   = mover.position;
        _clashDir     = (playerCenter ? (playerCenter.position - _clashStart).normalized : Vector3.left);

        Vector3 prePos    = _clashStart + _clashDir * preStepDistance;
        Vector3 strikePos = _clashStart + _clashDir * (preStepDistance + lungeDistance);

        _clashActive = true;
        _clashTime   = conductor.SongTimeSec + e.windup;

        if (clashOutline) clashOutline.ManualArm(conductor, _clashTime, e.windup);

        float t = 0f;
        while (t < e.windup && !_clashParried) {
            t += Time.deltaTime;
            float u = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / e.windup));
            mover.position = Vector3.Lerp(_clashStart, prePos, u);
            yield return null;
        }
        if (_clashParried) { if (spawner) spawner.enabled = true; _clashCo = null; yield break; }

        t = 0f;
        while (t < lungeDuration && !_clashParried) {
            t += Time.deltaTime;
            float u = EaseOutQuad(Mathf.Clamp01(t / lungeDuration));
            mover.position = Vector3.Lerp(prePos, strikePos, u);
            yield return null;
        }
        if (_clashParried) { if (spawner) spawner.enabled = true; _clashCo = null; yield break; }

        _clashActive = false;
        if (clashOutline) clashOutline.ManualResolve();
        ShowClashJudge("MISS");
        player?.TakeHit();
        score?.RegisterMiss();

        if (lingerDuration > 0f) yield return new WaitForSeconds(lingerDuration);

        Vector3 from = mover.position;
        t = 0f;
        while (t < retreatDuration) {
            t += Time.deltaTime;
            float u = EaseInOutQuad(Mathf.Clamp01(t / retreatDuration));
            mover.position = Vector3.Lerp(from, _clashStart, u);
            yield return null;
        }

        if (spawner) spawner.enabled = true;
        _clashCo = null;
    }

    IEnumerator ClashParrySequence()
    {
        var mover = Mover;

        Vector3 fromNow    = mover.position;
        Vector3 strikePos  = _clashStart + _clashDir * (preStepDistance + lungeDistance);
        Vector3 contactPos = Vector3.Lerp(fromNow, strikePos, Mathf.Clamp01(parryFollowThrough));

        float t = 0f;
        while (t < parryFollowTime) {
            t += Time.deltaTime;
            float u = EaseOutQuad(Mathf.Clamp01(t / parryFollowTime));
            mover.position = Vector3.Lerp(fromNow, contactPos, u);
            yield return null;
        }

        if (parryHitStop > 0f) yield return new WaitForSecondsRealtime(parryHitStop);

        Vector3 recoilPos = contactPos - _clashDir * recoilDistance;
        t = 0f;
        while (t < recoilDuration) {
            t += Time.deltaTime;
            float u = EaseOutQuad(Mathf.Clamp01(t / recoilDuration));
            mover.position = Vector3.Lerp(contactPos, recoilPos, u);
            yield return null;
        }

        Vector3 backFrom = mover.position;
        t = 0f;
        while (t < recoilReturnTime) {
            t += Time.deltaTime;
            float u = EaseInOutQuad(Mathf.Clamp01(t / recoilReturnTime));
            mover.position = Vector3.Lerp(backFrom, _clashStart, u);
            yield return null;
        }
    }

    void ShowClashJudge(string label)
    {
        if (!clashJudge) return;

        Vector3 anchor = clashJudgeParent ? clashJudgeParent.position : Mover.position;

        Transform worldParent = judgeWorldParent ? judgeWorldParent : null;

        clashJudge.transform.SetParent(worldParent, true);

        Vector3 off = clashJudgeWorldOffset;
        if (scaleJudgeOffset && visualRoot) off *= visualRoot.lossyScale.x;
        clashJudge.transform.position = anchor + off;

        clashJudge.transform.localRotation = Quaternion.identity;
        clashJudge.transform.localScale    = Vector3.one;

        clashJudge.Play(label);
    }
}

public enum BossEventType { SpikeVolley, Clash }

[System.Serializable]
public class BossEvent {
    public float t;
    public BossEventType type;
    // volley
    public int   count    = 3;
    public float interval = 0.30f;
    // clash
    public float windup   = 1.20f;
}
