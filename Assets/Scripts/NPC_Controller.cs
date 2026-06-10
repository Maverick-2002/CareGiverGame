using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPC_Controller : MonoBehaviour
{
    public static NPC_Controller Instance;
    private bool shouldLookAtPlayer;

    [Header("Tasks")]
    public string[] taskItemNames;
    public string[] taskDialogues;
    public string[] confusionDialogues;

    [Header("Response Choices")]
    public string[] task0Choices;
    public string[] task1Choices;
    public string[] task2Choices;

    [Header("Reaction Dialogues")]
    public string entryDialogue;
    public string[] comfortReactions;
    public string[] neutralReactions;
    public string[] stressReactions;
    private string[][] responseChoices;
    public int[] choiceEffects = { 0, 1, 2 };
    private int currentTask = 0;

    [Header("UI References")]
    public GameObject requestPanel;
    public TextMeshProUGUI grandpaDialogueText;
    public GameObject choicePanel;
    public Button[] choiceButtons;
    public TextMeshProUGUI[] choiceTexts;
    public TextMeshProUGUI feedbackText;
    public GameObject feedbackPanel;

    [Header("Animator")]
    public Animator grandpaAnimator;
    public float stress;
    public Transform playerTransform;

    [Header("Entry Animation")]
    public bool playerEnteredRoom = false;

    [Header("Voice")]
    public AudioSource npcVoice;
    public AudioClip entryVoiceClip;
    public TaskVoice[] taskVoices;
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

        responseChoices = new string[][]
        {
            task0Choices,
            task1Choices,
            task2Choices
        };
    }
    private void Start()
    {
        shouldLookAtPlayer = false;
        choicePanel.SetActive(false);
        feedbackPanel.SetActive(false);
        requestPanel.SetActive(false);
        grandpaAnimator.SetBool("isStanding", false);
        GameManager.Instance.OnStressChanged.AddListener(UpdateStressAnimation);
        UpdateStressAnimation(GameManager.Instance.grandpaStress);
        GameManager.Instance.OnGameEnded.AddListener(PlayEndAnimation);
        GameManager.Instance.OnTaskCompletedEvent.AddListener(StartNextTask);


    }
    private void OnDisable()
    {
        GameManager.Instance.OnStressChanged.RemoveListener(UpdateStressAnimation);
        GameManager.Instance.OnGameEnded.RemoveListener(PlayEndAnimation);
        GameManager.Instance.OnTaskCompletedEvent.RemoveListener(StartNextTask);
    }
    private void UpdateStressAnimation(float stressValue)
    {
        grandpaAnimator.SetFloat("StressLevel", stressValue);
    }
    public void OnPlayerEnterRoom()
    {
        if (playerEnteredRoom) return;
        playerEnteredRoom = true;
        StartCoroutine(StandUpSequence());
    }
    IEnumerator StandUpSequence()
    {
        yield return new WaitForSeconds(1f);
        requestPanel.SetActive(true);
        grandpaDialogueText.text = entryDialogue;
        npcVoice.PlayOneShot(entryVoiceClip);
        grandpaAnimator.SetTrigger("StandUp");

        yield return new WaitForSeconds(2f);
        grandpaAnimator.SetBool("isStanding", true);
        shouldLookAtPlayer = true;
        StartTask(0);

        yield return new WaitForSeconds(7f);
        shouldLookAtPlayer = false;
        grandpaAnimator.SetTrigger("isSearching");

    }
    public void StartTask(int index)
    {
        GameManager.Instance.gameActive = true;
        requestPanel.SetActive(true);
        npcVoice.PlayOneShot(taskVoices[index].taskClip);
        grandpaDialogueText.text = taskDialogues[index];     
        StartCoroutine(ConfusionEvent());
    }

    private IEnumerator ConfusionEvent()
    {
        yield return new WaitForSeconds(7f);
        npcVoice.PlayOneShot(taskVoices[currentTask].confusionClip);
        grandpaDialogueText.text = confusionDialogues[currentTask];
        yield return new WaitForSeconds(10f);
        grandpaDialogueText.text = taskDialogues[currentTask];
    }

    public void OnCorrectItemFound()
    {
        StopAllCoroutines();
        requestPanel.SetActive(false);
        ShowChoicePanel();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (shouldLookAtPlayer)
        {
            grandpaAnimator.SetLookAtWeight(1f);
            grandpaAnimator.SetLookAtPosition(playerTransform.position + Vector3.up * 1.6f);
        }
    }

    private void ShowChoicePanel()
    {
        choicePanel.SetActive(true);
        ThirdPersonCamera.OpenUI();
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i;
            choiceTexts[i].text = responseChoices[currentTask][i];
            choiceButtons[i].onClick.RemoveAllListeners();
            choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(index));
        }
    }

    public void OnChoiceSelected(int choiceIndex)
    {
        choicePanel.SetActive(false);
        ThirdPersonCamera.CloseUI();
        switch (choiceEffects[choiceIndex])
        {
            case 0:
                MetricLogger.Instance.TrackCorrectChoice();
                GameManager.Instance.ReduceStress(15f);
                GameManager.Instance.AddScore(50);
                ShowFeedback("Grandpa feels calm and cared for!", new Color(0.2f, 0.8f, 0.3f, 1f));
                grandpaDialogueText.text = comfortReactions[currentTask];
                grandpaAnimator.SetTrigger("Idle");
                grandpaAnimator.SetBool("isStanding", false);
                break;

            case 1:
                MetricLogger.Instance.TrackNeutralChoice();
                GameManager.Instance.AddStress(5f);
                ShowFeedback("Try to be more empathetic!", new Color(0.9f, 0.7f, 0.1f, 1f));
                grandpaDialogueText.text = neutralReactions[currentTask];
                grandpaAnimator.SetBool("isStanding", true);
                break;

            case 2:
                MetricLogger.Instance.TrackWrongChoice();
                GameManager.Instance.AddStress(20f);
                ShowFeedback("He appears confused and upset!", new Color(0.8f, 0.2f, 0.2f, 1f));
                grandpaDialogueText.text = stressReactions[currentTask];
                grandpaAnimator.SetTrigger("Confused");
                break;
        }
        GameManager.Instance.OnTaskCompleted();
    }

    private void ShowFeedback(string message, Color color)
    {
        feedbackPanel.SetActive(true);
        feedbackText.text = message;
        feedbackText.color = color;
        StartCoroutine(HideFeedback());
    }

    public void PlayEndAnimation(bool success)
    {
        shouldLookAtPlayer = false;
        if (success)
        {
            grandpaAnimator.SetTrigger("Happy");
            shouldLookAtPlayer = true;
        }
            
        else
            grandpaAnimator.SetTrigger("Sad");
    }

    private IEnumerator HideFeedback()
    {
        yield return new WaitForSeconds(2f);
        feedbackPanel.SetActive(false);
    }

    public void ShowWrongItemFeedback(string message)
    {
        StartCoroutine(WrongItemMessage(message));
    }

    private IEnumerator WrongItemMessage(string message)
    {
        feedbackPanel.SetActive(true);
        feedbackText.text = message;
        feedbackText.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        yield return new WaitForSeconds(2f);
        feedbackPanel.SetActive(false);
    }
    private void StartNextTask()
    {
        StartCoroutine(NextTask());
    }
    private IEnumerator NextTask()
    {
        yield return new WaitForSeconds(1.5f);
        currentTask++;
        if (currentTask < taskItemNames.Length)
        {
            StartTask(currentTask);
        }
    }

    [System.Serializable]
    public class TaskVoice
    {
        public AudioClip taskClip;
        public AudioClip confusionClip;
    }
}