using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarterAssets;
using SCUD3D;

public class UI : MonoBehaviour
{
    public Canvas myCanvas;
    public Button TurnstileButton;

    public Button AddButton;

    public Button CloseButton;

    public GameObject previewPanel;

    public GameObject infoPanel;

    public TMP_Text infoPanel_text;

    private StarterAssetsInputs inputs;

    private string log;
    private int count = 0;
    public bool isVisible = true;

    void Start()
    {
        GameObject otherObject = GameObject.FindWithTag("Player");
        if (otherObject != null)
        {

            inputs = otherObject.GetComponent<StarterAssetsInputs>();
        }
        TurnstileButton.onClick.AddListener(OnButtonClick);
        AddButton.onClick.AddListener(OnAddButtonClick);
        CloseButton.onClick.AddListener(OnCloseClick);
        previewPanel.SetActive(false);
        infoPanel.SetActive(false);
    }

    void OnButtonClick()
    {
        previewPanel.SetActive(true);
        infoPanel.SetActive(true);

    }

    void OnCloseClick()
    {
        ShowHideMenu();
    }

    void OnAddButtonClick()
    {
        ShowHideMenu();
        ObjectAdder adder = myCanvas.GetComponent<ObjectAdder>();
        adder.object_chosen = true;
    }

    void ShowHideMenu() {
        myCanvas.planeDistance = (myCanvas.planeDistance == 0) ? 1 : 0;
            count = 0;
            if (myCanvas.planeDistance == 1)
            {
                if (inputs != null)
                {
                    inputs.cursorLocked = false;
                    inputs.cursorInputForLook = false;
                    inputs.SetCursorState(inputs.cursorLocked);
                }
            }
            if (myCanvas.planeDistance == 0)
            {
                if (inputs != null)
                {
                    inputs.cursorLocked = true;
                    inputs.cursorInputForLook = true;
                    inputs.SetCursorState(inputs.cursorLocked);
                }
            }
    }

    void Update()
    {
        // Проверяем, нажата ли клавиша "H"
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log(log);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            // Переключаем видимость элемента UI
            ShowHideMenu();
        }

    }
}