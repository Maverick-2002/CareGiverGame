using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

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

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null)
            originalColor = objectRenderer.material.color;


        if (playerObj != null)
            player = playerObj.transform;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        // hide duplicate at start
        if (confusionDuplicate != null)
            confusionDuplicate.SetActive(false);
    }

    private void Update()
    {
        if (!GameManager.Instance.gameActive) return;
        if (player == null) return;

        float dist = Vector3.Distance(
            transform.position, player.position);

        // confusion object logic
        if (isConfusionObject && !confusionTriggered)
        {
            if (dist <= confusionTriggerDistance)
            {
                confusionTriggered = true;
                TriggerConfusion();
                return;
            }
        }

        // normal interaction logic
        if (dist <= interactDistance)
        {
            if (!playerNearby)
            {
                playerNearby = true;
                ShowHighlight();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                OnInteract();
            }
        }
        else
        {
            if (playerNearby)
            {
                playerNearby = false;
                HideHighlight();
            }
        }
    }

    private void TriggerConfusion()
    {
        StartCoroutine(ConfusionSequence());
    }

    private IEnumerator ConfusionSequence()
    {
        GameManager.Instance.playerConfusionSFX();
        yield return StartCoroutine(FadeOut());
        gameObject.SetActive(false);
        confusionDuplicate.SetActive(true);
        NPC_Controller.Instance.ShowWrongItemFeedback("You know what you're looking for... but not where to find it.");
        yield return new WaitForSeconds(3f);
        MetricLogger.Instance.TrackConfusionTriggered();
    }

    private IEnumerator FadeOut()
    {
        if (objectRenderer == null) yield break;

        Material mat = objectRenderer.material;

        // set material to transparent mode
        mat.SetFloat("_Surface", 1); // 0 = opaque, 1 = transparent
        mat.SetFloat("_Blend", 0);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = 3000;

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
            NPC_Controller.Instance.OnCorrectItemFound();
           // GameManager.Instance.AddScore(100);
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
        if (highlightMaterial != null)
            objectRenderer.material = highlightMaterial;
        else if (objectRenderer != null)
            objectRenderer.material.color = Color.yellow;

        if (interactPrompt != null)
            interactPrompt.SetActive(true);
    }

    private void HideHighlight()
    {
        if (normalMaterial != null)
            objectRenderer.material = normalMaterial;
        else if (objectRenderer != null)
            objectRenderer.material.color = originalColor;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    private IEnumerator WrongItemFeedback()
    {
        if (objectRenderer != null)
            objectRenderer.material.color = Color.red;

        yield return new WaitForSeconds(0.3f);

        if (objectRenderer != null)
            objectRenderer.material.color = originalColor;
    }
}