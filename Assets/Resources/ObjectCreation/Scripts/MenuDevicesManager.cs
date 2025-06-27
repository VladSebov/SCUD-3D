using System;
using System.Collections.Generic;
using System.Linq;
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
    private int currentCableType; // Store the selected connection ID
    private CablePlacer CablePlacer;

    public void Start()
    {
        CablePlacer = GetComponent<CablePlacer>();
    }

    public void ShowMenu(InteractiveObject obj, int cableType)
    {
        interactiveObject = obj;
        currentCableType = cableType;
        selectedDeviceId = null;
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
        connectButton.interactable = selectedDeviceId != null;
    }

    public void SelectDevice(string deviceId)
    {
        selectedDeviceId = deviceId;
        UpdateMenu();
    }

    public void ConnectDevices() // You can modify this to get input from the user
    {
        CloseMenu();
        InteractiveObject selectedObject = ObjectManager.Instance.GetObject(selectedDeviceId);
        CablePlacer.AutoMountCable(interactiveObject, selectedObject, currentCableType);
    }

    public void FillDevices()
    {
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }
        List<string> availableDevices = new List<string>();

        if (currentCableType == CableType.Ethernet)
        {
            availableDevices = ObjectManager.Instance.GetAvailableDevicesIDs(interactiveObject.id);
        }
        else if (currentCableType == CableType.UPS)
        {
            if (interactiveObject.type == ObjectType.UPS)
            {
                //Show devices that aren't connected to current UPS
                availableDevices = ObjectManager.Instance.GetAvailableDevicesIDs(interactiveObject.id)
                    .Where(deviceId => !((UPS)interactiveObject).connectedDevices.Contains(deviceId))
                    .ToList();
            }
            else
            {
                //Show all UPS 
                availableDevices = ObjectManager.Instance.GetObjectsByType(ObjectType.UPS);
            }
        }
        foreach (var deviceId in availableDevices)
        {
            GameObject item = Instantiate(availableItemPrefab, scrollContent);
            item.GetComponentInChildren<TextMeshProUGUI>().text = deviceId;
            Button button = item.GetComponentInChildren<Button>();
            if (deviceId == selectedDeviceId)
            {
                button.GetComponent<Image>().color = Color.gray;
            }
            Debug.Log("finding bug 00");
            Debug.Log(deviceId);
            Debug.Log(button);
            button.onClick.AddListener(() => SelectDevice(deviceId));
        }
    }
}