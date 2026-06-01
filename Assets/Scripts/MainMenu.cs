using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField nameInputField;
    public Button startButton;

    private void Start()
    {
        startButton.onClick.AddListener(OnStartClicked);
    }

    private void OnStartClicked()
    {
        PlayerPrefs.SetString("PlayerName",nameInputField.text.Trim());
        PlayerPrefs.Save();
        SceneManager.LoadScene(1);
    }
}