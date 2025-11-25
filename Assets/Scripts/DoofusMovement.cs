using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DoofusMovement : MonoBehaviour
{
    private Rigidbody rb;
    private float moveSpeed = 4f; // default until JSON loads

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Use FixedUpdate for physics movement
    private void FixedUpdate()
    {
        // Update speed from JSON if available (safe null checks)
        if (GameConfig.Instance != null && GameConfig.Instance.IsLoaded && 
            GameConfig.Instance.diary != null && GameConfig.Instance.diary.player_data != null)
        {
            moveSpeed = GameConfig.Instance.diary.player_data.speed;
        }

        float h = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right arrows
        float v = Input.GetAxisRaw("Vertical");   // W/S or Up/Down arrows

        Vector3 dir = new Vector3(h, 0f, v).normalized;

        if (dir.sqrMagnitude > 0.0001f)
        {
            Vector3 newPos = rb.position + dir * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }
    }
}
