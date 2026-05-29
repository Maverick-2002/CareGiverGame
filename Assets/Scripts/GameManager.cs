using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Volume globalVolume;
    private Vignette vignette;
    private ColorAdjustments colorAdjustments;

    [Header("Game State")]
    public float grandpaStress = 0f;
    public float maxStress = 100f;
    public int score = 0;
    public int tasksCompleted = 0;
    public float sessionTime = 0f;
    public bool gameActive = false;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public Slider stressSlider;
    public GameObject gameOverPanel;
    public GameObject successPanel;

    [Header("Stress Visuals")]
    public Light roomLight;
    public Color normalColor = Color.white;
    public Color stressColor = new Color(0.8f, 0.4f, 0.4f);

    [Header("Audio")]
    public AudioSource bgmSource;
    public AudioClip correctSFX;
    public AudioClip wrongSFX;
    public AudioSource sfxSource;
    public AudioClip playerConfusion;
    public AudioClip heartbeatSFX;
    private bool heartbeatPlaying = false;

    // metrics
    private int incorrectPickups = 0;
    private int correctChoices = 0;
    private int wrongChoices = 0;
    private float currentTaskTimer = 0f;
    private float stressTimer = 0f;
    public float stressIncreaseInterval = 5f;
    public float stressIncreaseAmount = 5f;
    private float peakStress = 0f;

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
        sfxSource = GetComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.Play();
        globalVolume.profile.TryGet(out vignette);
        globalVolume.profile.TryGet(out colorAdjustments);
        gameActive = true;
        UpdateUI();
        NPC_Controller.Instance.StartTask(0);
    }

    private void Update()
    {
        if (!gameActive) return;
        MetricLogger.Instance.TrackStress(grandpaStress);
        MetricLogger.Instance.TrackSessionTime(sessionTime);
        MetricLogger.Instance.TrackScore(score);
        HandleStressIncrease();
        UpdateTimers();
        UpdateTimerUI();
        CheckGameOver();
    }

    private void HandleStressIncrease()
    {
        stressTimer += Time.deltaTime;

        if (stressTimer >= stressIncreaseInterval)
        {
            stressTimer = 0f;
            AddStress(stressIncreaseAmount);
        }
    }

    private void UpdateTimers()
    {
        sessionTime += Time.deltaTime;
        currentTaskTimer += Time.deltaTime;
    }

    private void UpdateTimerUI()
    {
        timerText.text = "Time: " + Mathf.FloorToInt(sessionTime) + "s";
        stressSlider.value = grandpaStress / maxStress;
        UpdateStressVisuals();
    }

    private void CheckGameOver()
    {
        if (grandpaStress >= maxStress)
        {
            EndGame(false);
        }
    }
    public void playerConfusionSFX()
    {
       sfxSource.PlayOneShot(playerConfusion);
    }
    public void AddStress(float amount)
    {
        grandpaStress = Mathf.Clamp(grandpaStress + amount, 0f, maxStress);
        wrongChoices++;
        HandleHeartbeat();
    }
    private void HandleHeartbeat()
    {
        if (grandpaStress > 70f && !heartbeatPlaying)
        {
            heartbeatPlaying = true;
            sfxSource.clip = heartbeatSFX;
            sfxSource.loop = true;
            sfxSource.Play();
        }
        else if (grandpaStress <= 70f && heartbeatPlaying)
        {
            heartbeatPlaying = false;
            sfxSource.loop = false;
            sfxSource.Stop();
        }
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
        sfxSource.PlayOneShot(correctSFX);
    }

    public void OnIncorrectPickup()
    {
        incorrectPickups++;
        AddStress(5f);
        sfxSource.PlayOneShot(wrongSFX);
        MetricLogger.Instance.TrackIncorrectPickup();
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
        MetricLogger.Instance.TrackTaskCompleted();
    }

    private void UpdateStressVisuals()
    {
        if (roomLight == null) return;

        float t = grandpaStress / maxStress;

        // track peak stress
        if (grandpaStress > peakStress)
            peakStress = grandpaStress;

        roomLight.color = Color.Lerp(normalColor, stressColor, t);
        roomLight.intensity = Mathf.Lerp(1f, 0.3f, t);

        if (vignette != null)
            vignette.intensity.value = Mathf.Lerp(0.3f, 0.75f, t);

        if (colorAdjustments != null)
            colorAdjustments.saturation.value =
                Mathf.Lerp(0f, -100f, t);

        // pitch increase for urgency
        if (bgmSource != null)
            bgmSource.pitch = Mathf.Lerp(1f, 1.4f, t);
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

        // save success state
        PlayerPrefs.SetInt("GameSuccess", success ? 1 : 0);

        // save all metrics
        PlayerPrefs.SetInt("Score", score);
        PlayerPrefs.SetInt("TasksCompleted", tasksCompleted);
        PlayerPrefs.SetInt("CorrectChoices", correctChoices);
        PlayerPrefs.SetInt("WrongChoices", wrongChoices);
        PlayerPrefs.SetFloat("SessionTime", sessionTime);
        PlayerPrefs.SetFloat("PeakStress", peakStress);
        PlayerPrefs.Save();

        StartCoroutine(SendToAWS());

        SceneManager.LoadScene(2);
    }

    private IEnumerator SendToAWS()
    {
        MetricLogger.Instance.SendMetrics();
        yield return null;
    }
}
