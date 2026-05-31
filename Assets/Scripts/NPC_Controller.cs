using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPC_Controller : MonoBehaviour
{
    public static NPC_Controller Instance;

    [Header("Tasks")]
    public string[] taskItemNames = {
        "Medicine",
        "FamilyPhoto",
        "Notepad"
    };

    public string[] taskDialogues = {
        "I can't find my medicine...",
        "Where is my family photo? I need to see it...",
        "Can you get me a notepad please..."
    };

    public string[] confusionDialogues = {
        "Did you take my medicine?!",
        "Someone moved my photo!",
        "I asked for notepad ages ago!"
    };

    private string[][] responseChoices = {
        new string[] {
            "Here it is, take your time",
            "You forgot where you kept it",
            "You need to be more careful"
        },
        new string[] {
            "Here's your photo, your family loves you",
            "It was right there the whole time",
            "You should keep track of your things"
        },
        new string[] {
            "Here's your notepad, no rush",
            "You keep forgetting things, write it quickly",
            "You should keep it with yourself all the time"
        }
    };

    private int[] choiceEffects = { 0, 1, 2 };

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
 
    [Header("Entry Animation")]
    public bool playerEnteredRoom = false;

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
        choicePanel.SetActive(false);
        feedbackPanel.SetActive(false);
        requestPanel.SetActive(false);
        grandpaAnimator.SetBool("IsStanding", false);
    }

    public void Update()
    {
        stress = GameManager.Instance.grandpaStress;
        grandpaAnimator.SetFloat("StressLevel", stress);
    }

    public void OnPlayerEnterRoom()
    {
        if (playerEnteredRoom) return;
        playerEnteredRoom = true;
        StartTask(0);
        StartCoroutine(StandUpSequence());
    }

    IEnumerator StandUpSequence()
    {
        yield return new WaitForSeconds(1f);

        requestPanel.SetActive(true);
        grandpaDialogueText.text = "Oh... someone's here.";
        grandpaAnimator.SetTrigger("StandUp");

        yield return new WaitForSeconds(2f);
        grandpaAnimator.SetBool("isStanding", true);
        StartTask(currentTask);

        yield return new WaitForSeconds(3f);
        grandpaAnimator.SetTrigger("isSearching");

        yield return new WaitForSeconds(6f);
        grandpaAnimator.SetTrigger("Confused");



    }

    public void StartTask(int index)
    {
        if (index >= taskItemNames.Length)
        {
            GameManager.Instance.OnTaskCompleted();
            return;
        }
        requestPanel.SetActive(true);
        grandpaDialogueText.text = taskDialogues[index];

        if (MetricLogger.Instance.isReady)
            MetricLogger.Instance.SendLiveUpdate();

        
        StartCoroutine(ConfusionEvent());
    }

    private IEnumerator ConfusionEvent()
    {
        yield return new WaitForSeconds(5f);
        grandpaDialogueText.text = confusionDialogues[currentTask];
        GameManager.Instance.AddStress(10f);
       
        MetricLogger.Instance.SendLiveUpdate();
    }

    public void OnCorrectItemFound()
    {
        StopAllCoroutines();
        requestPanel.SetActive(false);
        MetricLogger.Instance.SendLiveUpdate();
        ShowChoicePanel();
    }

    private void ShowChoicePanel()
    {
        choicePanel.SetActive(true);
        ThirdPersonCamera.OpenUI();

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i;
            choiceTexts[i].text =
                responseChoices[currentTask][i];
            choiceButtons[i].onClick.RemoveAllListeners();
            choiceButtons[i].onClick.AddListener(
                () => OnChoiceSelected(index));
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
                ShowFeedback("Grandpa feels calm and cared for",Color.green);
                grandpaDialogueText.text ="Thank you so much beta...";
                
                break;

            case 1:
                MetricLogger.Instance.TrackCorrectChoice();
                GameManager.Instance.AddStress(5f);
                ShowFeedback("Try to be more empathetic",Color.yellow);
                grandpaDialogueText.text = "Oh... okay.";
                break;

            case 2:
                MetricLogger.Instance.TrackCorrectChoice();
                GameManager.Instance.AddStress(20f);
                ShowFeedback("This response caused distress", Color.red);
                grandpaDialogueText.text ="You don't understand...";
               
                break;
        }

        MetricLogger.Instance.SendLiveUpdate();
        StartCoroutine(NextTask());
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
        if (success)
            grandpaAnimator.SetTrigger("Happy");
        else
            grandpaAnimator.SetTrigger("Sad");
    }

    private IEnumerator HideFeedback()
    {
        yield return new WaitForSeconds(2f);
        feedbackPanel.SetActive(false);
    }

    public void ShowWrongItemFeedback(
        string message = "You're forgetting where things are kept...")
    {
        StartCoroutine(WrongItemMessage(message));
    }

    private IEnumerator WrongItemMessage(string message)
    {
        feedbackPanel.SetActive(true);
        feedbackText.text = message;
        feedbackText.color = Color.red;
        yield return new WaitForSeconds(1.5f);
        feedbackPanel.SetActive(false);
    }

    private IEnumerator NextTask()
    {
        yield return new WaitForSeconds(1.5f);
        currentTask++;
        GameManager.Instance.OnTaskCompleted();

        if (currentTask < taskItemNames.Length)
        {
            StartTask(currentTask);
        }
    }
}