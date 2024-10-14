using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class dynamic_canvas : MonoBehaviour
{
    public Camera MainCamera;
    public Sprite newSprite;
   void Start()
    {
        // Создаем новый Canvas
        GameObject canvasObject = new GameObject("MyCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = MainCamera;
        canvas.planeDistance = 1;
        

        // Добавляем компонент CanvasScaler
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // Добавляем компонент GraphicRaycaster
        canvasObject.AddComponent<GraphicRaycaster>();

        // Создаем текст
        GameObject Panel = new GameObject("Panel");
        Panel.transform.parent = canvasObject.transform; // Устанавливаем родителем Canvas
        Image PanelImage = Panel.AddComponent<Image>();
        Panel.transform.localScale = new Vector3(0.5f, 0.5f, 0f);
        Panel.transform.localPosition = new Vector3(0f, 0f, 0f);

        PanelImage.sprite = newSprite;


        // Создаем кнопку
        GameObject buttonObject = new GameObject("MyButton");
        buttonObject.transform.parent = canvasObject.transform; // Устанавливаем родителем Canvas


        // Создаем текст для кнопки
        GameObject buttonTextObject = new GameObject("ButtonText");
        buttonTextObject.transform.parent = buttonObject.transform; // Устанавливаем родителем кнопки
        Text buttonText = buttonTextObject.AddComponent<Text>();
        buttonText.text = "Click Me!";

    }
}
