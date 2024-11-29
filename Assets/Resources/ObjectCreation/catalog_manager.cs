using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using StarterAssets;
using SCUD3D;
using System;
using System.Resources;

public class CatalogManager : MonoBehaviour
{
    public GameObject buttonAddPrefab;
    public GameObject PanelItems;
    public GameObject PanelPreview;
    public GameObject PanelInfo;
    public GameObject buttonPrefab; // Префаб элемента списка
    public GameObject itemImage;
    public TextMeshProUGUI itemName;
    private CatalogItemData selectedItemData;
    public TextMeshProUGUI itemDescription;
    public Transform contentPanel;    // Панель внутри Scroll View, куда будут добавляться элементы
    public TextAsset jsonFile;        // JSON файл, подключённый через инспектор

    public bool isPreviewVisible = false;
    public bool isItemsVisible = true;

    private void Start()
    {
        PanelPreview.SetActive(isPreviewVisible);
        PanelInfo.SetActive(isPreviewVisible);
        LoadCatalog();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) ShowHideItems();


    }

    // Метод для загрузки и отображения данных
    private void LoadCatalog()
    {
        // Читаем JSON файл
        CatalogItemsList itemList = JsonUtility.FromJson<CatalogItemsList>(jsonFile.text);

        foreach (var itemData in itemList.items)
        {
            // Создаём элемент списка на основе префаба
            GameObject newItemButton = Instantiate(buttonPrefab, contentPanel);


            // Ищем текстовые и графические компоненты
            Button itemButton = newItemButton.GetComponentInChildren<Button>();
            TextMeshProUGUI buttonText = itemButton.GetComponentInChildren<TextMeshProUGUI>();
            // Text descriptionText = newItem.transform.Find("DescriptionText").GetComponent<Text>();
            //добавляем eventlistener
            itemButton.onClick.AddListener(() => ViewItem(itemData));

            // дескрипшен
            buttonText.text = itemData.itemName;
            Debug.Log($"Item Name: {itemData.itemName}, Type: {itemData.type}, Raw Type: {itemData.type.ToString()}");
        }
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
}


[System.Serializable]
public class CatalogItemData
{
    public string itemName;
    public string description;
    public string type;
    public int maxConnections;
    public int powerConsumption;
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
