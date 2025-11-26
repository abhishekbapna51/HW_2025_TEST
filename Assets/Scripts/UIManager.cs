using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Persistent UIManager that re-binds scene UI on every scene load and manages music start/stop.
/// Robust restart handling: disables restart button during reload to avoid double-click issues.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels (auto-bound if left null)")]
    public GameObject startPanel;
    public GameObject gameOverPanel;

    [Header("Buttons / texts (auto-bound if left null)")]
    public Button startButton;
    public TMP_Text finalScoreText;
    public Button restartButton;
    public Button quitButton;

    [Header("Music (optional)")]
    [Tooltip("Optional: assign AudioSource here (or name your GameObject 'music' so it auto-finds). Play On Awake should be OFF.")]
    public AudioSource musicSource;

    [Header("Behavior")]
    public bool autoFindByName = true; // try to find objects by name if inspector fields are null

    // internal state to avoid duplicate restarts
    bool isReloading = false;

    private void Awake()
    {
        // singleton
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        // Try to bind now (if UI already present)
        FindAndBindUI();
        // Pause and show start
        ShowStart(resetScore: true);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[UIManager] Scene loaded: {scene.name}. Rebinding UI and Audio.");
        isReloading = false; // allow restart again
        FindAndBindUI();

        // After reload show Start panel and pause (so user clicks Start to begin)
        ShowStart(resetScore: true);

        // ensure restart button interactable
        if (restartButton != null) restartButton.interactable = true;
    }

    /// <summary>
    /// Finds and binds UI controls and music (if left null in inspector).
    /// Uses GameObject.Find with conventional names (change names in this file if your objects are named differently).
    /// </summary>
    private void FindAndBindUI()
    {
        // Panels
        if (startPanel == null && autoFindByName)
            startPanel = GameObject.Find("StartPanel");

        if (gameOverPanel == null && autoFindByName)
            gameOverPanel = GameObject.Find("GameOverPanel");

        // Buttons & texts
        if (startButton == null && autoFindByName)
        {
            var sbGo = GameObject.Find("StartButton");
            if (sbGo != null) startButton = sbGo.GetComponent<Button>();
        }

        if (restartButton == null && autoFindByName)
        {
            var rbGo = GameObject.Find("RestartButton");
            if (rbGo != null) restartButton = rbGo.GetComponent<Button>();
        }

        if (quitButton == null && autoFindByName)
        {
            var qbGo = GameObject.Find("QuitButton");
            if (qbGo != null) quitButton = qbGo.GetComponent<Button>();
        }

        if (finalScoreText == null && autoFindByName)
        {
            var fsGo = GameObject.Find("FinalScoreText");
            if (fsGo != null) finalScoreText = fsGo.GetComponent<TMP_Text>();
        }

        // Music auto-find: look for GameObject named "music" or "Music"
        if (musicSource == null && autoFindByName)
        {
            var mg = GameObject.Find("music") ?? GameObject.Find("Music");
            if (mg != null)
            {
                var src = mg.GetComponent<AudioSource>();
                if (src != null)
                {
                    musicSource = src;
                    Debug.Log("[UIManager] Auto-bound musicSource from GameObject named 'music' / 'Music'.");
                }
            }
        }

        // Safety: ensure PlayOnAwake is off for musicSource (we start music on Start button click)
        if (musicSource != null && musicSource.playOnAwake)
        {
            musicSource.playOnAwake = false;
            Debug.Log("[UIManager] musicSource.playOnAwake was true — set to false to prevent auto-play.");
        }

        // Wire listeners safely (remove previous so we don't duplicate)
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartPressed);
        }
        else Debug.LogWarning("[UIManager] startButton not found or assigned.");

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartPressed);
            restartButton.interactable = true;
        }
        else Debug.LogWarning("[UIManager] restartButton not found or assigned.");

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnQuitPressed);
        }

        // Ensure panels exist
        if (startPanel == null) Debug.LogWarning("[UIManager] StartPanel not found in scene.");
        if (gameOverPanel == null) Debug.LogWarning("[UIManager] GameOverPanel not found in scene.");
    }

    // ---------- UI control ----------

    public void ShowStart(bool resetScore = false)
    {
        if (startPanel != null) startPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        Time.timeScale = 0f;

        if (resetScore) ScoreManager.Instance?.ResetScore();

        // Stop music on showing Start to ensure clean state
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            Debug.Log("[UIManager] musicSource stopped on ShowStart.");
        }
    }

    public void HideStart() { if (startPanel != null) startPanel.SetActive(false); }

    public void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (startPanel != null) startPanel.SetActive(false);

        // Update final score if visible
        if (finalScoreText != null)
            finalScoreText.text = ScoreManager.Instance != null ? ScoreManager.Instance.Score.ToString() : "0";

        // Stop music on Game Over
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            Debug.Log("[UIManager] musicSource stopped on GameOver.");
        }

        Time.timeScale = 0f;
    }

    public void HideGameOver() { if (gameOverPanel != null) gameOverPanel.SetActive(false); }

    // ---------- Button callbacks ----------

    public void OnStartPressed()
    {
        HideStart();
        HideGameOver();
        ScoreManager.Instance?.ResetScore();

        // Start music (user gesture)
        if (musicSource != null)
        {
            musicSource.loop = true;
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
                Debug.Log("[UIManager] musicSource.Play() called on Start.");
            }
        }

        Time.timeScale = 1f;
        GameStateManager.Instance?.StartGame();
    }

    public void OnRestartPressed()
    {
        // Prevent duplicate restarts while reload is in progress
        if (isReloading)
        {
            Debug.Log("[UIManager] Restart requested but reload already in progress — ignoring duplicate click.");
            return;
        }

        isReloading = true;

        // Ensure time restored before reload
        Time.timeScale = 1f;

        // Stop music here (it will be restarted on next Start press)
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            Debug.Log("[UIManager] musicSource stopped on Restart.");
        }

        // Disable restart button immediately to avoid double-clicks
        if (restartButton != null) restartButton.interactable = false;

        // Prefer GameStateManager.Restart if available (it will do Time.timeScale reset + LoadScene)
        if (GameStateManager.Instance != null)
        {
            Debug.Log("[UIManager] Delegating restart to GameStateManager.Restart()");
            GameStateManager.Instance.Restart();
            // GameStateManager.Restart will trigger Scene load; OnSceneLoaded will reset isReloading = false
            return;
        }

        // Fallback: async reload the active scene (so OnSceneLoaded fires and rebind occurs)
        Debug.Log("[UIManager] No GameStateManager found — using SceneManager.LoadSceneAsync fallback.");
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        // OnSceneLoaded will set isReloading = false
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
