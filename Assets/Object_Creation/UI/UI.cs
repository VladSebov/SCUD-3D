using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StarterAssets;

public class UI : MonoBehaviour
{
    public Canvas myCanvas;
    public Button myButton;

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
        myButton.onClick.AddListener(OnImageClick);

    }

    void OnImageClick()
    {
        // Действие, которое произойдет при нажатии на Image
        Debug.Log("Image был нажат!");
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
        if (Input.GetMouseButtonDown(0))
        {
            // Raycast для определения позиции курсора на земле
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log(hit.collider.name);
                if (hit.collider.name == "table B")
                {
                    log = hit.collider.GetComponent<cam>().model_name;
                    hit.collider.GetComponent<cam>().model_name += "5";
                }


            }
        }
    }
}

