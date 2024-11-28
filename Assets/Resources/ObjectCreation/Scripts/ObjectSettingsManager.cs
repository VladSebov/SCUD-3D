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
    private Connection selectedConnection; // Store the selected connection ID

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
            CablePlacer.StartCablePlacement(interactiveObject);
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
        int currentObjectConnectionsCount = ConnectionsManager.Instance.GetConnections(interactiveObject).Count;
        // Update connection count text
        connectionCountText.text = $"{currentObjectConnectionsCount} / {interactiveObject.maxConnections}";

        FillConnections();

        // Update button visibility
        deleteButton.interactable = selectedConnection != null;
        addConnectionButton.interactable = currentObjectConnectionsCount < interactiveObject.maxConnections;
    }

    public void SelectConnection(Connection connection)
    {
        selectedConnection = connection;
        UpdateMenu();
    }

    public void DeleteConnection()
    {
        if (selectedConnection != null)
        {
            ConnectionsManager.Instance.RemoveConnection(selectedConnection);
            selectedConnection = null;
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
        List<Connection> currentObjectconnections = ConnectionsManager.Instance.GetConnections(interactiveObject);
        // Populate the scroll view with connected device IDs
        foreach (var connection in currentObjectconnections)
        {
            InteractiveObject otherObject = connection.ObjectA == interactiveObject ? connection.ObjectB : connection.ObjectA;
            GameObject item = Instantiate(connectionItemPrefab, scrollView.content);
            item.GetComponentInChildren<TextMeshProUGUI>().text = otherObject.id;
            Button button = item.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => SelectConnection(connection));
        }
    }

    void OnButtonClick(string deviceId)
    {
        Debug.Log("Button clicked for Item " + deviceId);
    }
}