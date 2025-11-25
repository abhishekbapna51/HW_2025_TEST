using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int Score { get; private set; }

    [Header("UI (assign these in the Inspector)")]
    public TMP_Text scoreBigText;   // big green number (top-center)
    public TMP_Text scoreLabelText; // small "score" label under the number

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[ScoreManager] Awake - instance set");
    }

    private void Start()
    {
        ResetScore();
        Debug.Log("[ScoreManager] Start - score reset");
    }

    public void AddScore(int amount = 1)
    {
        Score += amount;
        UpdateUI();
        Debug.Log($"[ScoreManager] AddScore called. New Score = {Score}");
    }

    public void ResetScore()
    {
        Score = 0;
        UpdateUI();
        Debug.Log("[ScoreManager] ResetScore executed");
    }

    private void UpdateUI()
    {
        if (scoreBigText != null)
            scoreBigText.text = Score.ToString();
        if (scoreLabelText != null)
            scoreLabelText.text = "score";
    }
}
