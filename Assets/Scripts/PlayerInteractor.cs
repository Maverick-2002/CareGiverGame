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

    private async void TriggerConfusion()
    {
        // disable this object
        HideHighlight();
        gameObject.SetActive(false);

        // enable duplicate elsewhere
        if (confusionDuplicate != null)
            confusionDuplicate.SetActive(true);

        // show confusion message
        NPC_Controller.Instance.ShowWrongItemFeedback("You're forgetting where things are kept...");
        GameManager.Instance.playerConfusionSFX();
        await Task.Delay(2000);
        NPC_Controller.Instance.ShowWrongItemFeedback("Now Search the item in the room?!");
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