using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DoofusMovement : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Base Movement Speed (used if JSON not loaded)")]
    public float inspectorSpeed = 4f;   // faster default speed

    private float activeSpeed;          // final speed used

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        activeSpeed = inspectorSpeed;   // initial speed
    }

    private void FixedUpdate()
    {
        // Load speed from JSON if available (this overrides inspector)
        if (GameConfig.Instance != null &&
            GameConfig.Instance.IsLoaded &&
            GameConfig.Instance.diary != null &&
            GameConfig.Instance.diary.player_data != null)
        {
            activeSpeed = GameConfig.Instance.diary.player_data.speed;

            // OPTIONAL: Multiply JSON speed for faster gameplay 
            activeSpeed *= 1.5f;  // uncomment if needed
        }
        else
        {
            // fallback → use inspector speed
            activeSpeed = inspectorSpeed;
        }

        // Input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 dir = new Vector3(h, 0f, v).normalized;

        if (dir.sqrMagnitude > 0.001f)
        {
            Vector3 newPos = rb.position + dir * activeSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }
    }
}
