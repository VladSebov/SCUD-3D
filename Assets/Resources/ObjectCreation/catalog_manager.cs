using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using StarterAssets;
using SCUD3D;
using System;
using System.Resources;
using Unity.VisualScripting;

public class CatalogManager : MonoBehaviour
{
    public GameObject buttonAddPrefab;
    public GameObject PanelItems;
    public GameObject PanelPreview;
    public GameObject PanelInfo;
    public GameObject HintsPanel;
    public GameObject defaultItemPrefab; // Префаб элемента списка
    public GameObject customItemPrefab; // Префаб элемента списка
    public GameObject itemImage;
    public TextMeshProUGUI itemName;
    private CatalogItemData selectedItemData;
    public TextMeshProUGUI itemDescription;
    public Transform contentPanel;    // Панель внутри Scroll View, куда будут добавляться элементы
    public TextAsset jsonFile;        // JSON файл, подключённый через инспектор

    public bool isPreviewVisible = false;
    public bool isItemsVisible = true;

    public GameObject customObjectForm;
    public TextAsset customCatalogFile;
    private List<CatalogItemData> allItems = new List<CatalogItemData>();

    private bool isCustomItem = false;

    public GameObject listHeaderPrefab; // Prefab for the expandable header
    private GameObject defaultListHeader;
    private GameObject customListHeader;
    private Transform defaultItemsContainer;
    private Transform customItemsContainer;

    private void Start()
    {
        PanelPreview.SetActive(isPreviewVisible);
        PanelInfo.SetActive(isPreviewVisible);
        LoadCatalog();
    }

    private void Update()
    {
        if (InputHelper.IsTypingInInputField())
            return;
        if (Input.GetKeyDown(KeyCode.I)) ShowHideItems();
        if (Input.GetKeyDown(KeyCode.M)) {
            HintsPanel.SetActive(!HintsPanel.activeSelf);
        }
    }

    // Метод для загрузки и отображения данных
    private void LoadCatalog()
    {
        // Clear existing content
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Make sure content panel has proper layout components
        EnsureContentPanelLayout();

        // Create containers for default and custom items
        defaultItemsContainer = new GameObject("DefaultItemsContainer", typeof(RectTransform), typeof(VerticalLayoutGroup)).transform;
        defaultItemsContainer.SetParent(contentPanel, false);
        defaultListHeader = CreateListHeader("Встроенные объекты");

        // Set up layout group for default items container
        VerticalLayoutGroup defaultLayout = defaultItemsContainer.GetComponent<VerticalLayoutGroup>();
        defaultLayout.childAlignment = TextAnchor.UpperLeft;
        defaultLayout.childControlHeight = false;
        defaultLayout.childControlWidth = false;
        defaultLayout.childForceExpandHeight = false;
        defaultLayout.childForceExpandWidth = false;
        defaultLayout.spacing = 2;
        defaultLayout.padding = new RectOffset(10, 10, 5, 5); // Add some padding

        // // Add LayoutElement to container
        // LayoutElement containerLayout = defaultItemsContainer.gameObject.GetOrAddComponent<LayoutElement>();
        // containerLayout.flexibleWidth = 1;
        // containerLayout.flexibleHeight = 0;

        // Set the RectTransform size
        RectTransform defaultRect = defaultItemsContainer.GetComponent<RectTransform>();
        defaultRect.anchorMin = new Vector2(0, 0);
        defaultRect.anchorMax = new Vector2(1, 0);
        defaultRect.sizeDelta = new Vector2(0, 0);

        // Move container right after its header
        defaultItemsContainer.SetSiblingIndex(defaultListHeader.transform.GetSiblingIndex() + 1);

        // Repeat for custom items
        customItemsContainer = new GameObject("CustomItemsContainer", typeof(RectTransform), typeof(VerticalLayoutGroup)).transform;
        customItemsContainer.SetParent(contentPanel, false);
        customListHeader = CreateListHeader("Пользовательские");

        // Set up layout group for custom items container
        VerticalLayoutGroup customLayout = customItemsContainer.GetComponent<VerticalLayoutGroup>();
        customLayout.childAlignment = TextAnchor.UpperLeft;
        customLayout.childControlHeight = false;
        customLayout.childControlWidth = false;
        customLayout.childForceExpandHeight = false;
        customLayout.childForceExpandWidth = false;
        customLayout.spacing = 2;

        RectTransform customRect = customItemsContainer.GetComponent<RectTransform>();
        customRect.anchorMin = new Vector2(0, 0);
        customRect.anchorMax = new Vector2(1, 0);
        customRect.sizeDelta = new Vector2(0, 0);

        // Move container right after its header
        customItemsContainer.SetSiblingIndex(customListHeader.transform.GetSiblingIndex() + 1);

        // Load default catalog
        CatalogItemsList defaultItemList = JsonUtility.FromJson<CatalogItemsList>(jsonFile.text);
        foreach (var item in defaultItemList.items)
        {
            CreateItemButton(item, false);
        }

        // Load custom catalog if exists
        if (customCatalogFile != null)
        {
            CatalogItemsList customItemList = JsonUtility.FromJson<CatalogItemsList>(customCatalogFile.text);
            foreach (var item in customItemList.items)
            {
                CreateItemButton(item, true);
            }
        }

        // Make sure containers start expanded
        defaultItemsContainer.gameObject.SetActive(true);
        customItemsContainer.gameObject.SetActive(true);
    }

