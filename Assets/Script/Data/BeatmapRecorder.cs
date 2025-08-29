using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BeatmapRecorder : MonoBehaviour
{
    public RhythmConductor conductor;
    public InputActionProperty hitAction;

    [Header("Dump helper")]
    public bool autoDumpOnStop = true;
    public bool copyToClipboard = true;
    [TextArea(5, 12)] public string lastJson;

    private readonly List<float> _times = new List<float>();

    void OnEnable() {
        hitAction.action.performed += OnHit;
        hitAction.action.Enable();
    }
    void OnDisable() {
        hitAction.action.performed -= OnHit;
        hitAction.action.Disable();
        if (autoDumpOnStop && _times.Count > 0) DumpJson();
    }

    private void OnHit(InputAction.CallbackContext ctx) {
        var t = conductor.SongTimeSec;
        _times.Add(t);
        Debug.Log($"rec {t:0.000}");
    }

    [ContextMenu("Dump JSON")]
    public void DumpJson() {
        var map = new Beatmap { songName = "recorded", offset = 0f, approachTime = 1.1f };
        foreach (var t in _times) map.notes.Add(new NoteData { t = t });
        var json = JsonUtility.ToJson(map, true);
        lastJson = json;
#if UNITY_EDITOR
        if (copyToClipboard) GUIUtility.systemCopyBuffer = json;
#endif
        Debug.Log("Beatmap JSON:\n" + json);
    }
}
