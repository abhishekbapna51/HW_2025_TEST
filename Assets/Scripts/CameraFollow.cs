using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                // Drag Doofus here

    [Header("Follow settings")]
    public Vector3 offset = new Vector3(0f, 6f, -10f); // default camera offset from target
    public float moveSmoothTime = 0.15f;
    public float rotateSmoothTime = 0.12f;

    [Header("Dynamic zoom (optional)")]
    public bool enableDynamicZoom = true;
    public float minFOV = 55f;
    public float maxFOV = 80f;
    public float zoomSmoothTime = 0.35f;
    public float pulpitSpreadToMaxFOV = 10f; // how large spread causes max zoom

    [Header("Bounds (optional)")]
    public bool clampToBounds = false;
    public Vector2 minXMaxX = new Vector2(-50f, 50f);
    public Vector2 minZMaxZ = new Vector2(-50f, 50f);

    Camera cam;
    Vector3 currentVelocity;
    float fovVelocity;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        if (target == null)
        {
            Debug.LogWarning("[CameraFollow] No target assigned. Drag your Doofus Transform into the inspector.");
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // desired world pos = target position + offset rotated by target yaw (optional)
        Vector3 desiredPos = target.position + offset;

        // optional clamping to level bounds
        if (clampToBounds)
        {
            desiredPos.x = Mathf.Clamp(desiredPos.x, minXMaxX.x, minXMaxX.y);
            desiredPos.z = Mathf.Clamp(desiredPos.z, minZMaxZ.x, minZMaxZ.y);
        }

        // smooth move
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref currentVelocity, moveSmoothTime);

        // smooth rotate to look at target (keep camera upright)
        Vector3 lookDir = (target.position - transform.position).normalized;
        Quaternion desiredRot = Quaternion.LookRotation(lookDir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.deltaTime / Mathf.Max(0.0001f, rotateSmoothTime));

        // dynamic zoom (adjust FOV)
        if (enableDynamicZoom && PulpitSpawner.Instance != null)
        {
            List<Vector3> pulpits = PulpitSpawner.Instance.GetActivePulpitsWorldPositions();
            float spread = CalculateMaxSpread(target.position, pulpits);
            float t = Mathf.Clamp01(spread / pulpitSpreadToMaxFOV);
            float desiredFOV = Mathf.Lerp(minFOV, maxFOV, t);
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, desiredFOV, ref fovVelocity, zoomSmoothTime);
        }
    }

    // returns the largest horizontal distance between the player and any pulpit (in world XZ plane)
    float CalculateMaxSpread(Vector3 playerPos, List<Vector3> pulpitPositions)
    {
        if (pulpitPositions == null || pulpitPositions.Count == 0) return 0f;

        float maxDist = 0f;
        foreach (var p in pulpitPositions)
        {
            Vector2 a = new Vector2(playerPos.x, playerPos.z);
            Vector2 b = new Vector2(p.x, p.z);
            float d = Vector2.Distance(a, b);
            if (d > maxDist) maxDist = d;
        }
        return maxDist;
    }
}
