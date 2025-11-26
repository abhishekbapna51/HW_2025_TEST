using UnityEngine;

/// <summary>
/// Robust game-over detector:
/// - Primary: if object's Y <= fallYThreshold -> GameOver()
/// - Optional: if this object enters a trigger collider tagged "DeathPlane" -> GameOver()
/// - Disables DoofusMovement and Rigidbody motion on game over to prevent continued movement.
/// - Helpful Debug.Log lines to assist debugging in Console.
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
            Debug.Log($"[FallGameOver] Y threshold reached: y={transform.position.y:F2} <= {fallYThreshold:F2}");
            TriggerGameOver("YThreshold");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (invoked) return;

        if (!string.IsNullOrEmpty(deathPlaneTag) && other != null)
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

        // Disable move input/behaviour if present
        var move = GetComponent<DoofusMovement>();
        if (move != null)
        {
            move.enabled = false;
            Debug.Log("[FallGameOver] Disabled DoofusMovement.");
        }

        // Freeze physics and clear velocities
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            Debug.Log("[FallGameOver] Rigidbody stopped and set to kinematic.");
        }

        // Try to notify GameStateManager
        if (GameStateManager.Instance != null)
        {
            Debug.Log($"[FallGameOver] Calling GameStateManager.GameOver() (cause={cause})");
            GameStateManager.Instance.GameOver();
        }
        else
        {
            // Still try to show UI fallback if UIManager exists (defensive)
            Debug.LogError("[FallGameOver] GameStateManager.Instance is NULL. Making fallback call to UIManager.ShowGameOver() if available.");
            UIManager.Instance?.ShowGameOver();
        }
    }
}
