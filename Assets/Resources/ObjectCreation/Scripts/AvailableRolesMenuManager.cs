using System;
using System.Collections.Generic;
using System.Linq;
using StarterAssets;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AvailableRolesMenuManager : MonoBehaviour
{
    public GameObject menuAvailableRoles; // Reference to the available roles
    public InteractiveObject interactiveObject; // Reference to the InteractiveObject
    public Transform scrollContent; // Reference to the Scroll View
    public GameObject availableRolePrefab; // Prefab for displaying available roles items
    public Button saveButton; // Reference to the save button
    private List<GameObject> roleItems = new List<GameObject>(); // Store all items to filter selected ones

    public void ShowMenu(InteractiveObject obj)
    {
        interactiveObject = obj;
        menuAvailableRoles.SetActive(true);
        UpdateMenu();
    }

    void Update()
    {
        // Check if the menu is active and the Escape key is pressed
        if (menuAvailableRoles.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }
    }

    public void CloseMenu()
    {
        menuAvailableRoles.SetActive(false);
    }

     public void UpdateMenu()
    {
        //FillRoles();
    }

    public void SelectDevice(string deviceId)
    {
        //selectedDeviceId = deviceId;
        UpdateMenu();
    }

    public void ConnectRoles() // You can modify this to get input from the user
    {
        // TODO() check wtf is even this, why would anyone connect object in ConnectRoles method
        if (interactiveObject.HasAvailablePorts())
        {
            //ObjectManager.Instance.ConnectObjects(interactiveObject.id, selectedDeviceId);
            CloseMenu();
        }
    }

    /* public void FillRoles()
    {
        // Clear existing items in the scroll view
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
            roleItems.Clear();
        }
        List<string> availableRoles = ScudManager.Instance.GetRoles();
        // Populate the scroll view with connected device IDs
        foreach (var role in availableRoles)
        {
            GameObject item = Instantiate(availableRolePrefab, scrollContent);
            item.GetComponentInChildren<TextMeshProUGUI>().text = role;
            roleItems.Add(item);
            if (interactiveObject is AccessController accessController)
            {
                if (accessController.allowedRoles != null && accessController.allowedRoles.Contains(role))
                {
                    // Assuming item is defined and is a GameObject
                    var toggle = item.GetComponentInChildren<Toggle>();
                    if (toggle != null)
                    {
                        toggle.isOn = true;
                    }
                    else
                    {
                        Debug.LogWarning("Toggle component not found in item.");
                    }
                }
                else
                {
                    Debug.LogWarning("allowedRoles is null or does not contain the specified role.");
                }
            }
            else
            {
                Debug.LogWarning("interactiveObject is not an AccessController.");
            }
            // Button button = item.GetComponentInChildren<Button>();

            // button.onClick.AddListener(() => SelectDevice(deviceId));
        }
    } */

    public void OnSaveButtonClick()
    {
        var selectedRoles = roleItems
        .Where(roleItem => roleItem.GetComponentInChildren<Toggle>().isOn == true)
        .Select(roleItem => roleItem.GetComponentInChildren<TextMeshProUGUI>().text)
        .ToList();
        ScudManager.Instance.UpdateAccessControllerRoles(interactiveObject.id, selectedRoles);
        // Assuming interactiveObject is of type InteractiveObject
        if (interactiveObject is AccessController accessController)
        {
            // Log the allowedRoles of the AccessController
            Debug.Log(string.Join(", ", accessController.allowedRoles));
        }
        else
        {
            Debug.LogWarning("The interactiveObject is not an AccessController.");
        }
    }
}