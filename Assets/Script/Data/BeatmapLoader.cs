using UnityEngine;

public class BeatmapLoader : MonoBehaviour
{
    public TextAsset beatmapJson;

    public Beatmap Load() {
        if (!beatmapJson) {
            Debug.LogError("[BeatmapLoader] Nenhum TextAsset atribuído.");
            return null;
        }
        
        var map = JsonUtility.FromJson<Beatmap>(beatmapJson.text);
        
        if (map == null || map.notes == null || map.notes.Count == 0) {
            Debug.LogError("[BeatmapLoader] JSON inválido ou sem notas.");
            return null;
        }
        
        map.notes.Sort((a,b) => a.t.CompareTo(b.t));
        return map;
    }
}
