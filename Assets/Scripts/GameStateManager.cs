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
        // start paused (Start screen visible). UIManager will handle showing start panel on its Start.
        PauseGame();
        IsRunning = false;
        IsGameOver = false;
    }

    /// <summary>Called by UIManager when Start pressed (or other systems).</summary>
    public void StartGame()
    {
        IsGameOver = false;
        IsRunning = true;
        ResumeGame();

        // Reset score at the start of the run
        ScoreManager.Instance?.ResetScore();

        // Hide start panel (defensive)
        UIManager.Instance?.HideStart();

        Debug.Log("[GameStateManager] StartGame called - game running.");
    }

    /// <summary>Call this to mark game over (show UI, pause gameplay)</summary>
    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        IsRunning = false;

        PauseGame();

        // Show game over UI
        UIManager.Instance?.ShowGameOver();

        Debug.Log("[GameStateManager] GameOver called - game paused and UI shown.");
    }

    /// <summary>Restart the current scene. Time scale is restored before reload.</summary>
    public void Restart()
    {
        Debug.Log("[GameStateManager] Restart called - reloading active scene.");
        // Make sure time scale reset so scene starts normally
        Time.timeScale = 1f;

        // Reset flags so singletons behave correctly
        IsGameOver = false;
        IsRunning = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        Debug.Log("[GameStateManager] PauseGame - Time.timeScale set to 0.");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        Debug.Log("[GameStateManager] ResumeGame - Time.timeScale set to 1.");
    }
}
