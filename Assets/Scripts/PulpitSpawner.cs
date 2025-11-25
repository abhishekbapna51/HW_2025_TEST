using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PulpitSpawner : MonoBehaviour
{
    public static PulpitSpawner Instance { get; private set; }

    [Tooltip("Drag Pulpit prefab here (must have Pulpit.cs)")]
    public GameObject pulpitPrefab;

    private List<Pulpit> pulpits = new List<Pulpit>();
    private Vector3 currentPos = Vector3.zero;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        // Wait until GameConfig JSON is ready
        yield return new WaitUntil(() =>
            GameConfig.Instance != null && GameConfig.Instance.IsLoaded);

        // First pulpit
        SpawnPulpit(currentPos);

        while (true)
        {
            var p = GameConfig.Instance?.diary?.pulpit_data;
            float spawnDelay = p != null ? p.pulpit_spawn_time : 2.5f;

            yield return new WaitForSeconds(spawnDelay);

            Vector3 next = GetNextPosition();
            SpawnPulpit(next);
            currentPos = next;

            CleanupNulls();

            // Keep only TWO active pulpits
            if (pulpits.Count > 2)
            {
                if (pulpits[0] != null)
                    Destroy(pulpits[0].gameObject);

                CleanupNulls();
            }
        }
    }

    private void SpawnPulpit(Vector3 pos)
    {
        if (pulpitPrefab == null)
        {
            Debug.LogError("PulpitSpawner: PREFAB IS NOT ASSIGNED!");
            return;
        }

        GameObject go = Instantiate(pulpitPrefab, pos, Quaternion.identity);

        Pulpit p = go.GetComponent<Pulpit>();

        if (p == null)
        {
            Debug.LogError("Pulpit prefab has NO Pulpit.cs script!");
            return;
        }

        pulpits.Add(p);
    }

    private Vector3 GetNextPosition()
    {
        switch (Random.Range(0, 4))
        {
            case 0: return currentPos + new Vector3(9f, 0f, 0f);
            case 1: return currentPos + new Vector3(-9f, 0f, 0f);
            case 2: return currentPos + new Vector3(0f, 0f, 9f);
            default: return currentPos + new Vector3(0f, 0f, -9f);
        }
    }

    private void CleanupNulls()
    {
        pulpits.RemoveAll(p => p == null);
    }

    public void NotifyPulpitDestroyed(Pulpit p)
    {
        if (pulpits.Contains(p))
            pulpits.Remove(p);
    }

    // ------------------------------
    // Helpers for Camera + TimerUI
    // ------------------------------

    public List<Vector3> GetActivePulpitsWorldPositions()
    {
        CleanupNulls();
        List<Vector3> list = new();
        foreach (var p in pulpits)
            if (p != null) list.Add(p.transform.position);

        return list;
    }

    public List<Pulpit> GetActivePulpitsComponents()
    {
        CleanupNulls();
        return new List<Pulpit>(pulpits); // shallow copy
    }
}
