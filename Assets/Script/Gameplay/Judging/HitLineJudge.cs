using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class HitLineJudge : MonoBehaviour
{
    [Header("Refs")]
    public RhythmConductor conductor;
    public NoteSpawner spawner;
    public ScoreManager score;
    public InputActionProperty hitAction;
    public TimingWindows windows;

    void OnEnable() {
        var act = hitAction.action;
        if (act != null) {
            act.performed += OnHit;
            act.Enable();
        }
    }

    void OnDisable() {
        var act = hitAction.action;
        if (act != null) {
            act.performed -= OnHit;
            act.Disable();
        }
    }

    void Update()
    {
        if (!IsSetupOk() || IsPaused()) return;
        ProcessTimeouts(conductor.SongTimeSec);
    }

    private void OnHit(InputAction.CallbackContext ctx)
    {
        if (!IsSetupOk() || IsPaused()) return;

        float tNow = conductor.SongTimeSec;

        if (!TryGetBestCandidate(tNow, out var note, out var diff)) {
            score?.RegisterMiss();
            return;
        }

        var rank = EvaluateRank(diff);
        
        if (rank == HitRank.Miss) {
            score?.RegisterMiss();
            return;
        }

        ApplyRank(note, rank, diff);
    }


    private void ProcessTimeouts(float songTime)
    {
        var notes = spawner.ActiveNotes;
        for (int i = notes.Count - 1; i >= 0; i--) {
            var n = notes[i];
            if (n == null || n.judged) continue;
            if (songTime > n.targetTime + windows.miss) {
                n.judged = true;
                n.missed = true;
                n.ShowJudge("MISS");
            }
        }
    }

    private bool TryGetBestCandidate(float tNow, out EnemyNote best, out float bestDiff)
    {
        best = null; bestDiff = float.MaxValue;
        var notes = spawner.ActiveNotes;

        for (int i = 0; i < notes.Count; i++) {
            var n = notes[i];
            if (n == null || n.judged) continue;
            float d = Mathf.Abs(n.targetTime - tNow);
            if (d < bestDiff) { best = n; bestDiff = d; }
        }
        return best != null;
    }

    private HitRank EvaluateRank(float absDiff)
    {
        if (absDiff <= windows.perfect) return HitRank.Perfect;
        if (absDiff <= windows.great)   return HitRank.Great;
        if (absDiff <= windows.good)    return HitRank.Good;
        return HitRank.Miss;
    }

    private void ApplyRank(EnemyNote note, HitRank rank, float diff)
    {
        note.judged = true;
        note.missed = false;

        switch (rank) {
            case HitRank.Perfect:
                score?.RegisterHit("PERFECT", diff);
                note.ShowJudge("PERFECT");
                break;
            case HitRank.Great:
                score?.RegisterHit("GREAT", diff);
                note.ShowJudge("GREAT");
                break;
            case HitRank.Good:
                score?.RegisterHit("GOOD", diff);
                note.ShowJudge("GOOD");
                break;
        }
    }

    private bool IsSetupOk() =>
        conductor != null && spawner != null && windows != null;

    private bool IsPaused() =>
        conductor is { IsPaused: true };
}

// Rank sem “strings mágicas”
public enum HitRank { Miss, Good, Great, Perfect }