using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class MetricLogger : MonoBehaviour
{
    public static MetricLogger Instance;

    [Header("AWS Settings")]
    public string apiEndpoint = "YOUR_API_GATEWAY_URL";

    private string sessionId = "";
    private int finalScore = 0;
    private int tasksCompleted = 0;
    private int incorrectPickups = 0;
    private float sessionTime = 0f;
    private List<float> taskTimes = new List<float>();
    private float taskStartTime = 0f;
    private int correctChoices = 0;
    private int wrongChoices = 0;
    private int neutralChoices = 0;
    private float peakStress = 0f;
    private int confusionTriggersHit = 0;
    public bool isReady = false;

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
        sessionId = System.Guid.NewGuid().ToString();
        isReady = true;
    }

    private void Start()
    {
        taskStartTime = Time.time;
        GameManager.Instance.OnScoreChanged.AddListener(TrackScore);
        GameManager.Instance.OnStressChanged.AddListener(TrackStress);
        GameManager.Instance.OnTaskCompletedEvent.AddListener(TrackTaskCompleted);
        GameManager.Instance.OnIncorrectPickupEvent.AddListener(TrackIncorrectPickup);
        GameManager.Instance.OnGameEnded.AddListener(OnGameEnded);
        SendLiveUpdate();
    }

    private void OnDisable()
    {
        GameManager.Instance.OnScoreChanged.RemoveListener(TrackScore);
        GameManager.Instance.OnStressChanged.RemoveListener(TrackStress);
        GameManager.Instance.OnTaskCompletedEvent.RemoveListener(TrackTaskCompleted);
        GameManager.Instance.OnIncorrectPickupEvent.RemoveListener(TrackIncorrectPickup);
        GameManager.Instance.OnGameEnded.RemoveListener(OnGameEnded);
    }

    public void TrackScore(int score)
    {
        finalScore = score;
    }
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
    public void OnGameEnded(bool success)
    {
        StartCoroutine(PostMetricsAndWaits());
    }

    public void TrackCorrectChoice()
    {
        correctChoices++;
    }

    public void TrackWrongChoice()
    {
        wrongChoices++;
        SendLiveUpdate();
    }

    public void TrackNeutralChoice()
    {
        neutralChoices++;
        SendLiveUpdate();
    }

    public void TrackConfusionTriggered()
    {
        confusionTriggersHit++;
        SendLiveUpdate();
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

    public void SendLiveUpdate()
    {
        StartCoroutine(PostMetricsAndWaits());
    }

    public IEnumerator PostMetricsAndWaits()
    {
        MetricsData data = new MetricsData
        {
            sessionId = sessionId,
            playerName = PlayerPrefs.GetString("PlayerName", "Player"),
            finalScore = finalScore,
            tasksCompleted = tasksCompleted,
            incorrectPickups = incorrectPickups,
            sessionTime = sessionTime,
            correctChoices = correctChoices,
            wrongChoices = wrongChoices,
            neutralChoices = neutralChoices,
            peakStress = peakStress,
            avgTaskTime = GetAverageTaskTime(),
            confusionTriggersHit = confusionTriggersHit
        };

        string json = JsonUtility.ToJson(data);
        Debug.Log("Sending: " + json);

        using (UnityWebRequest request = UnityWebRequest.Post(apiEndpoint, json, "application/json"))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("SUCCESS: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("FAILED: " + request.error);
            }
        }
    }
}

[System.Serializable]
public class MetricsData
{
    public string sessionId;
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
}