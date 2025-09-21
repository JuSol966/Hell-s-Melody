using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class GameFlow : MonoBehaviour
{
    [Header("Core Refs")]
    public NoteSpawner spawner;
    public RhythmConductor conductor;
    public ScoreManager score;
    public PlayerHealth health;

    [Header("UI")]
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    [Header("Input")]
    public InputActionProperty pauseAction;

    [Header("Options")]
    public bool pauseOnFocusLoss = true;
    [Range(0f, 2f)] public float restartLeadIn = 0.8f;

    [Header("Boss")]
    public BossDirector bossPrefab;
    public Transform bossSpawnPoint;
    public float bossSpawnAtSec = 0f;
    public TimingWindows timingWindows; 
    public InputActionProperty hitActionForBoss;
    private BossDirector _boss;
    private Coroutine _bossSpawnCo;

    private bool _paused;
    private bool _gameOver;

    void Awake() {
        Time.timeScale = 1f;
        if (pausePanel)    pausePanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }

    void OnEnable() {
        var act = pauseAction.action;
        if (act != null) { act.performed += OnPausePerformed; act.Enable(); }
        if (health) health.onDeath += OnPlayerDeath;
    }

    void OnDisable() {
        var act = pauseAction.action;
        if (act != null) { act.performed -= OnPausePerformed; act.Disable(); }
        if (health) health.onDeath -= OnPlayerDeath;
        if (_bossSpawnCo != null) StopCoroutine(_bossSpawnCo);
        Time.timeScale = 1f;
    }

    void Start() => StartRun();

    void OnApplicationFocus(bool focus) {
        if (pauseOnFocusLoss && !focus && !_paused && !_gameOver) Pause();
    }

    void OnPausePerformed(InputAction.CallbackContext _) {
        if (_gameOver) return;
        TogglePause();
    }

    public void StartRun() {
        _gameOver = false;
        _paused = false;

        Time.timeScale = 1f;
        if (pausePanel)    pausePanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);

        score?.ResetAll();
        health?.ResetAll();

        conductor?.Restart(restartLeadIn);

        if (spawner) {
            spawner.enabled = true;
            spawner.StartChart();
        }

        if (_boss) { Destroy(_boss.gameObject); _boss = null; }
        if (_bossSpawnCo != null) StopCoroutine(_bossSpawnCo);
        _bossSpawnCo = StartCoroutine(SpawnBossWhenReady());
    }

    IEnumerator SpawnBossWhenReady() {
        if (!bossPrefab) yield break;

        while (!_gameOver && conductor == null) yield return null;
        while (!_gameOver && conductor.SongTimeSec < bossSpawnAtSec) yield return null;

        if (!_gameOver) SpawnBossNow();
    }

    void SpawnBossNow() {
        var pos = bossSpawnPoint ? bossSpawnPoint.position : Vector3.zero;
        _boss = Instantiate(bossPrefab, pos, Quaternion.identity);

        _boss.WireSceneRefs(
            conductor,
            health,
            spawner,
            timingWindows,
            hitActionForBoss,
            spawner.hitLine,
            score
        );

        if (_boss.boss == null) _boss.boss = _boss.GetComponent<BossHealth>();
    }

    public void Retry() => StartRun();

    public void GameOver() {
        if (_gameOver) return;
        _gameOver = true;

        conductor?.Pause();
        if (spawner) spawner.enabled = false;
        Time.timeScale = 0f;

        if (pausePanel) pausePanel.SetActive(false);
        if (gameOverPanel) {
            gameOverPanel.SetActive(true);
            var ui = gameOverPanel.GetComponent<GameOverUI>();
            if (ui) ui.Refresh();
        }
    }

    public void TogglePause() { if (_paused) Resume(); else Pause(); }

    public void Pause() {
        if (_paused || _gameOver) return;
        _paused = true;
        if (pausePanel) pausePanel.SetActive(true);
        conductor?.Pause();
        if (spawner) spawner.enabled = false;
        Time.timeScale = 0f;
    }

    public void Resume() {
        if (!_paused || _gameOver) return;
        _paused = false;
        if (pausePanel) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        conductor?.Resume();
        if (spawner) spawner.enabled = true;
    }

    void OnPlayerDeath() => GameOver();

    public void QuitToMenu(string sceneName) {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