    private void EnsureContentPanelLayout()
    {
        // Add or get VerticalLayoutGroup
        VerticalLayoutGroup contentLayout = contentPanel.GetOrAddComponent<VerticalLayoutGroup>();
        contentLayout.childAlignment = TextAnchor.UpperCenter;
        contentLayout.childControlHeight = true;  // Don't control child heights
        contentLayout.childControlWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childForceExpandWidth = true;
        contentLayout.spacing = 5;

        // Add or get ContentSizeFitter
        ContentSizeFitter sizeFitter = contentPanel.GetOrAddComponent<ContentSizeFitter>();
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Set RectTransform settings
        RectTransform rect = contentPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
    }

    private GameObject CreateListHeader(string headerText)
    {
        GameObject header = Instantiate(listHeaderPrefab, contentPanel);

        // Set proper size for the header
        RectTransform headerRect = header.GetComponent<RectTransform>();
        headerRect.sizeDelta = new Vector2(0, 40); // Set height to 40 (or your desired height)

        // Add LayoutElement to maintain height
        LayoutElement headerLayout = header.GetOrAddComponent<LayoutElement>();
        headerLayout.minHeight = 40;
        headerLayout.preferredHeight = 40;
        headerLayout.flexibleHeight = 0;
        headerLayout.flexibleWidth = 1;

        TextMeshProUGUI text = header.GetComponentsInChildren<TextMeshProUGUI>()[1];
        text.text = headerText;

        // Get the arrow from the header prefab
        RectTransform arrow = header.GetComponentsInChildren<TextMeshProUGUI>()[0].GetComponent<RectTransform>();

        // Get or add toggle button
        Button toggleButton = header.GetOrAddComponent<Button>();

        // Setup toggle functionality
        Transform container = headerText == "Встроенные объекты" ? defaultItemsContainer : customItemsContainer;
        toggleButton.onClick.AddListener(() => ToggleList(container, arrow));

        return header;
    }

    private void ToggleList(Transform container, RectTransform arrow)
    {
        bool isExpanded = container.gameObject.activeSelf;
        container.gameObject.SetActive(!isExpanded);

        // Rotate this header's arrow
        if (arrow != null)
        {
            float targetRotation = isExpanded ? 0 : -90;
            arrow.rotation = Quaternion.Euler(0, 0, targetRotation);
        }
    }

    // New method to create item buttons
    private void CreateItemButton(CatalogItemData itemData, bool isCustom)
    {
        Transform parent = isCustom ? customItemsContainer : defaultItemsContainer;
        GameObject newItemButton = Instantiate(isCustom ? customItemPrefab : defaultItemPrefab, parent);

        // // Ensure proper layout element settings on the button
        // LayoutElement buttonLayout = newItemButton.GetComponent<LayoutElement>();
        // if (buttonLayout == null)
        //     buttonLayout = newItemButton.AddComponent<LayoutElement>();

        // buttonLayout.minHeight = 30; // Adjust this value to match your button height
        // buttonLayout.flexibleWidth = 1;

        Button itemButton = newItemButton.GetComponentInChildren<Button>();
        TextMeshProUGUI buttonText = itemButton.GetComponentInChildren<TextMeshProUGUI>();
        itemButton.onClick.AddListener(() =>
        {
            isCustomItem = isCustom;
            ViewItem(itemData);
        });
        if (isCustom)
        {
            Button deleteButton = newItemButton.GetComponentsInChildren<Button>()[1];
            deleteButton.onClick.AddListener(() => DeleteCustomItem(itemData));
        }
        buttonText.text = itemData.itemName;
    }

    public void CloseCustomObjectForm()
    {
        customObjectForm.SetActive(false);
    }

