using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CatalogManager : MonoBehaviour
{
    public GameObject buttonPrefab; // Префаб элемента списка
    public GameObject itemImage;
    public TextMeshProUGUI itemText;
    public Transform contentPanel;    // Панель внутри Scroll View, куда будут добавляться элементы
    public TextAsset jsonFile;        // JSON файл, подключённый через инспектор

    private void Start()
    {
        LoadCatalog();
    }

    // Метод для загрузки и отображения данных
    private void LoadCatalog()
    {
        // Читаем JSON файл
        CatalogItemList itemList = JsonUtility.FromJson<CatalogItemList>(jsonFile.text);

        foreach (var itemData in itemList.items)
        {
            // Создаём элемент списка на основе префаба
            GameObject newItem = Instantiate(buttonPrefab, contentPanel);

            // Ищем текстовые и графические компоненты
            Button itemButton = newItem.GetComponentInChildren<Button>();
            TextMeshProUGUI buttonText = itemButton.GetComponentInChildren<TextMeshProUGUI>();
            // Text descriptionText = newItem.transform.Find("DescriptionText").GetComponent<Text>();

            // Устанавливаем данные для элемента
            buttonText.text = itemData.itemName;
            itemText.text = itemData.description;
            // descriptionText.text = itemData.description;
            Sprite loadedSprite = Resources.Load<Sprite>(itemData.icon);
            if (loadedSprite != null)
            {
                Image ImagePic = itemImage.GetComponent<Image>();
                if (ImagePic != null)
                {
                    ImagePic.sprite = loadedSprite;
                }
            }
            else
            {
                Debug.LogError($"Failed to load sprite: {itemData.icon}");
            }
        }
    }
}


[System.Serializable]
public class CatalogItemData
{
    public string itemName;
    public string description;
    public string icon;  // Название иконки
}

[System.Serializable]
public class CatalogItemList
{
    public List<CatalogItemData> items;
}