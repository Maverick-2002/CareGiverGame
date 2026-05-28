using System.Collections;
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

    // 0=comfort, 1=neutral, 2=stress
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
        StartTask(currentTask);
    }

    public void StartTask(int index)
    {
        if (index >= taskItemNames.Length)
        {
            // all tasks done
            GameManager.Instance.OnTaskCompleted();
            return;
        }

        requestPanel.SetActive(true);
        grandpaDialogueText.text = taskDialogues[index];

        // set correct item
        //SetCorrectItem(taskItemNames[index]);

        // confusion event after 10 seconds
        StartCoroutine(ConfusionEvent());
    }

    private void SetCorrectItem(string itemName)
    {
        PlayerInteractor[] allItems =
            FindObjectsOfType<PlayerInteractor>();

        foreach (var item in allItems)
        {
            item.isCorrectItem =
                (item.objectName == itemName);
        }
    }

    private IEnumerator ConfusionEvent()
    {
        yield return new WaitForSeconds(5f);

        if (!GameManager.Instance.gameActive) yield break;

        // grandpa gets confused
        grandpaDialogueText.text =
            confusionDialogues[currentTask];
        GameManager.Instance.AddStress(10f);

        // animator trigger
        if (grandpaAnimator != null)
        {
            grandpaAnimator.SetTrigger("Confused");
        }
    }

    public void OnCorrectItemFound()
    {
        StopAllCoroutines();
        requestPanel.SetActive(false);
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
                // comfort
                GameManager.Instance.ReduceStress(15f);
                GameManager.Instance.AddScore(50);
                ShowFeedback("Grandpa feels calm and cared for",
                    Color.green);
                grandpaDialogueText.text =
                    "Thank you so much beta...";

                if (grandpaAnimator != null)
                    grandpaAnimator.SetTrigger("Happy");
                break;

            case 1:
                // neutral
                GameManager.Instance.AddStress(5f);
                ShowFeedback("Try to be more empathetic",
                    Color.yellow);
                grandpaDialogueText.text = "Oh... okay.";
                break;

            case 2:
                // stress
                GameManager.Instance.AddStress(20f);
                ShowFeedback("This response caused distress",
                    Color.red);
                grandpaDialogueText.text =
                    "You don't understand...";

                if (grandpaAnimator != null)
                    grandpaAnimator.SetTrigger("Sad");
                break;
        }

        StartCoroutine(NextTask());
    }

    private void ShowFeedback(string message, Color color)
    {
        feedbackPanel.SetActive(true);
        feedbackText.text = message;
        feedbackText.color = color;
        StartCoroutine(HideFeedback());
    }

    private IEnumerator HideFeedback()
    {
        yield return new WaitForSeconds(2f);
        feedbackPanel.SetActive(false);
    }

    public void ShowWrongItemFeedback()
    {
        StartCoroutine(WrongItemMessage());
    }

    private IEnumerator WrongItemMessage()
    {
        feedbackPanel.SetActive(true);
        feedbackText.text = "That's not it...";
        feedbackText.color = Color.red;
        yield return new WaitForSeconds(1f);
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