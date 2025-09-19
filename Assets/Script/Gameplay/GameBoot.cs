using UnityEngine;

public class GameBoot2D : MonoBehaviour
{
    public NoteSpawner spawner;
    public bool autoStart = true;

    void Start() {
        if (autoStart && spawner != null) spawner.StartChart();
    }
}
