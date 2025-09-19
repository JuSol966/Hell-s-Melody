using UnityEngine;

public class EnemyNote : MonoBehaviour
{
    private RhythmConductor _conductor;
    
    [Header("Runtime")] public float targetTime;
    public bool judged;
    public bool missed;
    public System.Action<EnemyNote> OnDespawn;

    [Header("Tuning")] public float unitsPerSecond = 6f;
    public float approachTime = 1.1f;
   

    [Header("Lane/Refs")] public float laneY = 0f;
    public float hitlineX = 0f;
    
    [Header("UI")]
    public NoteJudgeLabel judge;
    public float despawnAfterJudge = 0.55f;
    public Transform judgeWorldParent;
    public Vector3 judgeLocalOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Visual")]
    public Renderer[] hideOnJudge;

    [Header("Judge Mode")]
    public bool despawnInstantOnJudge = true;
    
    void Update() {
        if (_conductor == null || _conductor.IsPaused) return;
        float songTime = _conductor.SongTimeSec;

        float timeToHit = targetTime - songTime;
        float x = hitlineX + timeToHit * unitsPerSecond;
        transform.position = new Vector3(x, laneY, 0f);
    }
    
    public void Judge(string label, bool wasMiss) {
        judged = true;
        missed = wasMiss;
        ShowJudge(label);
    }

    public void HideBody(bool hide) {
        if (hideOnJudge == null) return;
        foreach (var r in hideOnJudge) if (r) r.enabled = !hide;
    }

    public void ShowJudge(string label) {
        HideBody(true);

        if (judge) {
            var parent = judgeWorldParent ? judgeWorldParent : transform.parent;
            judge.transform.SetParent(parent, true);
            judge.Play(label);
            StartCoroutine(ReattachJudgeWhenDone());
        }

        if (despawnInstantOnJudge) OnDespawn?.Invoke(this);
        else StartCoroutine(DespawnLater(despawnAfterJudge));
    }

    private System.Collections.IEnumerator ReattachJudgeWhenDone() {
        while (judge && judge.gameObject.activeSelf) yield return null;
        if (!judge) yield break;

        judge.transform.SetParent(transform, true);
        judge.transform.localPosition = judgeLocalOffset;
        judge.transform.localRotation = Quaternion.identity;
        judge.transform.localScale    = Vector3.one;
    }

    private System.Collections.IEnumerator DespawnLater(float delay) {
        yield return new WaitForSeconds(delay);
        OnDespawn?.Invoke(this);
    }
    
    public float SongTimeNow() => _conductor ? _conductor.SongTimeSec : 0f;

    public void Init(RhythmConductor c, float hitX, float y, float t, float approach, float ups) {
        _conductor = c;
        hitlineX = hitX;
        laneY = y;
        targetTime = t;
        approachTime = approach;
        unitsPerSecond = ups;
        judged = false;
        missed = false;

        HideBody(false);
        if (judge) {
            judge.gameObject.SetActive(false);
            judge.transform.SetParent(transform, true);
            judge.transform.localPosition = judgeLocalOffset;
            judge.transform.localRotation = Quaternion.identity;
            judge.transform.localScale    = Vector3.one;
        }

        float songTime = _conductor.SongTimeSec;
        float timeToHit = targetTime - songTime;
        transform.position = new Vector3(hitlineX + timeToHit * unitsPerSecond, laneY, 0f);
    }
}
