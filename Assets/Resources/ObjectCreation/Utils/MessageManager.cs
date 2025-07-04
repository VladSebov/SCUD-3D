using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageManager : MonoBehaviour
{
    public static MessageManager Instance { get; private set; }

    // Для обычных сообщений
    public GameObject messagePanel;
    public TextMeshProUGUI messageText;
    private float displayDuration = 2f;
    private Coroutine currentMessageCoroutine;


    // Для подсказок (hints)
    public GameObject hintPanel;
    public GameObject EnterPanel;
    public Button EnterPanelCardButton;
    public Button EnterPanelFingerButton;
    public Button EnterPanelPasswordButton;
    public TMP_InputField EnterPanelPasswordInputField;
    public TextMeshProUGUI hintText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Для временных сообщений
    public void ShowMessage(string message)
    {
        if (currentMessageCoroutine != null) StopCoroutine(currentMessageCoroutine);
        currentMessageCoroutine = StartCoroutine(DisplayMessage(message));
    }

    // Для подсказок (постоянные)
    public void ShowHint(string hint)
    {
        hintPanel.SetActive(true);
        hintText.text = hint;
    }
    public void HideHint()
    {
        hintPanel.SetActive(false);
    }

    private IEnumerator DisplayMessage(string message)
    {
        // Show the panel and set the message
        messagePanel.SetActive(true);
        messageText.text = message;

        // Wait for the duration
        yield return new WaitForSeconds(displayDuration);

        // Hide the panel
        messagePanel.SetActive(false);
    }
    public void ShowEnterPanel()
    {
        EnterPanel.SetActive(true);
    }
    public void HideEnterPanel()
    {
        EnterPanel.SetActive(false);
    }

    public string GetPasswordFromEnterPanel()
    {
        return EnterPanelPasswordInputField.text;
    }
}