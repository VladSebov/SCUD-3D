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
    public GameObject selectConnectionForm;
    public int currentCableType;
    public InteractiveObject interactiveObject; // Reference to the InteractiveObject
    public TextMeshProUGUI connectionCountText; // Reference to the Text for connection count
    public ScrollRect scrollView; // Reference to the Scroll View
    public GameObject connectionItemPrefab; // Prefab for displaying connection items
    public Button deleteButton; // Reference to the delete button
    public Button addConnectionButton; // Reference to the add button
    private Connection selectedConnection; // Store the selected connection ID

    //UPS settings
    public TextMeshProUGUI UPSInfoText;
    public Button UPSActionButton;

    private CablePlacer CablePlacer;
    public MenuDevicesManager MenuDevicesManager; // скрипт для menuDevices
    public UPSSettingsManager UPSSettingsManager; // скрипт для menuDevices

    public void Start()
    {
        CablePlacer = GetComponent<CablePlacer>();
        MenuDevicesManager = GetComponent<MenuDevicesManager>();
        UPSSettingsManager = GetComponent<UPSSettingsManager>();
    }

    public void ShowSelectConnectionForm(int cableType)
    {
        currentCableType = cableType;
        selectConnectionForm.SetActive(true);
    }

    public void AddConnectionManually()
    {
        CloseMenu();
        CablePlacer.StartCablePlacement(interactiveObject, currentCableType);
    }

    public void ShowMenu(InteractiveObject obj)
    {
        interactiveObject = obj;
        if (interactiveObject.type == ObjectType.UPS)
        {
            UPSSettingsManager.ShowMenu((UPS)interactiveObject);
        }
        else
        {
            UpdateMenu();
            objectSettings.SetActive(true);
        }
    }

    public void ShowAvailableDevices()
    {
        selectConnectionForm.SetActive(false);
        MenuDevicesManager.ShowMenu(interactiveObject, currentCableType);
    }

    void Update()
    {
        // Check if the menu is active and the Escape key is pressed
        if (objectSettings.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }
    }

    public void CloseMenu()
    {
        selectConnectionForm.SetActive(false);
        objectSettings.SetActive(false);
        UPSSettingsManager.CloseMenu();
    }

    public void UpdateMenu()
    {
        int currentObjectConnectionsCount = ConnectionsManager.Instance.GetConnections(interactiveObject).Count;
        // Update connection count text
        connectionCountText.text = $"{currentObjectConnectionsCount} / {interactiveObject.maxConnections}";

        FillConnections();

        FillUPSConnection();

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
    public void FillUPSConnection()
    {
        // Check if the interactive object implements ConnectableToUPS
        if (interactiveObject is ConnectableToUPS connectableToUPS)
        {
            // Check if connectedUPSId is not null
            if (!string.IsNullOrEmpty(connectableToUPS.connectedUPSId))
            {
                UPSInfoText.text = $"Connected to UPS: {connectableToUPS.connectedUPSId}";
                UPSActionButton.gameObject.SetActive(true);
                UPSActionButton.onClick.RemoveAllListeners(); // Clear previous listeners
                UPSActionButton.onClick.AddListener(() =>
                {
                    connectableToUPS.connectedUPSId = null;
                    UpdateMenu(); // Refresh the menu
                });
                UPSActionButton.GetComponentInChildren<TextMeshProUGUI>().text = "Отключить";
            }
            else
            {
                UPSInfoText.text = "Not connected to any UPS";
                UPSActionButton.gameObject.SetActive(true);
                UPSActionButton.onClick.RemoveAllListeners(); // Clear previous listeners
                UPSActionButton.onClick.AddListener(() =>
                {
                    ShowSelectConnectionForm(CableType.UPS);
                });
                UPSActionButton.GetComponentInChildren<TextMeshProUGUI>().text = "Подключить";
            }
        }
        else
        {
            UPSInfoText.text = "This device doesn't support UPS";
            UPSActionButton.gameObject.SetActive(false);
        }
    }

}