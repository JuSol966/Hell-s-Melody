using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("Judge Popup")]
    public bool showJudgeOnHitOnly = true;
    public float judgeVisibleSeconds = 0.45f;
    public bool judgeFade = true;
    public float judgeFadeSeconds = 0.20f;

    private Coroutine _judgeRoutine;

    public TMP_Text scoreText;
    public TMP_Text comboText;
    public TMP_Text judgeText;
    public TMP_Text playerHPText;
    
    public int CurrentScore => _score;

    private int _score;
    private int _combo;
    private int _maxCombo;
    private int _playerHP = 5;
    
    void Start()
    {
        if (judgeText) judgeText.gameObject.SetActive(false);
    }

    public void ResetAll()
    {
        _score = 0;
        _combo = 0;
        _maxCombo = 0;
        _playerHP = 5;
        
        if (scoreText) scoreText.text = "Score: 0";
        if (comboText) comboText.text = "Combo: -";
        if (judgeText)
        {
            judgeText.gameObject.SetActive(false);
            judgeText.text = "";
        }
        if (playerHPText) playerHPText.text = _playerHP.ToString();
    }

    public void RegisterHit(string label, float diff)
    {
        int add = label switch
        {
            "PERFECT" => 1000,
            "GREAT" => 700,
            "GOOD" => 400,
            _ => 0
        };
        _score += add;
        _combo++;
        _maxCombo = Mathf.Max(_maxCombo, _combo);
        UpdateUI(label);

        if (judgeText && showJudgeOnHitOnly)
        {
            if (_judgeRoutine != null) StopCoroutine(_judgeRoutine);
            _judgeRoutine = StartCoroutine(ShowJudgeOnce(label));
        }
    }

    private System.Collections.IEnumerator ShowJudgeOnce(string label)
    {
        judgeText.gameObject.SetActive(true);
        judgeText.text = label;

        var baseColor = judgeText.color;
        baseColor.a = 1f;
        judgeText.color = baseColor;

        yield return new WaitForSeconds(judgeVisibleSeconds);

        if (judgeFade)
        {
            float t = 0f;
            while (t < judgeFadeSeconds)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(1f, 0f, t / judgeFadeSeconds);
                var c = judgeText.color; c.a = a; judgeText.color = c;
                yield return null;
            }
        }

        judgeText.gameObject.SetActive(false);
        var c2 = judgeText.color; c2.a = 1f; judgeText.color = c2;
    }

    public void RegisterMiss()
    {
        _combo = 0;
        _playerHP--;
        Debug.Log(_playerHP);
        UpdateUI("MISS");
    }

    private void UpdateUI(string label)
    {
        if (scoreText) scoreText.text = $"Score: {_score}";
        if (comboText) comboText.text = _combo > 0 ? $"Combo: {_combo}" : "Combo: -";
        if (judgeText) judgeText.text = label;
        if (judgeText) playerHPText.text = _playerHP.ToString();
    }
}
