using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

namespace SCUD3D
{
public class ObjectAdder : MonoBehaviour
{

    public LayerMask layermask;
    public GameObject objectPrefab; // Префаб объекта
    public GameObject Ground;
    private GameObject previewObject; // Объект для предварительного просмотра
    public bool object_chosen = false;


    void Update()
    {
        // Проверяем нажатие ЛКМ

        if (object_chosen)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (previewObject != null)
                {
                    // Добавляем объект на сцену
                    //turnstile turnstile_object = objectPrefab.GetComponent<turnstile>();
                    //turnstile_object.id = 1;
                    Instantiate(objectPrefab, previewObject.transform.position, Quaternion.identity);
                    Destroy(previewObject); // Удаляем объект предварительного просмотра
                    object_chosen = false;
                }
            }
            // Raycast для определения позиции курсора на земле
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;



            if (Physics.Raycast(ray, out hit, 150f, layermask))
            {
                
                if (previewObject == null)
                {
                    // Создаем объект предварительного просмотра
                    Vector3 previewPosition = hit.point;
                    previewPosition.y = Ground.transform.position.y;
                    Debug.Log(previewPosition);
                    previewObject = Instantiate(objectPrefab, hit.point, Quaternion.identity);
                    // Устанавливаем материал с полупрозрачностью
                    Color color = previewObject.GetComponent<Renderer>().material.color;
                    color.a = 0.50f; // Устанавливаем уровень прозрачности
                    previewObject.GetComponent<Renderer>().material.color = color;
                }
                else if (hit.collider.name != previewObject.name)
                {
                    // Обновляем позицию объекта предварительного просмотра
                    Vector3 previewPosition = hit.point;
                    previewPosition.y = Ground.transform.position.y;
                    previewObject.transform.position = previewPosition;
                }


            }
        }


    }
}
}
