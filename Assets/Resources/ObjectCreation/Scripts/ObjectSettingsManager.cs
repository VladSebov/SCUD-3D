using System;
using System.Collections.Generic;
using StarterAssets;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using SCUD3D;
using UnityEngine.UI;

public class ObjectSettingsManager : MonoBehaviour
{
    public GameObject objectSettings; // Reference to the objectSettings
    public InteractiveObject interactiveObject; // Reference to the InteractiveObject
    public TextMeshProUGUI connectionCountText; // Reference to the Text for connection count
    public ScrollRect scrollView; // Reference to the Scroll View
    public GameObject connectionItemPrefab; // Prefab for displaying connection items
    public Button deleteButton; // Reference to the delete button
    public Button addConnectionButton; // Reference to the add button
    private string selectedConnectionId; // Store the selected connection ID

    private CablePlacer CablePlacer;

    public void Start()
    {
        CablePlacer = GetComponent<CablePlacer>();
        addConnectionButton.onClick.AddListener(OnAddConnectionButtonClick);
    }

    private void OnAddConnectionButtonClick()
    {
        if (interactiveObject != null && CablePlacer != null)
        {
            CablePlacer.StartCablePlacement(interactiveObject.gameObject.transform.position);
        }
        else
        {
            Debug.LogError("CablePlacer or InteractiveObject is not assigned!");
        }
    }

    public void ShowMenu(InteractiveObject obj)
    {

        interactiveObject = obj;
        UpdateMenu();
        objectSettings.SetActive(true);
    }

    void Update()
    {
        // Check if the menu is active and the Escape key is pressed
        if (objectSettings.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }
    }

    private void CloseMenu()
    {
        objectSettings.SetActive(false);
    }

    public void UpdateMenu()
    {
        // Update connection count text
        connectionCountText.text = $"{interactiveObject.connections.Count} / {interactiveObject.maxConnections}";

        FillConnections();

        // Update button visibility
        deleteButton.interactable = !string.IsNullOrEmpty(selectedConnectionId);
        addConnectionButton.interactable = interactiveObject.connections.Count < interactiveObject.maxConnections;
    }

    public void SelectConnection(string connectionId)
    {
        selectedConnectionId = connectionId;
        UpdateMenu();
    }

    public void DeleteConnection()
    {
        if (!string.IsNullOrEmpty(selectedConnectionId))
        {
            ObjectManager.Instance.DisconnectObjects(interactiveObject.id, selectedConnectionId);
            selectedConnectionId = null;
            UpdateMenu();
        }
    }

    public void FillConnections()
    {
        // Clear existing items in the scroll view
        foreach (Transform child in scrollView.content)
        {
            Destroy(child.gameObject);
        }

        // Populate the scroll view with connected device IDs
        foreach (var connectionId in interactiveObject.connections)
        {
            GameObject item = Instantiate(connectionItemPrefab, scrollView.content);
            item.GetComponentInChildren<TextMeshProUGUI>().text = connectionId;
            Button button = item.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => SelectConnection(connectionId));
        }
    }

    void OnButtonClick(string deviceId)
    {
        Debug.Log("Button clicked for Item " + deviceId);
    }
}