using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    public LayerMask layermask;
    public GameObject objectPrefab; // Префаб объекта
    public GameObject Ground;
    private GameObject previewObject; // Объект для предварительного просмотра
    private bool object_chosen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            // Переключаем видимость курсора
            if (Cursor.visible)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked; // Закрепляем курсор
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None; // Освобождаем курсор
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            object_chosen = !object_chosen;
        }
        // Проверяем нажатие ЛКМ
        if (Input.GetMouseButtonDown(0))
        {
            if (previewObject != null)
            {
                // Добавляем объект на сцену
                Instantiate(objectPrefab, previewObject.transform.position, Quaternion.identity);
                Destroy(previewObject); // Удаляем объект предварительного просмотра
            }
        }

        // Raycast для определения позиции курсора на земле
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (object_chosen && Physics.Raycast(ray, out hit, 150f, layermask))
        {

            if (previewObject == null)
            {
                // Создаем объект предварительного просмотра
                Vector3 previewPosition = hit.point;
                previewPosition.y = Ground.transform.position.y + 0.5f;
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
                previewPosition.y = Ground.transform.position.y + 0.5f;
                previewObject.transform.position = previewPosition;
            }


        }
    }
}
