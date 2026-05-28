using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public float grandpaStress = 0f;
    public float maxStress = 100f;
    public int score = 0;
    public int tasksCompleted = 0;
    public float sessionTime = 0f;
    public bool gameActive = false;

    [Header("UI")]
    public Text scoreText;
    public Text timerText;
    public Slider stressSlider;
    public GameObject gameOverPanel;
    public GameObject successPanel;

    [Header("Stress Visuals")]
    public Light roomLight;
    public Color normalColor = Color.white;
    public Color stressColor = new Color(0.8f, 0.4f, 0.4f);

    // metrics
    private int incorrectPickups = 0;
    private int correctChoices = 0;
    private int wrongChoices = 0;
    private float currentTaskTimer = 0f;

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
        gameActive = true;
        UpdateUI();
    }

    private void Update()
    {
        if (!gameActive) return;

        sessionTime += Time.deltaTime;
        currentTaskTimer += Time.deltaTime;

        timerText.text = "Time: "
            + Mathf.FloorToInt(sessionTime) + "s";

        stressSlider.value = grandpaStress / maxStress;

        UpdateStressVisuals();

        if (grandpaStress >= maxStress)
        {
            EndGame(false);
        }
    }

    public void AddStress(float amount)
    {
        grandpaStress = Mathf.Clamp(
            grandpaStress + amount, 0f, maxStress);
        wrongChoices++;
    }

    public void ReduceStress(float amount)
    {
        grandpaStress = Mathf.Clamp(
            grandpaStress - amount, 0f, maxStress);
        correctChoices++;
    }

    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = "Score: " + score;
    }

    public void OnIncorrectPickup()
    {
        incorrectPickups++;
        AddStress(5f);
    }

    public void OnTaskCompleted()
    {
        tasksCompleted++;
        currentTaskTimer = 0f;
        AddScore(100);

        if (tasksCompleted >= 3)
        {
            EndGame(true);
        }
    }

    private void UpdateStressVisuals()
    {
        if (roomLight == null) return;

        float t = grandpaStress / maxStress;
        roomLight.color = Color.Lerp(normalColor, stressColor, t);
    }

    private void UpdateUI()
    {
        scoreText.text = "Score: 0";
        stressSlider.value = 0f;
    }

    private void EndGame(bool success)
    {
        gameActive = false;
        ThirdPersonCamera.OpenUI();

        // save metrics
        PlayerPrefs.SetInt("Score", score);
        PlayerPrefs.SetInt("TasksCompleted", tasksCompleted);
        PlayerPrefs.SetInt("IncorrectPickups", incorrectPickups);
        PlayerPrefs.SetInt("CorrectChoices", correctChoices);
        PlayerPrefs.SetInt("WrongChoices", wrongChoices);
        PlayerPrefs.SetFloat("SessionTime", sessionTime);
        PlayerPrefs.Save();

        // send to AWS
        StartCoroutine(SendToAWS());

        if (success)
            successPanel.SetActive(true);
        else
            gameOverPanel.SetActive(true);
    }

    private IEnumerator SendToAWS()
    {
        // fill this after AWS setup
        yield return null;
    }
}