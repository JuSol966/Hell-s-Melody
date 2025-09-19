using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    public ScoreManager score;
    public TMP_Text scoreText;

    void Awake() {
        if (!score) score = FindObjectOfType<ScoreManager>();
    }

    void OnEnable() => Refresh();

    public void Refresh() {
        if (!score) return;
        if (scoreText)    scoreText.text  = "SCORE " + score.CurrentScore.ToString("N0");
    }
}