    public void ViewItem(CatalogItemData itemData)
    {
        if (!isPreviewVisible)
        {
            ShowHidePreview();
        }
        selectedItemData = itemData;
        itemName.text = itemData.itemName;
        itemDescription.text = itemData.description;

        //подгружаем спрайт:
        Sprite loadedSprite = Resources.Load<Sprite>(itemData.icon);
        Image ImagePic = itemImage.GetComponent<Image>();
        if (loadedSprite != null)
        {
            if (ImagePic != null) { ImagePic.sprite = loadedSprite; }
        }
        else { ImagePic.sprite = Resources.Load<Sprite>("no_preview_dark"); }
    }

    public void AddItemToScene()
    {
        // parse device type
        Enum.TryParse(selectedItemData.type, true, out ObjectType type);
        if (!CheckTypeRestriction(type))
        {
            Debug.Log($"Достигнуто максимальное количество объектов типа {type}");
            return;
        }
        if (!CheckPriceRestriction(1000)) // TODO() replace with actual price
        {
            Debug.Log($"Не хватает средств на установку объекта");
            return;
        }
        ShowHideItems();
        ObjectAdder adder = this.GetComponent<ObjectAdder>();
        adder.objectPrefab = Resources.Load<GameObject>(selectedItemData.prefab).GetComponent<BoxCollider>().gameObject;
        adder.objectData = selectedItemData;
        if (adder.objectPrefab != null)
        {
            adder.gameState = 1;
            adder.inputs.SetInputsState(true);
        }

        else Debug.Log("Префаб не найден");
    }

    private bool CheckPriceRestriction(int itemPrice)
    {
        return RestrictionsManager.Instance.CheckItemAffordable(itemPrice);
    }

    private bool CheckTypeRestriction(ObjectType type)
    {
        switch (type)
        {
            case ObjectType.Camera:
                return RestrictionsManager.Instance.CheckCameraAvailable();
            default:
                return true;
        }
    }

    void ShowHideItems()
    {
        isItemsVisible = !isItemsVisible;
        PanelItems.SetActive(isItemsVisible);
        if (!PanelItems.activeSelf)
        {
            customObjectForm.SetActive(false);
        }
        if (!isItemsVisible)
        {
            if (isPreviewVisible) ShowHidePreview();
        }

    }

    void ShowHidePreview()
    {
        isPreviewVisible = !isPreviewVisible;
        PanelPreview.SetActive(isPreviewVisible);
        PanelInfo.SetActive(isPreviewVisible);
    }

    public void ShowCustomObjectForm()
    {
        customObjectForm.SetActive(true);
    }

    public void ReloadCatalog()
    {
        // Read custom catalog directly from file
        string customCatalogPath = Path.Combine(Application.dataPath, "Resources/ObjectCreation/custom_catalog.json");
        if (File.Exists(customCatalogPath))
        {
            string jsonContent = File.ReadAllText(customCatalogPath);
            JsonUtility.FromJson<CatalogItemsList>(jsonContent);
            customCatalogFile = new TextAsset(jsonContent);
        }
        else
        {
            customCatalogFile = null;
        }

        LoadCatalog();
        customObjectForm.SetActive(false);
    }

    // Add new method to delete custom items
    public void DeleteCustomItem(CatalogItemData itemData)
    {
        string path = Path.Combine(Application.dataPath, "Resources/ObjectCreation/custom_catalog.json");
        if (File.Exists(path))
        {
            string jsonContent = File.ReadAllText(path);
            CatalogItemsList catalog = JsonUtility.FromJson<CatalogItemsList>(jsonContent);

            // Remove the item with matching name
            catalog.items.RemoveAll(item =>
                item.itemName.Equals(itemData.itemName, StringComparison.OrdinalIgnoreCase));

            // Save the updated catalog
            string json = JsonUtility.ToJson(catalog, true);
            File.WriteAllText(path, json);

            // Hide preview panels
            if (isPreviewVisible)
            {
                ShowHidePreview();
            }

            // Reload the catalog to update the UI
            ReloadCatalog();
        }
    }
}


[System.Serializable]
public class CatalogItemData
{
    public string itemName;
    public string description;
    public string type;
    public int maxConnections;
    public int powerConsumption;
    public float price;
    public List<string> connectableTypes;
    public List<string> mountTags;
    public string icon;  // Название иконки
    public string prefab;

    //NVR specific fields
    public int maxChannels;
    //UPS specific fields
    public int maxBatteries;
    //Battery specific fields
    public int powerWatts;

}

[System.Serializable]
public class CatalogItemsList
{
    public List<CatalogItemData> items;
}
