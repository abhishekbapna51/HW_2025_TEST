using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Global game state: Start, Running, GameOver.
/// Controls Time.timeScale pause/resume and scene restart.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public bool IsRunning { get; private set; } = false;
    public bool IsGameOver { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // start paused (Start screen visible)
        PauseGame();
    }

    /// <summary>Called by UIManager when Start pressed.</summary>
    public void StartGame()
    {
        IsGameOver = false;
        IsRunning = true;
        ResumeGame();
        ScoreManager.Instance?.ResetScore();
        // Optionally enable spawners, player input etc.
    }

    /// <summary>Call this to mark game over (show UI, pause gameplay)</summary>
    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        IsRunning = false;
        PauseGame();

        // Update UI
        UIManager.Instance?.ShowGameOver();
    }

    public void Restart()
    {
        // Make sure time scale reset
        Time.timeScale = 1f;
        // reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }
}
