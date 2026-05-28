using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Object Settings")]
    public string objectName;
    public bool isCorrectItem;
    public float interactDistance = 2f;

    [Header("Visuals")]
    public Material normalMaterial;
    public Material highlightMaterial;
    private Renderer objectRenderer;
    private Color originalColor;

    [Header("UI")]
    public GameObject interactPrompt;

    private Transform player;
    private bool playerNearby = false;

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null)
            originalColor = objectRenderer.material.color;

        // find player by tag
        GameObject playerObj =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    private void Update()
    {
        if (!GameManager.Instance.gameActive) return;
        if (player == null) return;

        float dist = Vector3.Distance(
            transform.position, player.position);

        if (dist <= interactDistance)
        {
            if (!playerNearby)
            {
                playerNearby = true;
                ShowHighlight();
            }

            // press E to interact
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

    private void OnInteract()
    {
        if (isCorrectItem)
        {
            HideHighlight();
            gameObject.SetActive(false);
            NPC_Controller.Instance.OnCorrectItemFound();
            GameManager.Instance.AddScore(100);
        }
        else
        {
            // wrong item — flash red + stress
            GameManager.Instance.OnIncorrectPickup();
            StartCoroutine(WrongItemFeedback());

            // show wrong item message
            NPC_Controller.Instance.ShowWrongItemFeedback();
        }
    }

    private void ShowHighlight()
    {
        if (highlightMaterial != null)
        {
            objectRenderer.material = highlightMaterial;
        }
        else if (objectRenderer != null)
        {
            objectRenderer.material.color = Color.yellow;
        }

        if (interactPrompt != null)
            interactPrompt.SetActive(true);
    }

    private void HideHighlight()
    {
        if (normalMaterial != null)
        {
            objectRenderer.material = normalMaterial;
        }
        else if (objectRenderer != null)
        {
            objectRenderer.material.color = originalColor;
        }

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    private IEnumerator WrongItemFeedback()
    {
        // flash red
        if (objectRenderer != null)
            objectRenderer.material.color = Color.red;

        yield return new WaitForSeconds(0.3f);

        if (objectRenderer != null)
            objectRenderer.material.color = originalColor;
    }
}