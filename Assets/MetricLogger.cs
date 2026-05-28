using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class LoopData
{
    public int loopId;
    public string taskType;
    public float searchDurationSeconds;
    public int incorrectPickups;
    public bool taskCompleted;
    public string choiceMade;
    public float choiceSpeedSeconds;
    public int stressAfterChoice;
}

[System.Serializable]
public class SessionData
{
    public string sessionId;
    public string timestamp;
    public List<LoopData> loops = new List<LoopData>();
    public float totalDurationSeconds;
    public int finalStressLevel;
    public string completionStatus;
}

public class MetricLogger : MonoBehaviour
{
    private SessionData currentSession;
    private LoopData currentLoop;
    private float sessionStartTime;
    private int loopCount = 0;

    private void Start()
    {
        sessionStartTime = Time.time;
        currentSession = new SessionData();
        currentSession.sessionId = System.Guid.NewGuid().ToString();
        currentSession.timestamp = System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

        currentLoop = new LoopData();
        currentLoop.loopId = 1;
        currentLoop.taskType = "find_medicine";
    }

    public void RecordItemFound(string itemName, float searchDuration)
    {
        currentLoop.searchDurationSeconds = searchDuration;
        currentLoop.taskCompleted = true;
    }

    public void RecordIncorrectPickup()
    {
        currentLoop.incorrectPickups++;
    }

    public void RecordChoice(string choice, float choiceSpeed, int stressAfter)
    {
        currentLoop.choiceMade = choice;
        currentLoop.choiceSpeedSeconds = choiceSpeed;
        currentLoop.stressAfterChoice = stressAfter;
        currentSession.loops.Add(currentLoop);
    }

    public void SaveSession(int finalStress)
    {
        currentSession.totalDurationSeconds = Time.time - sessionStartTime;
        currentSession.finalStressLevel = finalStress;
        currentSession.completionStatus = "success";

        string json = JsonUtility.ToJson(currentSession, true);
        string path = Application.persistentDataPath + "/session_" + currentSession.sessionId + ".json";
        File.WriteAllText(path, json);

        Debug.Log("Session saved to: " + path);
        Debug.Log(json);
    }
}