using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class GeneralInformationController
{
    public string id;          // Уникальный идентификатор
    public string description; // Основное описание
}

[System.Serializable]
public class GeneralInformationControllerList
{
    public List<GeneralInformationController> generalInformationControllers;
}

public class Guide : MonoBehaviour
{
    public Button GeneralInformationButton;
    public Button CameraInfoButton;
    public Button SwitchboardInfoButton;
    public Button TurnstileInfoButton;
    public Button NvrInfoButton;
    public Button IbpInfoButton;
    public GameObject GeneralInformationContent;
    public GameObject CameraInfoContent;
    public GameObject SwitchboardInfoContent;
    public GameObject TurnstileInfoContent;
    public GameObject NvrInfoContent;
    public GameObject IbpInfoContent;
    public GameObject scudGuide;
    public TextMeshProUGUI[] displayTexts = new TextMeshProUGUI[6]; // Массив для отображения текста 
    public string[] targetIds; // Массив для хранения targetId для каждого TextMeshProUGUI
    public TMP_InputField inputField; // Поле для ввода id

    void Start()
    {
        GeneralInformationButton.onClick.AddListener(ShowGeneralInformationContent);
        CameraInfoButton.onClick.AddListener(ShowCameraInfoContent);
        SwitchboardInfoButton.onClick.AddListener(ShowSwitchboardInfoContent);
        TurnstileInfoButton.onClick.AddListener(ShowTurnstileInfoContent);
        NvrInfoButton.onClick.AddListener(ShowNvrInfoContent);
        IbpInfoButton.onClick.AddListener(ShowIbpInfoContent);
    }

    private void ResetButtonColors()
    {
        GeneralInformationButton.interactable = true;
        CameraInfoButton.interactable = true;
        SwitchboardInfoButton.interactable = true;
        TurnstileInfoButton.interactable = true;
        NvrInfoButton.interactable = true;
        IbpInfoButton.interactable = true;
    }

    private void ShowGeneralInformationContent()
    {
        ResetButtonColors();
        GeneralInformationButton.interactable = false;
        HideAllContent();
        GeneralInformationContent.SetActive(true);
        
        // Загружаем и отображаем информацию для каждого targetId
        for (int i = 0; i < targetIds.Length; i++)
        {
            LoadAndDisplayText(targetIds[i], i); // Передаем targetId и индекс
        }
    }
    private void ShowCameraInfoContent()
    {
        ResetButtonColors();
        CameraInfoButton.interactable = false;
        HideAllContent();
        CameraInfoContent.SetActive(true);
        
        // Загружаем и отображаем информацию для каждого targetId
        for (int i = 0; i < targetIds.Length; i++)
        {
            LoadAndDisplayText(targetIds[i], i); // Передаем targetId и индекс
        }
    }
    private void ShowSwitchboardInfoContent()
    {
        ResetButtonColors();
        SwitchboardInfoButton.interactable = false;
        HideAllContent();
        SwitchboardInfoContent.SetActive(true);
        
        // Загружаем и отображаем информацию для каждого targetId
        for (int i = 0; i < targetIds.Length; i++)
        {
            LoadAndDisplayText(targetIds[i], i); // Передаем targetId и индекс
        }
    }
    private void ShowTurnstileInfoContent()
    {
        ResetButtonColors();
        TurnstileInfoButton.interactable = false;
        HideAllContent();
        TurnstileInfoContent.SetActive(true);
        
        // Загружаем и отображаем информацию для каждого targetId
        for (int i = 0; i < targetIds.Length; i++)
        {
            LoadAndDisplayText(targetIds[i], i); // Передаем targetId и индекс
        }
    }
    private void ShowNvrInfoContent()
    {
        ResetButtonColors();
        NvrInfoButton.interactable = false;
        HideAllContent();
        NvrInfoContent.SetActive(true);
        
        // Загружаем и отображаем информацию для каждого targetId
        for (int i = 0; i < targetIds.Length; i++)
        {
            LoadAndDisplayText(targetIds[i], i); // Передаем targetId и индекс
        }
    }
    private void ShowIbpInfoContent()
    {
        ResetButtonColors();
        IbpInfoButton.interactable = false;
        HideAllContent();
        IbpInfoContent.SetActive(true);
        
        // Загружаем и отображаем информацию для каждого targetId
        for (int i = 0; i < targetIds.Length; i++)
        {
            LoadAndDisplayText(targetIds[i], i); // Передаем targetId и индекс
        }
    }

    private void HideAllContent()
    {
        GeneralInformationContent.SetActive(false);
        CameraInfoContent.SetActive(false);
        SwitchboardInfoContent.SetActive(false);
        TurnstileInfoContent.SetActive(false);
        NvrInfoContent.SetActive(false);
        IbpInfoContent.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            HideAllContent();
            ResetButtonColors();
            scudGuide.SetActive(!scudGuide.activeSelf);
        }
    }

    private void LoadAndDisplayText(string id, int index)
    {
        GeneralInformationControllerList generalInformationControllerList = LoadGeneralInformationControllersFromJson();

        if (generalInformationControllerList == null || generalInformationControllerList.generalInformationControllers.Count == 0)
        {
            displayTexts[index].text = "Нет доступной информации."; // Сообщение, если данных нет
            return;
        }

        // Поиск контроллера по id
        GeneralInformationController foundController = generalInformationControllerList.generalInformationControllers
            .FirstOrDefault(controller => controller.id == id);

        if (foundController != null)
        {
            // Заполнение поля displayTexts по индексу
            displayTexts[index].text = foundController.description; // Отображаем описание
        }
        else
        {
            displayTexts[index].text = "Информация не найдена."; // Сообщение, если контроллер не найден
        }
    }

    private GeneralInformationControllerList LoadGeneralInformationControllersFromJson()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("information"); // Имя JSON-файла
        if (jsonFile != null)
        {
            return JsonUtility.FromJson<GeneralInformationControllerList>(jsonFile.text);
        }
        return null;
    }
}
