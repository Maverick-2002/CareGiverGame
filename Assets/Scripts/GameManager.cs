using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Volume globalVolume;
    private Vignette vignette;
    private ColorAdjustments colorAdjustments;
    private DepthOfField depthOfField;

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

    [Header("Unity Events")]
    public UnityEvent<float> OnStressChanged;
    public UnityEvent<int> OnScoreChanged;
    public UnityEvent OnTaskCompletedEvent;
    public UnityEvent<bool> OnGameEnded;
    public UnityEvent OnIncorrectPickupEvent;

    // metrics
    private int incorrectPickups = 0;
    private int correctChoices = 0;
    private int wrongChoices = 0;
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
        OnStressChanged = new UnityEvent<float>();
        OnScoreChanged = new UnityEvent<int>();
        OnTaskCompletedEvent = new UnityEvent();
        OnGameEnded = new UnityEvent<bool>();
        OnIncorrectPickupEvent = new UnityEvent();
    }

    private void Start()
    {
        sfxSource = GetComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.Play();
        globalVolume.profile.TryGet(out vignette);
        globalVolume.profile.TryGet(out colorAdjustments);
        globalVolume.profile.TryGet(out depthOfField);
        gameActive = false;
        UpdateUI();
    }

    private void Update()
    {
        if (!gameActive) return;
        HandleStressIncrease();
        UpdateTimers();
    }

    public void TriggerBlur()
    {
        StartCoroutine(BlurEffect());
    }

    private IEnumerator BlurEffect()
    {
        depthOfField.active = true;
        float elapsed = 0f;
        float duration = 4f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float blur;
            if (t < 0.3f)
            {
                blur = Mathf.Lerp(0f, 25f, t / 0.3f);
            }
            else
            {
                blur = Mathf.Lerp(25f, 0f, (t - 0.3f) / 0.7f);
            }
            depthOfField.gaussianMaxRadius.value = blur;
            yield return null;
        }

        depthOfField.gaussianMaxRadius.value = 0f;
        depthOfField.active = false;
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
        timerText.text = "Time: " + Mathf.FloorToInt(sessionTime) + "s";
        stressSlider.value = grandpaStress / maxStress;
        UpdateStressVisuals();
    }
    public void playerConfusionSFX()
    {
        sfxSource.PlayOneShot(playerConfusion);
    }

    public void AddStress(float amount)
    {
        grandpaStress = Mathf.Clamp(grandpaStress + amount, 0f, maxStress);
        HandleHeartbeat();
        OnStressChanged?.Invoke(grandpaStress);
        if (grandpaStress >= maxStress)
        {
            EndGame(false);
        }
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
        grandpaStress = Mathf.Clamp(grandpaStress - amount, 0f, maxStress);
        correctChoices++;
        OnStressChanged?.Invoke(grandpaStress);
    }

    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = "Score: " + score;
        sfxSource.PlayOneShot(correctSFX);
        OnScoreChanged?.Invoke(score);
    }

    public void OnIncorrectPickup()
    {
        incorrectPickups++;
        AddStress(5f);
        sfxSource.PlayOneShot(wrongSFX);
        OnIncorrectPickupEvent?.Invoke();
    }

    public void OnTaskCompleted()
    {
        tasksCompleted++;
        AddScore(100);
        OnTaskCompletedEvent?.Invoke();
        if (tasksCompleted >= 3)
        {
            EndGame(true);
            return;
            
        }
            
    }

    private void UpdateStressVisuals()
    {
        if (roomLight == null) return;
        float t = grandpaStress / maxStress;

        if (grandpaStress > peakStress)
            peakStress = grandpaStress;

        roomLight.color = Color.Lerp(normalColor, stressColor, t);
        roomLight.intensity = Mathf.Lerp(1f, 0.3f, t);

        if (vignette != null)
            vignette.intensity.value = Mathf.Lerp(0.3f, 0.75f, t);

        if (colorAdjustments != null)
            colorAdjustments.saturation.value = Mathf.Lerp(0f, -50f, t);

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
        PlayerPrefs.SetInt("GameSuccess", success ? 1 : 0);
        PlayerPrefs.SetInt("Score", score);
        PlayerPrefs.SetInt("TasksCompleted", tasksCompleted);
        PlayerPrefs.SetInt("CorrectChoices", correctChoices);
        PlayerPrefs.SetInt("WrongChoices", wrongChoices);
        PlayerPrefs.SetFloat("SessionTime", sessionTime);
        PlayerPrefs.SetFloat("PeakStress", peakStress);
        PlayerPrefs.Save();
        OnGameEnded?.Invoke(success);
        StartCoroutine(LoadResultScene());
    }

    private IEnumerator LoadResultScene()
    {
        yield return new WaitForSeconds(2.5f);
        SceneManager.LoadScene(2);
    }
}