using UnityEngine;
using TMPro;
using UnityEngine.Rendering;

[RequireComponent(typeof(Collider))]
public class Pulpit : MonoBehaviour
{
    public enum TimerPlacement { Center, FrontLeft, FrontRight }

    [Header("Optional: assign a child 3D TextMeshPro for the timer (world-space TMP)")]
    public TextMeshPro timerText;   // if left null the script will create a world-space TextMeshPro

    [Header("Runtime-created timer settings (tuned for reference look)")]
    public TimerPlacement placement = TimerPlacement.FrontLeft;
    public float timerYOffset = 0.6f;        // vertical offset above pulpit top
    [Tooltip("World-space font size")]
    public float timerFontSize = 80f;
    [Tooltip("Scale of the TMP transform to match world units")]
    public float runtimeLocalScale = 0.012f;
    public Color timerColor = Color.white;
    public Color outlineColor = Color.black;
    [Range(0f, 1f)]
    public float outlineThickness = 0.20f;   // strong outline for readability

    private float destroyTime = 5f;
    private float timer;
    private bool scored = false;
    public float TimerRemaining => Mathf.Max(0f, timer);

    private GameObject runtimeTimerGO;

    private void Start()
    {
        // Try to find an existing world-space TMP if user attached one in the prefab instance
        if (timerText == null)
            timerText = GetComponentInChildren<TextMeshPro>();

        // get pulpit top half-height from collider
        float pulpitHalfHeight = 0f;
        var col = GetComponent<Collider>();
        if (col != null) pulpitHalfHeight = col.bounds.extents.y;

        // Create a 3D TextMeshPro if none found
        if (timerText == null)
            CreateFloatingTimer(pulpitHalfHeight);
        else
            StyleAndPlaceExistingTimer(pulpitHalfHeight);

        // Read destroy time from GameConfig (safe fallback)
        if (GameConfig.Instance != null && GameConfig.Instance.IsLoaded && GameConfig.Instance.diary != null)
        {
            var p = GameConfig.Instance.diary.pulpit_data;
            if (p != null)
                destroyTime = Random.Range(p.min_pulpit_destroy_time, p.max_pulpit_destroy_time);
        }

        timer = destroyTime;

        if (timerText != null)
            timerText.text = timer.ToString("0.00");

        // schedule destruction (OnDestroy will notify spawner)
        Destroy(gameObject, destroyTime);

        Debug.Log($"[Pulpit] Spawned at {transform.position}, destroyTime={destroyTime:F2}");
    }

    private void Update()
    {
        if (timer <= 0f) return;

        timer -= Time.deltaTime;
        float display = Mathf.Max(0f, timer);

        if (timerText != null)
            timerText.text = display.ToString("0.00");

        // billboard to camera while staying upright
        if (timerText != null && Camera.main != null)
        {
            Transform t = timerText.transform;
            Vector3 toCam = Camera.main.transform.position - t.position;
            toCam.y = 0f; // keep text upright (no pitch)
            if (toCam.sqrMagnitude > 0.0001f)
                t.rotation = Quaternion.LookRotation(toCam);
        }
    }

    private void CreateFloatingTimer(float pulpitHalfHeight)
    {
        // Create container (world-space)
        runtimeTimerGO = new GameObject("TimerTMP");
        runtimeTimerGO.transform.SetParent(transform, false);

        // compute local placement
        Vector3 localPos = GetPlacementLocalPosition(pulpitHalfHeight);
        runtimeTimerGO.transform.localPosition = localPos;
        runtimeTimerGO.transform.localRotation = Quaternion.identity;
        runtimeTimerGO.transform.localScale = Vector3.one * runtimeLocalScale;

        // Add TextMeshPro (3D) component (not UI)
        var tmp = runtimeTimerGO.AddComponent<TextMeshPro>();
        tmp.text = "";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = timerFontSize;
        tmp.color = timerColor;
        tmp.enableAutoSizing = false;
        tmp.fontStyle = FontStyles.Bold;

        // Ensure we have a font asset (use default TMP font if available)
        if (tmp.font == null && TMPro.TMP_Settings.defaultFontAsset != null)
            tmp.font = TMPro.TMP_Settings.defaultFontAsset;

        // Configure material outline (works when font material supports outline)
        if (tmp.fontSharedMaterial != null)
        {
            tmp.fontSharedMaterial.EnableKeyword("OUTLINE_ON");
            tmp.fontSharedMaterial.SetColor("_OutlineColor", outlineColor);
            tmp.fontSharedMaterial.SetFloat("_OutlineWidth", outlineThickness);
        }

        // Make sure MeshRenderer is visible (disable shadows, increase sorting order)
        var mr = runtimeTimerGO.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.receiveShadows = false;
            // put on default sorting layer but make order high
            mr.sortingOrder = 5000;
        }

        // assign to field so Update uses it
        timerText = tmp;
    }

    private void StyleAndPlaceExistingTimer(float pulpitHalfHeight)
    {
        // Move existing TMP under pulpit and style it for world-space
        Transform t = timerText.transform;
        t.SetParent(transform, false);
        t.localPosition = GetPlacementLocalPosition(pulpitHalfHeight);
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one * runtimeLocalScale;

        timerText.alignment = TextAlignmentOptions.Center;
        timerText.fontSize = timerFontSize;
        timerText.color = timerColor;
        timerText.fontStyle = FontStyles.Bold;
        timerText.enableAutoSizing = false;

        if (timerText.fontSharedMaterial != null)
        {
            timerText.fontSharedMaterial.EnableKeyword("OUTLINE_ON");
            timerText.fontSharedMaterial.SetColor("_OutlineColor", outlineColor);
            timerText.fontSharedMaterial.SetFloat("_OutlineWidth", outlineThickness);
        }

        var mr = timerText.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.sortingOrder = 5000;
        }
    }

    private Vector3 GetPlacementLocalPosition(float pulpitHalfHeight)
    {
        // half side distance (platforms spawn ~9 unit apart, so half ~4)
        float halfSide = 4.0f;
        float y = pulpitHalfHeight + timerYOffset;

        switch (placement)
        {
            case TimerPlacement.FrontLeft:
                return new Vector3(-halfSide + 0.9f, y, -halfSide + 0.9f);
            case TimerPlacement.FrontRight:
                return new Vector3(halfSide - 0.9f, y, -halfSide + 0.9f);
            default:
                return new Vector3(0f, y, 0f);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (scored) return;

        if (other.gameObject.GetComponent<DoofusMovement>() != null)
        {
            scored = true;
            ScoreManager.Instance?.AddScore(1);
            Debug.Log("[Pulpit] Scored by Doofus.");
        }
    }

    private void OnDestroy()
    {
        if (runtimeTimerGO != null)
            Destroy(runtimeTimerGO);

        PulpitSpawner.Instance?.NotifyPulpitDestroyed(this);
    }
}
