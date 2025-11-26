using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// UIManager: controls Start and GameOver UI, plus simple music control.
/// - Start button: starts game, starts (and loops) music
/// - GameOver: stops music, shows only Restart button (no quit, no final score)
/// - Restart: reloads via GameStateManager.Restart() if available, otherwise reloads active scene
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject startPanel;      // top-level start panel (big play button)
    public GameObject gameOverPanel;   // top-level game over panel (should contain restartButton)

    [Header("Start Panel")]
    public Button startButton;

    [Header("Game Over Panel (we will only expose Restart for the player)")]
    public Button restartButton;

    // OPTIONAL inspector references we will hide on GameOver
    [Header("Optional UI (will be hidden on Game Over)")]
    public TMP_Text finalScoreText;    // optional final score text (we will hide it on show)
    public Button quitButton;          // optional quit button (we will hide it on show)

    [Header("Audio")]
    [Tooltip("Assign the AudioSource that plays background music. Set Play On Awake = false on this source.")]
    public AudioSource musicSource;

    [Header("Behavior")]
    [Tooltip("If true the UIManager will try to use GameStateManager.Instance.Restart() when Restart is pressed.")]
    public bool useGameStateManagerForRestart = true;

    private void Awake()
    {
        // singleton + persist
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
        // Bind buttons (remove previous listeners to avoid duplicates)
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

        // Ensure music doesn't auto-play even if inspector left it on
        if (musicSource != null && musicSource.playOnAwake)
        {
            musicSource.playOnAwake = false;
        }

        // At launch show Start and pause game
        ShowStart(resetScore: true);
        HideGameOver();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        // Clean up listeners
        if (startButton != null) startButton.onClick.RemoveListener(OnStartPressed);
        if (restartButton != null) restartButton.onClick.RemoveListener(OnRestartPressed);
        if (quitButton != null) quitButton.onClick.RemoveListener(OnQuitPressed);
    }

    // ----------------
    // Public UI control
    // ----------------

    /// <summary>
    /// Show the Start UI and pause gameplay. Optionally reset score.
    /// </summary>
    public void ShowStart(bool resetScore = false)
    {
        if (startPanel != null) startPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        Time.timeScale = 0f;

        if (resetScore) ScoreManager.Instance?.ResetScore();

        // Stop music to ensure a fresh start (safe)
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            Debug.Log("[UIManager] Stopped music on ShowStart.");
        }
    }

    public void HideStart()
    {
        if (startPanel != null) startPanel.SetActive(false);
    }

    /// <summary>
    /// Show Game Over UI. We will show only the Restart option and hide final score / quit button.
    /// Also stop music and pause time.
    /// </summary>
    public void ShowGameOver()
    {
        // Hide Final Score and Quit if present (user requested only Restart)
        if (finalScoreText != null) finalScoreText.gameObject.SetActive(false);
        if (quitButton != null) quitButton.gameObject.SetActive(false);

        // Ensure Restart button is visible and interactable
        if (restartButton != null) restartButton.gameObject.SetActive(true);

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        // Stop music immediately
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            Debug.Log("[UIManager] Music stopped on Game Over.");
        }

        // Pause gameplay
        Time.timeScale = 0f;
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // If you want finalScoreText visible again for a subsequent run, re-enable it here.
        if (finalScoreText != null) finalScoreText.gameObject.SetActive(true);
        if (quitButton != null) quitButton.gameObject.SetActive(true);
    }

    // ----------------
    // Button callbacks
    // ----------------

    /// <summary>Called when Start button is pressed.</summary>
    public void OnStartPressed()
    {
        // Hide start UI and resume gameplay
        HideStart();
        HideGameOver();

        ScoreManager.Instance?.ResetScore();

        // Start/resume music (user gesture)
        if (musicSource != null)
        {
            musicSource.loop = true;
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
                Debug.Log("[UIManager] Music started on Start button.");
            }
        }

        // Unpause
        Time.timeScale = 1f;

        // Delegates to GameStateManager if present
        GameStateManager.Instance?.StartGame();
    }

    /// <summary>Called when Restart button is pressed.</summary>
    public void OnRestartPressed()
    {
        // Ensure time restored before reload
        Time.timeScale = 1f;

        // Stop music (will restart on Start press after reload)
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            Debug.Log("[UIManager] Music stopped on Restart.");
        }

        // Prefer GameStateManager.Restart if you have it
        if (useGameStateManagerForRestart && GameStateManager.Instance != null)
        {
            GameStateManager.Instance.Restart();
            return;
        }

        // Fallback: reload current active scene
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
