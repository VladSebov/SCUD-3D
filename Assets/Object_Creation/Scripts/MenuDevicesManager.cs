using System;
using System.Collections.Generic;
using StarterAssets;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MenuDevicesManager : MonoBehaviour
{
    public GameObject menuDevices; // Reference to the objectSettings
    public InteractiveObject interactiveObject; // Reference to the InteractiveObject
    public Transform scrollContent; // Reference to the Scroll View
    public GameObject availableItemPrefab; // Prefab for displaying connection items
    public Button connectButton; // Reference to the add button
    private string selectedDeviceId; // Store the selected connection ID

    public void ShowMenu(InteractiveObject obj)
    {
        interactiveObject = obj;
        menuDevices.SetActive(true);
        UpdateMenu();
    }

    void Update()
    {
        // Check if the menu is active and the Escape key is pressed
        if (menuDevices.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }
    }

    public void CloseMenu()
    {
        menuDevices.SetActive(false);
    }

    public void UpdateMenu()
    {
        FillDevices();
        connectButton.gameObject.SetActive(selectedDeviceId!=null);
    }

    public void SelectDevice(string deviceId)
    {
        selectedDeviceId = deviceId;
        UpdateMenu();
    }

    public void AddConnection(string newConnectionId) // You can modify this to get input from the user
    {
        if (interactiveObject.connections.Count < interactiveObject.maxConnections)
        {
            interactiveObject.connections.Add(newConnectionId);
            UpdateMenu();
        }
    }

    public void FillDevices()
    {
        // Clear existing items in the scroll view
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }
        List<string> availableDevices = ObjectManager.Instance.GetAvailableDevicesIDs(interactiveObject.id);
        // Populate the scroll view with connected device IDs
        foreach (var deviceId in availableDevices)
        {
            GameObject item = Instantiate(availableItemPrefab, scrollContent);
            item.GetComponentInChildren<TextMeshProUGUI>().text = deviceId;
            Button button = item.GetComponentInChildren<Button>();
            if (deviceId == selectedDeviceId)
            {
                button.GetComponent<Image>().color = Color.gray;
            }
            button.onClick.AddListener(() => SelectDevice(deviceId));
        }
    }

    void OnButtonClick(string deviceId)
    {
        Debug.Log("Button clicked for Item " + deviceId);
    }
}