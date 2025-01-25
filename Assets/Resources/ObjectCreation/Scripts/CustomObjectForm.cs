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
        var options = new List<string> { "Камера", "Коммутатор", "СКУД контроллер", "Видеорегистратор", "ИБП", "Аккумулятор" };
        typeDropdown.AddOptions(options);
        OnTypeChanged(0); // Set default to Camera
    }

    private void OnTypeChanged(int index)
    {
        string selectedType = typeDropdown.options[index].text;

        // Show/hide type-specific fields
        nvrFields.SetActive(selectedType == "Видеорегистратор");
        upsFields.SetActive(selectedType == "ИБП");
        batteryFields.SetActive(selectedType == "Аккумулятор");
        powerConsumptionBlock.SetActive(
            selectedType != "ИБП" && selectedType != "Аккумулятор");
    }

    private bool ValidateInputs()
    {
        errorText.text = "";
        string selectedType = typeDropdown.options[typeDropdown.value].text;

        if (string.IsNullOrEmpty(itemNameInput.text))
        {
            errorText.text = "Имя элемента обязательно";
            return false;
        }

        if (!int.TryParse(maxConnectionsInput.text, out _))
        {
            errorText.text = "Максимальное количество подключений должно быть числом";
            return false;
        }

        if (!float.TryParse(priceInput.text, out _))
        {
            errorText.text = "Цена должна быть числом";
            return false;
        }

        if (selectedType != "ИБП" && selectedType != "Аккумулятор")
        {
            if (!int.TryParse(powerConsumptionInput.text, out _))
            {
                errorText.text = "Потребляемая мощность должна быть числом";
                return false;
            }
        }

        switch (selectedType)
        {
            case "Видеорегистратор":
                if (!int.TryParse(maxChannelsInput.text, out _))
                {
                    errorText.text = "Максимальное количество каналов должно быть числом";
                    return false;
                }
                break;
            case "ИБП":
                if (!int.TryParse(maxBatteriesInput.text, out _))
                {
                    errorText.text = "Максимальное количество батарей должно быть числом";
                    return false;
                }
                break;
            case "Аккумулятор":
                if (!int.TryParse(powerWattsInput.text, out _))
                {
                    errorText.text = "Мощность должна быть числом";
                    return false;
                }
                break;
        }

        return true;
    }

    private void SaveObject()
    {
        if (!ValidateInputs())
        {
            errorText.gameObject.SetActive(true);
            return;
        }
        else errorText.gameObject.SetActive(false);

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
            case "Видеорегистратор":
                newItem.maxChannels = int.Parse(maxChannelsInput.text);
                newItem.powerConsumption = int.Parse(powerConsumptionInput.text);
                break;
            case "ИБП":
                newItem.maxBatteries = int.Parse(maxBatteriesInput.text);
                break;
            case "Аккумулятор":
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
            case "Камера": return "Sprites/NewItemSprites/Camera";
            case "Коммутатор": return "Sprites/NewItemSprites/Switch";
            case "СКУД контроллер": return "Sprites/NewItemSprites/AccessController";
            case "Видеорегистратор": return "Sprites/NewItemSprites/NVR";
            case "ИБП": return "Sprites/NewItemSprites/UPS";
            default: return "Sprites/NewItemSprites/Battery";
        }
    }

    private string GetDefaultPrefab(string type)
    {
        switch (type)
        {
            case "Камера": return "Mashes/Camera";
            case "Коммутатор": return "Mashes/Switch";
            case "СКУД контроллер": return "Mashes/AccessController";
            case "Видеорегистратор": return "Mashes/NVR";
            case "ИБП": return "Mashes/UPS";
            default: return "Mashes/Battery";
        }
    }

    private List<string> GetDefaultConnectableTypes(string type)
    {
        switch (type)
        {
            case "Камера": return new List<string> { "Switch" };
            case "Коммутатор": return new List<string> { "Camera", "Switch", "NVR" };
            case "СКУД контроллер": return new List<string> { "Turnstile" };
            case "Видеорегистратор": return new List<string> { "Switch" };
            case "ИБП": return new List<string> { "UPS", "Switch" };
            default: return new List<string>();
        }
    }

    private List<string> GetDefaultMountTags(string type)
    {
        switch (type)
        {
            case "Камера": return new List<string> { "Wall", "Ceiling" };
            case "Аккумулятор": return new List<string> { "UPS" };
            default: return new List<string> { "Floor" };
        }
    }

    private void SaveToCustomCatalog(CatalogItemData newItem)
    {
        // Используем persistentDataPath вместо Application.dataPath
        string directoryPath = Path.Combine(Application.persistentDataPath, "CustomCatalogs");
        string path = Path.Combine(directoryPath, "custom_catalog.json");

        // Создаем директорию если ее нет
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        
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
            errorText.gameObject.SetActive(true);
            errorText.text = "Объект с таким именем уже существует";
            return;
        }
        errorText.gameObject.SetActive(false);

        catalog.items.Add(newItem);
        string json = JsonUtility.ToJson(catalog, true);
        File.WriteAllText(path, json);

        // Notify CatalogManager to reload
        FindObjectOfType<CatalogManager>().ReloadCatalog();
    }
}