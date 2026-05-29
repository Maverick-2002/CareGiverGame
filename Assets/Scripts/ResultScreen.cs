using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class ResultScreen : MonoBehaviour
{
    [Header("Success Panel")]
    public GameObject successPanel;

    [Header("Fail Panel")]
    public GameObject failPanel;

    [Header("Metrics Panel")]
    public GameObject metricsPanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI tasksText;
    public TextMeshProUGUI choicesText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI peakStressText;
    public TextMeshProUGUI empathyRatingText;
    public TextMeshProUGUI playerName;

    [Header("Fade")]
    public CanvasGroup canvasGroup;

    private bool isSuccess = false;

    private void Start()
    {
        // get result from PlayerPrefs
        isSuccess = PlayerPrefs.GetInt(
            "GameSuccess", 0) == 1;

        // hide all panels
        successPanel.SetActive(false);
        failPanel.SetActive(false);
        metricsPanel.SetActive(false);
        canvasGroup.alpha = 0f;
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        // fade in
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime * 0.8f;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        if (isSuccess)
            StartCoroutine(ShowSuccess());
        else
            StartCoroutine(ShowFail());
    }

    private IEnumerator ShowSuccess()
    {
        successPanel.SetActive(true);
        yield return new WaitForSeconds(1.5f);
    }

    private IEnumerator ShowFail()
    {
        failPanel.SetActive(true);
        yield return new WaitForSeconds(1.5f);
    }

    private IEnumerator TypeText(
        TextMeshProUGUI textObj, string message)
    {
        textObj.text = "";
        foreach (char c in message)
        {
            textObj.text += c;
            yield return new WaitForSeconds(0.03f);
        }
    }

    public void LoadMetrics()
    {
        string player = PlayerPrefs.GetString("PlayerName","");
        int score = PlayerPrefs.GetInt("Score", 0);
        int tasks = PlayerPrefs.GetInt(
            "TasksCompleted", 0);
        int correct = PlayerPrefs.GetInt(
            "CorrectChoices", 0);
        int wrong = PlayerPrefs.GetInt(
            "WrongChoices", 0);
        float time = PlayerPrefs.GetFloat(
            "SessionTime", 0f);
        float peakStress = PlayerPrefs.GetFloat(
            "PeakStress", 0f);
        int confusions = PlayerPrefs.GetInt(
            "ConfusionTriggers", 0);

        scoreText.text = "Final Score: " + score;
        tasksText.text = "Tasks Completed: " + tasks + "/3";
        choicesText.text = "Empathetic Responses: "
            + correct + "/3";
        timeText.text = "Completion Time: " +
            Mathf.FloorToInt(time) + "s";
        peakStressText.text = "Peak Stress: " +
            Mathf.FloorToInt(peakStress) + "%";
        playerName.text = player;

        // empathy rating
        string rating = "";
        if (correct == 3)
            rating = "⭐⭐⭐ Compassionate Caregiver";
        else if (correct == 2)
            rating = "⭐⭐ Caring and Learning";
        else if (correct == 1)
            rating = "⭐ Keep Practicing";
        else
            rating = "💙 Every Day Is A New Chance";

        empathyRatingText.text = rating;
    }

    public void OnReplay()
    {
        SceneManager.LoadScene(1);
    }

    public void OnMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}