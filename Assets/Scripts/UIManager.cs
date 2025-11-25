using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// UIManager: controls Start and GameOver UI. Place on a persistent object (Canvas or Managers).
/// Wire StartPanel, GameOverPanel, Start/Restart/Quit buttons and FinalScoreText in the inspector.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject startPanel;      // top-level start panel (big play button)
    public GameObject gameOverPanel;   // top-level game over panel

    [Header("Start Panel")]
    public Button startButton;

    [Header("Game Over Panel")]
    public TMP_Text finalScoreText;
    public Button restartButton;
    public Button quitButton;

    private void Awake()
    {
        // Singleton + persist across scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Wire UI callbacks (allow inspector wiring or automatic wiring here)
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartPressed);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartPressed);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnQuitPressed);
        }

        // At launch show start panel and pause gameplay
        ShowStart();
        HideGameOver();
    }

    private void OnDestroy()
    {
        // Cleanup listeners to avoid leaks / duplicate calls after reloads
        if (startButton != null) startButton.onClick.RemoveListener(OnStartPressed);
        if (restartButton != null) restartButton.onClick.RemoveListener(OnRestartPressed);
        if (quitButton != null) quitButton.onClick.RemoveListener(OnQuitPressed);

        if (Instance == this) Instance = null;
    }

    /// <summary>Show the Start UI and pause game. Pass true to reset score immediately.</summary>
    public void ShowStart(bool resetScore = false)
    {
        if (startPanel != null) startPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // pause gameplay
        Time.timeScale = 0f;

        if (resetScore)
            ScoreManager.Instance?.ResetScore();
    }

    public void HideStart()
    {
        if (startPanel != null) startPanel.SetActive(false);
    }

    /// <summary>Show Game Over UI, update final score, and pause game.</summary>
    public void ShowGameOver()
    {
        UpdateFinalScore();
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        // pause gameplay
        Time.timeScale = 0f;
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    private void UpdateFinalScore()
    {
        if (finalScoreText != null)
        {
            finalScoreText.text = ScoreManager.Instance != null ? ScoreManager.Instance.Score.ToString() : "0";
        }
    }

    // ----- Button callbacks -----

    /// <summary>Called by Start button (or can be called from code).</summary>
    public void OnStartPressed()
    {
        // Hide start UI and resume gameplay
        HideStart();
        HideGameOver();

        // Reset score to 0 at start of run
        ScoreManager.Instance?.ResetScore();

        // Unpause
        Time.timeScale = 1f;

        // Let GameStateManager handle any extra game-start logic if present
        GameStateManager.Instance?.StartGame();
    }

    /// <summary>Called by Restart button.</summary>
    public void OnRestartPressed()
    {
        // Ensure timeScale restored before reload
        Time.timeScale = 1f;

        // Prefer GameStateManager.Restart() if implemented
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.Restart();
            return;
        }

        // Fallback: reload active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnQuitPressed()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
