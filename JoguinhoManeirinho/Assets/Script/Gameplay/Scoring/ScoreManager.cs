using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text comboText;
    public TMP_Text judgeText;
    
    private int _score;
    private int _combo;
    private int _maxCombo;
    
    public void RegisterHit(string label, float diff) {
        int add = label switch {
            "PERFECT" => 1000,
            "GREAT"   => 700,
            "GOOD"    => 400,
            _         => 0
        };
        _score += add;
        _combo++;
        _maxCombo = Mathf.Max(_maxCombo, _combo);
        UpdateUI(label);
    }
    
    public void RegisterMiss() {
        _combo = 0;
        UpdateUI("MISS");
    }
    
    private void UpdateUI(string label) {
        if (scoreText) scoreText.text = $"Score: {_score}";
        if (comboText) comboText.text = _combo > 0 ? $"Combo: {_combo}" : "Combo: -";
        if (judgeText) judgeText.text = label;
    }
}
