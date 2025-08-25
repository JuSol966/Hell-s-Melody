using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NoteData {
    public float t;
    public string type = "tap";
    public float duration = 0f;
}

[Serializable]
public class Beatmap {
    public string songName = "sandbox";
    public float offset = 0f;
    public float approachTime = 1.1f;
    public List<NoteData> notes = new List<NoteData>();
}