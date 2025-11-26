using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent ScoreManager that re-binds the score TMP text on scene loads
/// so score UI keeps updating correctly across restarts.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    // Score value (persisted across scene reloads unless ResetScore is called)
    public int Score { get; private set; } = 0;

    [Header("Optional: assign in inspector (will auto-find if null)")]
    public TMP_Text scoreText;

    [Header("Auto-find names (if you used different names change them)")]
    public string[] scoreTextNames = new string[] { "ScoreBigText", "ScoreText", "Score" };

    private void Awake()
    {
        // singleton
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // listen for scene loads to rebind UI references
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // Try to find scoreText immediately (scene may already be loaded)
        TryBindScoreText();
        UpdateUI();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Rebind the score text after every scene load
        TryBindScoreText();
        // Ensure UI reflects current Score after rebinding
        UpdateUI();
    }

    private void TryBindScoreText()
    {
        if (scoreText != null) return;

        // Try by name(s)
        foreach (var n in scoreTextNames)
        {
            if (string.IsNullOrEmpty(n)) continue;
            var go = GameObject.Find(n);
            if (go != null)
            {
                var tmp = go.GetComponent<TMP_Text>();
                if (tmp != null)
                {
                    scoreText = tmp;
                    Debug.Log($"[ScoreManager] Bound scoreText to GameObject '{n}'.");
                    return;
                }
            }
        }

        // If still null, try to find any TMP with tag "Score" (if used)
        // (Optional: you can assign manually in inspector to avoid auto-finding)
        if (scoreText == null)
        {
            Debug.LogWarning("[ScoreManager] scoreText not found automatically. Assign in inspector or ensure a GameObject exists named ScoreBigText/ScoreText/Score.");
        }
    }

    public void AddScore(int amount = 1)
    {
        Score += amount;
        UpdateUI();
    }

    public void ResetScore()
    {
        Score = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = Score.ToString();
        }
    }
}
