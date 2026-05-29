using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class MetricLogger : MonoBehaviour
{
    public static MetricLogger Instance;

    [Header("AWS Settings")]
    public string apiEndpoint = "YOUR_API_GATEWAY_URL";

    // Gameplay Metrics
    private int finalScore = 0;
    private int tasksCompleted = 0;
    private int incorrectPickups = 0;
    private float sessionTime = 0f;

    // Behavioral Metrics
    private int correctChoices = 0;
    private int wrongChoices = 0;
    private int neutralChoices = 0;
    private float peakStress = 0f;

    // Engagement Metrics
    private int confusionTriggersHit = 0;
    private int replayCount = 0;

    // Task timing
    private List<float> taskTimes = new List<float>();
    private float taskStartTime = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        replayCount = PlayerPrefs.GetInt("ReplayCount", 0);
        taskStartTime = Time.time;
    }

    // called every frame from GameManager
    public void TrackStress(float currentStress)
    {
        if (currentStress > peakStress)
            peakStress = currentStress;
    }

    public void TrackTaskCompleted()
    {
        tasksCompleted++;
        float timeTaken = Time.time - taskStartTime;
        taskTimes.Add(timeTaken);
        taskStartTime = Time.time;
    }

    public void TrackIncorrectPickup()
    {
        incorrectPickups++;
    }

    public void TrackCorrectChoice()
    {
        correctChoices++;
    }

    public void TrackWrongChoice()
    {
        wrongChoices++;
    }

    public void TrackNeutralChoice()
    {
        neutralChoices++;
    }

    public void TrackConfusionTriggered()
    {
        confusionTriggersHit++;
    }

    public void TrackScore(int score)
    {
        finalScore = score;
    }

    public void TrackSessionTime(float time)
    {
        sessionTime = time;
    }

    private float GetAverageTaskTime()
    {
        if (taskTimes.Count == 0) return 0f;
        float total = 0f;
        foreach (float t in taskTimes)
            total += t;
        return total / taskTimes.Count;
    }

    public void SendMetrics()
    {
        replayCount++;
        PlayerPrefs.SetInt("ReplayCount", replayCount);
        PlayerPrefs.Save();

        StartCoroutine(PostMetrics());
    }

    private IEnumerator PostMetrics()
    {
        // build JSON
        string json = JsonUtility.ToJson(new MetricsData
        {
            playerName = PlayerPrefs.GetString(
                "PlayerName", "Player"),
            finalScore = finalScore,
            tasksCompleted = tasksCompleted,
            incorrectPickups = incorrectPickups,
            sessionTime = sessionTime,
            correctChoices = correctChoices,
            wrongChoices = wrongChoices,
            neutralChoices = neutralChoices,
            peakStress = peakStress,
            avgTaskTime = GetAverageTaskTime(),
            confusionTriggersHit = confusionTriggersHit,
            replayCount = replayCount
        });

        Debug.Log("Sending metrics: " + json);

        UnityWebRequest request = new UnityWebRequest(
            apiEndpoint, "POST");

        byte[] bodyRaw =
            System.Text.Encoding.UTF8.GetBytes(json);

        request.uploadHandler =
            new UploadHandlerRaw(bodyRaw);
        request.downloadHandler =
            new DownloadHandlerBuffer();
        request.SetRequestHeader(
            "Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result ==
            UnityWebRequest.Result.Success)
        {
            Debug.Log("Metrics sent successfully!");
        }
        else
        {
            Debug.LogError("Failed: " +
                request.error);
        }
    }
}

[System.Serializable]
public class MetricsData
{
    public string playerName;
    public int finalScore;
    public int tasksCompleted;
    public int incorrectPickups;
    public float sessionTime;
    public int correctChoices;
    public int wrongChoices;
    public int neutralChoices;
    public float peakStress;
    public float avgTaskTime;
    public int confusionTriggersHit;
    public int replayCount;
}