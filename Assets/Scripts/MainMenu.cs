using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField nameInputField;
    public Button startButton;
    public TextMeshProUGUI errorText;

    private void Start()
    {
        // load previous name if exists
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            nameInputField.text =
                PlayerPrefs.GetString("PlayerName");
        }

        errorText.gameObject.SetActive(false);
        startButton.onClick.AddListener(OnStartClicked);
    }

    private void OnStartClicked()
    {
        // validate name
        if (string.IsNullOrEmpty(
            nameInputField.text.Trim()))
        {
            errorText.gameObject.SetActive(true);
            errorText.text = "Please enter your name!";
            return;
        }

        // save name
        PlayerPrefs.SetString("PlayerName",
            nameInputField.text.Trim());
        PlayerPrefs.Save();

        // load game
        SceneManager.LoadScene(1);
    }
}