using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Object Settings")]
    public string objectName;
    public bool isCorrectItem;
    public float interactDistance = 2f;

    [Header("Confusion Settings")]
    public bool isConfusionObject = false;
    public GameObject confusionDuplicate;
    public float confusionTriggerDistance = 2.5f;

    [Header("Visuals")]
    public Material normalMaterial;
    public Material highlightMaterial;
    private Renderer objectRenderer;
    private Color originalColor;

    [Header("UI")]
    public GameObject interactPrompt;
    public GameObject playerObj;
    private Transform player;
    private bool playerNearby = false;
    private bool confusionTriggered = false;

    [Header("Task")]
    public int taskNumber;

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        originalColor = objectRenderer.material.color;
        player = playerObj.transform;
        interactPrompt.SetActive(false);
        confusionDuplicate.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance.currentTask != taskNumber)  return;

        playerNearby = true;
        ShowHighlight();

        if (isConfusionObject && !confusionTriggered)
        {
            confusionTriggered = true;
            TriggerConfusion();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = false;
        HideHighlight();
    }
    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            OnInteract();
        }
    }

    private void TriggerConfusion()
    {
        StartCoroutine(ConfusionSequence());
    }

    private IEnumerator ConfusionSequence()
    {
        GameManager.Instance.playerConfusionSFX();
        GameManager.Instance.TriggerBlur();
        yield return StartCoroutine(FadeOut());
        gameObject.SetActive(false);
        confusionDuplicate.SetActive(true);
        NPC_Controller.Instance.ShowWrongItemFeedback("He trusts you to find what his mind can no longer hold.");
        MetricLogger.Instance.TrackConfusionTriggered();
    }

    private IEnumerator FadeOut()
    {

        Material mat = objectRenderer.sharedMaterial;
        Color startColor = mat.color;
        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            mat.color = new Color(
                startColor.r,
                startColor.g,
                startColor.b,
                alpha);
            yield return null;
        }
    }

    private void OnInteract()
    {
        if (isCorrectItem)
        {
            HideHighlight();
            gameObject.SetActive(false);
            GameManager.Instance.currentTask++;
            NPC_Controller.Instance.OnCorrectItemFound();
        }
        else
        {
            GameManager.Instance.OnIncorrectPickup();
            StartCoroutine(WrongItemFeedback());
            NPC_Controller.Instance.ShowWrongItemFeedback("That's not the right item...");
        }
    }

    private void ShowHighlight()
    {
        objectRenderer.sharedMaterial = highlightMaterial;
        objectRenderer.sharedMaterial.color = Color.yellow;
        interactPrompt.SetActive(true);
    }

    private void HideHighlight()
    {
        objectRenderer.sharedMaterial = normalMaterial;
        objectRenderer.sharedMaterial.color = originalColor;
        interactPrompt.SetActive(false);
    }

    private IEnumerator WrongItemFeedback()
    {
        objectRenderer.sharedMaterial.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        objectRenderer.sharedMaterial.color = originalColor;
    }
}