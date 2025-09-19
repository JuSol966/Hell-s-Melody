using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Range(1, 10)]
    public int maxLives = 3;

    public int Lives { get; private set; }

    public System.Action<int> onLivesChanged;

    public System.Action onDeath;

    void Awake() => ResetAll();

    public void ResetAll()
    {
        Lives = maxLives;
        onLivesChanged?.Invoke(Lives);
    }

    public void TakeHit(int amount = 1)
    {
        if (Lives <= 0) return;
        Lives = Mathf.Max(0, Lives - amount);
        onLivesChanged?.Invoke(Lives);
        if (Lives <= 0) onDeath?.Invoke();
    }
}
