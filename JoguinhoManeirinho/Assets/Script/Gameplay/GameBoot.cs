using UnityEngine;

public class GameBoot2D : MonoBehaviour
{
    public OneKeyUISpawner spawner;
    public bool autoStart = true;

    void Start() {
        if (autoStart && spawner != null) spawner.StartChart();
    }
}
