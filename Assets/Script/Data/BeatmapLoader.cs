using UnityEngine;

public class BeatmapLoader : MonoBehaviour
{
    public TextAsset beatmapJson;

    public Beatmap Load() {
        if (!beatmapJson) {
            Debug.LogWarning("Beatmap JSON não atribuído. Use gerador por BPM.");
            return null;
        }
        
        return JsonUtility.FromJson<Beatmap>(beatmapJson.text);
    }
}
