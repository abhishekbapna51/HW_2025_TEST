using UnityEngine;

/// <summary>
/// Robust game-over detector:
/// - Primary: if object's Y <= fallYThreshold -> GameOver()
/// - Optional: if this object enters a trigger collider tagged "DeathPlane" -> GameOver()
/// - Disables Doofus movement component (DoofusMovement) on game over to prevent continued movement.
/// - Writes helpful Debug.Log lines so you can see what happened in Console.
/// </summary>
[DisallowMultipleComponent]
public class FallGameOver : MonoBehaviour
{
    [Tooltip("If player Y goes below this value, trigger game over")]
    public float fallYThreshold = -6f;

    [Tooltip("Tag for an optional death trigger area. If empty, only Y-threshold is used.")]
    public string deathPlaneTag = "DeathPlane";

    bool invoked = false;

    void Start()
    {
        invoked = false;
        Debug.Log($"[FallGameOver] started. threshold={fallYThreshold}, deathPlaneTag='{deathPlaneTag}'");
    }

    void Update()
    {
        if (invoked) return;

        // Check Y threshold
        if (transform.position.y <= fallYThreshold)
        {
            Debug.Log($"[FallGameOver] Y threshold reached: y={transform.position.y} <= {fallYThreshold}");
            TriggerGameOver("YThreshold");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (invoked) return;

        if (!string.IsNullOrEmpty(deathPlaneTag))
        {
            if (other.CompareTag(deathPlaneTag))
            {
                Debug.Log($"[FallGameOver] Entered death trigger '{deathPlaneTag}' with collider '{other.name}'");
                TriggerGameOver("DeathPlaneTrigger");
            }
        }
    }

    private void TriggerGameOver(string cause)
    {
        invoked = true;

        // Try disable movement so it visually stops
        var move = GetComponent<DoofusMovement>();
        if (move != null)
        {
            try { move.enabled = false; Debug.Log("[FallGameOver] Disabled DoofusMovement."); } catch { }
        }

        // Try disable Rigidbody forces (optional)
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            Debug.Log("[FallGameOver] Set Rigidbody.isKinematic = true");
        }

        // Call GameStateManager
        if (GameStateManager.Instance != null)
        {
            Debug.Log($"[FallGameOver] Calling GameStateManager.GameOver() (cause={cause})");
            GameStateManager.Instance.GameOver();
        }
        else
        {
            Debug.LogError("[FallGameOver] GameStateManager.Instance is NULL. Ensure GameStateManager exists in the scene.");
        }
    }
}
