using System;
using System.Collections.Generic;
using StarterAssets;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using SCUD3D;
using UnityEngine.UI;

public class UPSSettingsManager : MonoBehaviour
{
    public GameObject UPSSettings; // Reference to the objectSettings
    public UPS UPSObject; // Reference to the InteractiveObject

    //Connected devices UI
    public TextMeshProUGUI connectionsCountText; // Reference to the Text for connection count
    public TextMeshProUGUI objectNameText; // Reference to the Text for object name
    public ScrollRect connectionsScroll; // Reference to the Scroll View
    public GameObject connectionItemPrefab; // Prefab for displaying connection items
    public Button deleteConnectionButton; // Reference to the delete button
    public Button addConnectionButton; // Reference to the add button
    private Connection selectedConnection; // Store the selected connection ID

    //Installed batteries UI
    public TextMeshProUGUI batteriesCountText; // Reference to the Text for connection count
    public ScrollRect installedBatteriesScroll; // Reference to the Scroll View
    public GameObject batteryItemPrefab; // Prefab for displaying connection items
    public Button deleteBatteryButton; // Reference to the delete button
    private string selectedBatteryId; // Store the selected connection ID


    private CablePlacer CablePlacer;

    public void Start()
    {
        CablePlacer = GetComponent<CablePlacer>();
    }

    public void ShowMenu(UPS obj)
    {
        UPSObject = obj;
        UpdateMenu();
        UPSSettings.SetActive(true);
    }

    void Update()
    {
        // Check if the menu is active and the Escape key is pressed
        if (UPSSettings.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }
    }

    public void CloseMenu()
    {
        UPSSettings.SetActive(false);
    }

    public void UpdateMenu()
    {
        int currentObjectConnectionsCount = ConnectionsManager.Instance.GetAllConnections(UPSObject).Count;
        // Update connection count text
        connectionsCountText.text = $"{currentObjectConnectionsCount}";
        objectNameText.text = UPSObject.name;

        batteriesCountText.text = $"{UPSObject.connectedBatteries.Count} / {UPSObject.maxBatteries}";

        FillConnections();
        FillBatteries();

        // Update button visibility
        deleteConnectionButton.interactable = selectedConnection != null;
        addConnectionButton.interactable = currentObjectConnectionsCount < UPSObject.maxConnections;

        deleteBatteryButton.interactable = selectedBatteryId !=null;
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
        foreach (Transform child in connectionsScroll.content)
        {
            Destroy(child.gameObject);
        }
        List<Connection> currentObjectconnections = ConnectionsManager.Instance.GetAllConnections(UPSObject);
        // Populate the scroll view with connected device IDs
        foreach (var connection in currentObjectconnections)
        {
            InteractiveObject otherObject = connection.ObjectA == UPSObject ? connection.ObjectB : connection.ObjectA;
            GameObject item = Instantiate(connectionItemPrefab, connectionsScroll.content);
            item.GetComponentInChildren<TextMeshProUGUI>().text = otherObject.id;
            Button button = item.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => SelectConnection(connection));
        }
    }

    public void SelectBattery(string batteryId)
    {
        selectedBatteryId = batteryId;
        UpdateMenu();
    }

    public void DeleteBattery()
    {
        if (selectedBatteryId != null)
        {
            ObjectManager.Instance.RemoveObject(selectedBatteryId);
            UPSObject.connectedBatteries.Remove(selectedBatteryId);
            selectedBatteryId = null;
            UpdateMenu();
        }
    }

    public void FillBatteries()
    {
        // Clear existing items in the scroll view
        foreach (Transform child in installedBatteriesScroll.content)
        {
            Destroy(child.gameObject);
        }
        // Populate the scroll view with connected batteries
        foreach (var batteryId in UPSObject.connectedBatteries)
        {
            GameObject item = Instantiate(batteryItemPrefab, installedBatteriesScroll.content);
            item.GetComponentInChildren<TextMeshProUGUI>().text = batteryId;
            Button button = item.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => SelectBattery(batteryId));
        }
    }
}