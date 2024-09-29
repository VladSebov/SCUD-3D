using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{    
    public GameObject objectToSpawn; // Объект, который вы хотите добавлять
    public Camera mainCamera; // Ссылка на основную камеру

    void Update()
    {
        // Проверяем, был ли щелчок мыши
        if (Input.GetMouseButtonDown(0)) // 0 - левая кнопка мыши
        {
            SpawnObject();
        }
        if (Input.GetMouseButtonDown(1)) // 0 - левая кнопка мыши
        {
    
    
        // Создаем луч из позиции курсора мыши
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Проверяем, пересекает ли луч какой-либо объект
        if (Physics.Raycast(ray, out hit))
        {
            // Если пересечение найдено, выводим информацию о объекте
            Debug.Log("Пересечен объект: " + hit.collider.name);
            Debug.Log("Точка пересечения: " + hit.point);
            Debug.Log("Нормаль поверхности: " + hit.normal);
            Debug.Log("Расстояние до объекта: " + hit.distance);
        }
        }
    }

    void SpawnObject()
    {
        // Получаем позицию мыши в мировых координатах
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Проверяем, попадает ли луч на плоскость
        if (Physics.Raycast(ray, out hit) && hit.collider.name == "Ground")
        {
            // Создаем объект на позиции щелчка
            Instantiate(objectToSpawn, hit.point, Quaternion.identity);
        }
    }
    }
