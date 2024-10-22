using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UI;
using StarterAssets;
using SCUD3D;

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

    private bool isPreviewVisible = false;
    private bool isItemsVisible = true;
    private StarterAssetsInputs inputs;

    private void Start()
    {
        GameObject otherObject = GameObject.FindWithTag("Player");
        if (otherObject != null)
        {
            inputs = otherObject.GetComponent<StarterAssetsInputs>();
        }
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
        CatalogItemList itemList = JsonUtility.FromJson<CatalogItemList>(jsonFile.text);

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
        ShowHidePreview();
        ObjectAdder adder = this.GetComponent<ObjectAdder>();
        adder.objectPrefab = Resources.Load<GameObject>(selectedItemData.prefab).GetComponent<BoxCollider>().gameObject;
        if (adder.objectPrefab != null) adder.gameState = 1;
        else Debug.Log("Префаб не найден");
    }

    void ShowHideItems()
    {
        isItemsVisible = !isItemsVisible;
        PanelItems.SetActive(isItemsVisible);
        if (isItemsVisible)
        {
            if (inputs != null)
            {
                inputs.cursorLocked = false;
                inputs.cursorInputForLook = false;
                inputs.SetCursorState(inputs.cursorLocked);
            }
        }
        if (!isItemsVisible)
        {
            if (isPreviewVisible) ShowHidePreview();
            if (inputs != null)
            {
                inputs.cursorLocked = true;
                inputs.cursorInputForLook = true;
                inputs.SetCursorState(inputs.cursorLocked);
            }
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
    public string icon;  // Название иконки
    public string prefab;
}

[System.Serializable]
public class CatalogItemList
{
    public List<CatalogItemData> items;
}
