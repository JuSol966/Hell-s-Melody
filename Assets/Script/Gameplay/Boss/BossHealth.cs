using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Range(1,50)] public int maxHp = 8;
    public int Hp { get; private set; }

    public System.Action<int> onHpChanged;
    public System.Action onDeath;

    void Awake() => ResetAll();

    public void ResetAll() {
        Hp = maxHp;
        onHpChanged?.Invoke(Hp);
    }
    public void TakeHit(int amount = 1) {
        if (Hp <= 0) return;
        Hp = Mathf.Max(0, Hp - amount);
        onHpChanged?.Invoke(Hp);
        if (Hp == 0) onDeath?.Invoke();
    }
}
