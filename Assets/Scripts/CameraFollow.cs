using UnityEngine;
using System.Collections.Generic;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 6f, -10f);
    public float followSpeed = 6f;
    public float lookAtHeightOffset = 1.5f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;

        // Try to gently include pulpits if available
        if (PulpitSpawner.Instance != null)
        {
            List<Vector3> pulpits = PulpitSpawner.Instance.GetActivePulpitsWorldPositions();
            if (pulpits != null && pulpits.Count > 0)
            {
                Vector3 avg = Vector3.zero;
                foreach (var p in pulpits) avg += p;
                avg /= pulpits.Count;
                Vector3 combined = Vector3.Lerp(target.position, avg, 0.35f);
                desiredPos = combined + offset;
            }
        }

        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * followSpeed);

        Vector3 lookAtPoint = target.position + Vector3.up * lookAtHeightOffset;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookAtPoint - transform.position), Time.deltaTime * followSpeed * 0.6f);
    }
}
