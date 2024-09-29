using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Hide : MonoBehaviour
{
    public Canvas myCanvas; // Элемент UI, который нужно скрыть
    public Button myButton;
    
    void Start(){
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
        if (Input.GetKeyDown(KeyCode.H))
        {
            // Переключаем видимость элемента UI
            
            myCanvas.planeDistance = (myCanvas.planeDistance == 0) ? 1 : 0;
        }
    }
}
