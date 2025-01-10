using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System;

public class CustomObjectForm : MonoBehaviour
{
    public TMP_Dropdown typeDropdown;
    public TMP_InputField itemNameInput;
    public TMP_InputField descriptionInput;
    public TMP_InputField maxConnectionsInput;
    public GameObject powerConsumptionBlock;
    public TMP_InputField powerConsumptionInput;
    public TMP_InputField priceInput;

    // Type-specific fields
    public GameObject nvrFields;
    public TMP_InputField maxChannelsInput;

    public GameObject upsFields;
    public TMP_InputField maxBatteriesInput;

    public GameObject batteryFields;
    public TMP_InputField powerWattsInput;

    public Button saveButton;
    public TextMeshProUGUI errorText;

    private void Start()
    {
        InitializeTypeDropdown();
        typeDropdown.onValueChanged.AddListener(OnTypeChanged);
        saveButton.onClick.AddListener(SaveObject);
    }

    private void InitializeTypeDropdown()
    {
        typeDropdown.ClearOptions();
        var options = new List<string> { "Camera", "Switch", "AccessController", "NVR", "UPS", "Battery" };
        typeDropdown.AddOptions(options);
        OnTypeChanged(0); // Set default to Camera
    }

    private void OnTypeChanged(int index)
    {
        string selectedType = typeDropdown.options[index].text;

        // Show/hide type-specific fields
        nvrFields.SetActive(selectedType == "NVR");
        upsFields.SetActive(selectedType == "UPS");
        batteryFields.SetActive(selectedType == "Battery");
        powerConsumptionBlock.SetActive(
            selectedType != "UPS" && selectedType != "Battery");
    }

    private bool ValidateInputs()
    {
        errorText.text = "";
        string selectedType = typeDropdown.options[typeDropdown.value].text;

        if (string.IsNullOrEmpty(itemNameInput.text))
        {
            errorText.text = "Item name is required";
            return false;
        }

        if (!int.TryParse(maxConnectionsInput.text, out _))
        {
            errorText.text = "Max connections must be a number";
            return false;
        }

        if (!float.TryParse(priceInput.text, out _))
        {
            errorText.text = "Price must be a number";
            return false;
        }

        if (selectedType != "UPS" && selectedType != "Battery")
        {
            if (!int.TryParse(powerConsumptionInput.text, out _))
            {
                errorText.text = "Power consumption must be a number";
                return false;
            }
        }

        switch (selectedType)
        {
            case "NVR":
                if (!int.TryParse(maxChannelsInput.text, out _))
                {
                    errorText.text = "Max channels must be a number";
                    return false;
                }
                break;
            case "UPS":
                if (!int.TryParse(maxBatteriesInput.text, out _))
                {
                    errorText.text = "Max batteries must be a number";
                    return false;
                }
                break;
            case "Battery":
                if (!int.TryParse(powerWattsInput.text, out _))
                {
                    errorText.text = "Power watts must be a number";
                    return false;
                }
                break;
        }

        return true;
    }

    private void SaveObject()
    {
        if (!ValidateInputs()) return;

        var newItem = new CatalogItemData
        {
            itemName = itemNameInput.text,
            description = descriptionInput.text,
            type = typeDropdown.options[typeDropdown.value].text,
            maxConnections = int.Parse(maxConnectionsInput.text),
            price = float.Parse(priceInput.text),
            icon = GetDefaultIcon(typeDropdown.options[typeDropdown.value].text),
            prefab = GetDefaultPrefab(typeDropdown.options[typeDropdown.value].text),
            connectableTypes = GetDefaultConnectableTypes(typeDropdown.options[typeDropdown.value].text),
            mountTags = GetDefaultMountTags(typeDropdown.options[typeDropdown.value].text)
        };

        // Set type-specific fields
        switch (newItem.type)
        {
            case "NVR":
                newItem.maxChannels = int.Parse(maxChannelsInput.text);
                newItem.powerConsumption = int.Parse(powerConsumptionInput.text);
                break;
            case "UPS":
                newItem.maxBatteries = int.Parse(maxBatteriesInput.text);
                break;
            case "Battery":
                newItem.powerWatts = int.Parse(powerWattsInput.text);
                break;
            default:
                newItem.powerConsumption = int.Parse(powerConsumptionInput.text);
                break;
        }

        SaveToCustomCatalog(newItem);
    }

    private string GetDefaultIcon(string type)
    {
        switch (type)
        {
            case "Camera": return "dummy_camera";
            case "Switch": return "switch1";
            default: return "turnstile3";
        }
    }

    private string GetDefaultPrefab(string type)
    {
        return $"Mashes/{type}";
    }

    private List<string> GetDefaultConnectableTypes(string type)
    {
        switch (type)
        {
            case "Camera": return new List<string> { "Switch" };
            case "Switch": return new List<string> { "Camera", "Switch", "NVR" };
            case "AccessController": return new List<string> { "Turnstile" };
            case "NVR": return new List<string> { "Switch" };
            case "UPS": return new List<string> { "UPS", "Switch" };
            default: return new List<string>();
        }
    }

    private List<string> GetDefaultMountTags(string type)
    {
        switch (type)
        {
            case "Camera": return new List<string> { "Wall", "Ceiling" };
            case "Battery": return new List<string> { "UPS" };
            default: return new List<string> { "Floor" };
        }
    }

    private void SaveToCustomCatalog(CatalogItemData newItem)
    {
        string path = Path.Combine(Application.dataPath, "Resources/ObjectCreation/custom_catalog.json");
        CatalogItemsList catalog = null;

        // Check both default and custom catalogs for duplicate names
        bool isDuplicate = false;

        // Check default catalog
        CatalogItemsList defaultCatalog = JsonUtility.FromJson<CatalogItemsList>(
            FindObjectOfType<CatalogManager>().jsonFile.text);
        isDuplicate = defaultCatalog.items.Exists(item => 
            item.itemName.Equals(newItem.itemName, StringComparison.OrdinalIgnoreCase));

        if (!isDuplicate)
        {
            // Check custom catalog if it exists
            if (File.Exists(path))
            {
                string jsonContent = File.ReadAllText(path);
                catalog = JsonUtility.FromJson<CatalogItemsList>(jsonContent);
                isDuplicate = catalog.items.Exists(item => 
                    item.itemName.Equals(newItem.itemName, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                catalog = new CatalogItemsList { items = new List<CatalogItemData>() };
            }
        }

        if (isDuplicate)
        {
            errorText.text = "An item with this name already exists";
            return;
        }

        catalog.items.Add(newItem);
        string json = JsonUtility.ToJson(catalog, true);
        File.WriteAllText(path, json);

        // Notify CatalogManager to reload
        FindObjectOfType<CatalogManager>().ReloadCatalog();
    }
}