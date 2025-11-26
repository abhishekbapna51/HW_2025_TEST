using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // NOTE: these may be null between reloads; we rebind them on scene loaded.
    [Header("Panels (will be auto-bound if left null)")]
    public GameObject startPanel;
    public GameObject gameOverPanel;

    [Header("Buttons / texts (auto-bound if left null)")]
    public Button startButton;
    public TMP_Text finalScoreText;
    public Button restartButton;
    public Button quitButton;

    private void Awake()
    {
        // singleton
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Subscribe to scene loaded so we can re-bind UI from new scene
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // cleanup
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        // Try to bind now (in case the UI is already in the scene)
        FindAndBindUI();
        // Show start and pause
        ShowStart(resetScore: true);
    }

    // Called after each scene load
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // re-find UI elements in the freshly loaded scene and re-wire
        FindAndBindUI();

        // After a reload we want the Start screen visible and game paused
        ShowStart(resetScore: true);
    }

    // Finds UI objects by conventional names and wires listeners.
    // Change the names below if your Hierarchy uses different names.
    private void FindAndBindUI()
    {
        // If someone pre-assigned in inspector, keep that — otherwise try to find
        if (startPanel == null)
            startPanel = GameObject.Find("StartPanel");

        if (gameOverPanel == null)
            gameOverPanel = GameObject.Find("GameOverPanel");

        // Buttons & final score
        if (startButton == null)
        {
            var sbGo = GameObject.Find("StartButton");
            if (sbGo != null) startButton = sbGo.GetComponent<Button>();
        }

        if (restartButton == null)
        {
            var rbGo = GameObject.Find("RestartButton");
            if (rbGo != null) restartButton = rbGo.GetComponent<Button>();
        }

        if (quitButton == null)
        {
            var qbGo = GameObject.Find("QuitButton");
            if (qbGo != null) quitButton = qbGo.GetComponent<Button>();
        }

        if (finalScoreText == null)
        {
            var fsGo = GameObject.Find("FinalScoreText");
            if (fsGo != null) finalScoreText = fsGo.GetComponent<TMP_Text>();
        }

        // Wire listeners safely (remove previous so we don't duplicate)
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

        // Ensure panels exist; if null just log a warning (so you can fix naming)
        if (startPanel == null) Debug.LogWarning("UIManager: StartPanel not found in scene.");
        if (gameOverPanel == null) Debug.LogWarning("UIManager: GameOverPanel not found in scene.");
    }

    // Show start UI and pause game; optionally reset score
    public void ShowStart(bool resetScore = false)
    {
        if (startPanel != null) startPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        Time.timeScale = 0f;
        if (resetScore) ScoreManager.Instance?.ResetScore();
    }

    public void HideStart() { if (startPanel != null) startPanel.SetActive(false); }

    public void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (startPanel != null) startPanel.SetActive(false);

        // Update final score text
        if (finalScoreText != null)
            finalScoreText.text = ScoreManager.Instance != null ? ScoreManager.Instance.Score.ToString() : "0";

        // Pause gameplay
        Time.timeScale = 0f;
    }

    public void HideGameOver() { if (gameOverPanel != null) gameOverPanel.SetActive(false); }

    // Button callbacks
    public void OnStartPressed()
    {
        HideStart();
        HideGameOver();
        ScoreManager.Instance?.ResetScore();
        Time.timeScale = 1f;
        GameStateManager.Instance?.StartGame();
    }

    public void OnRestartPressed()
    {
        // Make sure the game is in a running state before reload
        Time.timeScale = 1f;

        // Prefer GameStateManager.Restart (if available)
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.Restart();
            return;
        }

        // fallback: reload current scene
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
