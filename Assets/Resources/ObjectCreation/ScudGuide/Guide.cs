using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SCUD3D;
using Unity.VisualScripting;

[System.Serializable]
public class GuideCategory
{
    public string name;
    public string header;
    public string content;
}

[System.Serializable]
public class GuideData
{
    public List<GuideCategory> categories;
}

public class Guide : MonoBehaviour
{
    public GameObject guideMenu;
    public GameObject categoryButtonPrefab; // Prefab for category buttons
    public Transform sidebar; // Parent transform for category buttons
    public TextMeshProUGUI headerText; // Text element for header
    public TextMeshProUGUI contentText; // Text element for content
    public RectTransform contentRect;  // RectTransform of the scroll view content
    public TextAsset jsonGuide;        // JSON файл, подключённый через инспектор
    private GuideData guideData;
    private Button currentActiveButton; // Add this field

    void Start()
    {
        LoadGuideData();
        PopulateSidebar();
        // Display first category and highlight its button
        if (guideData.categories.Any())
        {
            DisplayContent(guideData.categories.First());
            if (sidebar.childCount > 0)
            {
                SetActiveButton(sidebar.GetChild(0).GetComponent<Button>());
            }
        }
    }

    void LoadGuideData()
    {
        guideData = JsonUtility.FromJson<GuideData>(jsonGuide.text);
    }

    void PopulateSidebar()
    {
        foreach (var category in guideData.categories)
        {
            GameObject button = Instantiate(categoryButtonPrefab, sidebar);
            button.GetComponentInChildren<TextMeshProUGUI>().text = category.name;
            Button btnComponent = button.GetComponent<Button>();
            btnComponent.onClick.AddListener(() =>
            {
                DisplayContent(category);
                SetActiveButton(btnComponent);
            });
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            guideMenu.SetActive(!guideMenu.activeSelf);
        }
    }

    void DisplayContent(GuideCategory category)
    {
        headerText.text = category.header;
        contentText.text = category.content;

        // Adjust the size of the content RectTransform based on the text size
        float contentHeight = contentText.preferredHeight;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentHeight);
    }

    void SetActiveButton(Button button)
    {
        if (currentActiveButton != null)
        {
            currentActiveButton.interactable = true;
        }
        currentActiveButton = button;
        currentActiveButton.interactable = false;
    }
}
