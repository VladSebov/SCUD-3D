using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectSettingsManager : MonoBehaviour
{
    public GameObject itemTemplate; // Assign the ItemTemplate prefab here
    public Transform content; // Assign the Content GameObject here

    public void FillAvailableDevices(List<string> availableDevices)
    {
        // Clear old items before adding new ones
        foreach (Transform child in content)
        {
            Destroy(child.gameObject); // Destroy all previous child objects
        }
        foreach (string deviceId in availableDevices) // Example: create 20 items
        {
            GameObject newItem = Instantiate(itemTemplate, content);
            newItem.SetActive(true); // Activate the item

            // Set the text for the item
            TextMeshProUGUI itemText = newItem.GetComponentInChildren<TextMeshProUGUI>();
            itemText.text = deviceId;

            // Set up the button
            Button itemButton = newItem.GetComponentInChildren<Button>();
            itemButton.onClick.AddListener(() => OnButtonClick(deviceId)); // Pass the item number
        }
    }

    void OnButtonClick(string deviceId)
    {
        Debug.Log("Button clicked for Item " + deviceId);
    }
}