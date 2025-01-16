using System.Collections;
using UnityEngine;
using TMPro;

public class MessageManager : MonoBehaviour
{
    public static MessageManager Instance { get; private set; }

    public GameObject messagePanel;
    public TextMeshProUGUI messageText;
    public float displayDuration = 2f;

    private Coroutine currentMessageCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowMessage(string message)
    {
        // If a message is already being displayed, stop it
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
        }

        // Start a new message display
        currentMessageCoroutine = StartCoroutine(DisplayMessage(message));
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
}