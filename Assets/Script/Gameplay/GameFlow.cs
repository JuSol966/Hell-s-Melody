using UnityEngine;
using UnityEngine.InputSystem;

public class GameFlow : MonoBehaviour
{
    [Header("Refs")]
    public NoteSpawner spawner;
    public RhythmConductor conductor;
    public ScoreManager score;

    [Header("Pause (opcional)")]
    public InputActionProperty pauseAction;
    public GameObject pausePanel;

    bool _paused;

    void OnEnable() {
        if (pauseAction.action != null) {
            pauseAction.action.performed += OnPausePerformed;
            pauseAction.action.Enable();
        }
    }
    void OnDisable() {
        if (pauseAction.action != null) {
            pauseAction.action.performed -= OnPausePerformed;
            pauseAction.action.Disable();
        }
    }

    void OnPausePerformed(InputAction.CallbackContext ctx) {
        if (_paused) Resume();
        else Pause();
    }

    // Chamado pelo botão "Retry" ou quando o jogador perde
    public void Retry() {
        _paused = false;
        if (pausePanel) pausePanel.SetActive(false);
        if (score) score.ResetAll();
        if (spawner) spawner.StartChart();
    }

    public void Pause() {
        if (_paused) return;
        _paused = true;
        if (pausePanel) pausePanel.SetActive(true);
        if (spawner) spawner.enabled = false;
        if (conductor) conductor.Pause();
        Time.timeScale = 0f;
    }

    public void Resume() {
        if (!_paused) return;
        _paused = false;
        if (pausePanel) pausePanel.SetActive(false);
        if (conductor) conductor.Resume();
        if (spawner) spawner.enabled = true;
        Time.timeScale = 1f;
    }
}
