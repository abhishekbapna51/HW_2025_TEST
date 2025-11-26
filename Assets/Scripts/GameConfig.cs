using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class PlayerData { public float speed = 5f; }

[System.Serializable]
public class PulpitData
{
    public float min_pulpit_destroy_time = 4f;
    public float max_pulpit_destroy_time = 5f;
    public float pulpit_spawn_time = 2.5f;
}

[System.Serializable]
public class DoofusDiary
{
    public PlayerData player_data = new PlayerData();
    public PulpitData pulpit_data = new PulpitData();
}

public class GameConfig : MonoBehaviour
{
    public static GameConfig Instance { get; private set; }

    public DoofusDiary diary;
    public bool IsLoaded => diary != null;

    private const string DiaryUrl =
        "https://s3.ap-south-1.amazonaws.com/superstars.assetbundles.testbuild/doofus_game/doofus_diary.json";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // set defaults immediately so other scripts can read sensible defaults before load completes
        diary = new DoofusDiary();

        StartCoroutine(LoadDiary());
    }

    private IEnumerator LoadDiary()
    {
        using (UnityWebRequest req = UnityWebRequest.Get(DiaryUrl))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("GameConfig: Failed to load Doofus diary (using defaults): " + req.error);
                yield break;
            }

            try
            {
                diary = JsonUtility.FromJson<DoofusDiary>(req.downloadHandler.text);
                if (diary == null) diary = new DoofusDiary();
                Debug.Log($"Diary loaded. Speed = {diary.player_data.speed}");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("GameConfig: JSON parse error, using defaults. " + ex.Message);
                diary = new DoofusDiary();
            }
        }
    }
}
