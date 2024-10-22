using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using UnityEngine;

namespace SCUD3D
{
    public class ObjectAdder : MonoBehaviour
    {

        public LayerMask layermask;
        public GameObject objectPrefab; // Префаб объекта

        public GameObject testObject;
        public GameObject Ground;
        private GameObject previewObject; // Объект для предварительного просмотра
        public bool object_chosen = false;

        public GameObject[] gameObjects;

        private float t = 0f;

        private float deltat;
        private GameObject previousSelection = null;

        void Start()
        {
            deltat = Time.deltaTime / 2f;
        }

        void UpdateMaterial(GameObject previewObject)
        {
            Material material = previewObject.GetComponentInChildren<Renderer>().material;
            material.renderQueue = 3000;
            // Устанавливаем режим прозрачности
            material.SetFloat("_Surface", 1); // 0 = Opaque, 1 = Transparent
            material.SetFloat("_Blend", 1); // 0 = Alpha, 1 = Premultiply

            // Устанавливаем цвет с альфа-каналом
            Color color = material.GetColor("_BaseColor");
            color.a = 0.7f; // Устанавливаем альфа-канал на 0.5
            material.SetColor("_BaseColor", color);

        }

        void ColorAnimation(GameObject chosenObject)
        {
            Material material = chosenObject.GetComponentInChildren<Renderer>().material;

            t += deltat;
            material.color = Color.Lerp(Color.HSVToRGB(180 / 360f, 0.35f, 1f), Color.HSVToRGB(230 / 360f, 0.45f, 1f), t);
            if (t > 1 || t <= 0) deltat *= -1f; // Сброс значения для повторения
        }

        void SelectObject(Ray ray, RaycastHit hit)
        {
            if (gameObjects.Contains(hit.collider.gameObject))
            {
                ColorAnimation(hit.collider.gameObject);
            }
            if (previousSelection == null) previousSelection = hit.collider.gameObject;
                else if (previousSelection != null && hit.collider.gameObject != previousSelection)
                {
                    previousSelection.GetComponentInChildren<Renderer>().material.color = Color.white;
                    previousSelection = hit.collider.gameObject;
            }
        }

        void Update()
        {
            // Проверяем нажатие ЛКМ

            // UpdateMaterial(testObject);


            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (!object_chosen)
            {
                if (Physics.Raycast(ray, out hit, 150f, layermask))
                    SelectObject(ray, hit);
            }

            if (object_chosen)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (previewObject != null)
                    {
                        // Добавляем объект на сцену
                        //turnstile turnstile_object = objectPrefab.GetComponent<turnstile>();
                        //turnstile_object.id = 1;
                        gameObjects.Append(objectPrefab);
                        Instantiate(objectPrefab, previewObject.transform.position, Quaternion.identity);
                        Destroy(previewObject); // Удаляем объект предварительного просмотра
                        object_chosen = false;
                    }
                }
                // Raycast для определения позиции курсора на земле

                if (Physics.Raycast(ray, out hit, 150f, layermask))
                {

                    if (previewObject == null)
                    {
                        // Создаем объект предварительного просмотра
                        Vector3 previewPosition = hit.point;
                        previewPosition.y = Ground.transform.position.y;
                        previewObject = Instantiate(objectPrefab, hit.point, Quaternion.identity);
                        // Устанавливаем материал с полупрозрачностью
                        
                        UpdateMaterial(previewObject);
                        //Material ghostMaterial = Resources.Load<Material>("Materials/Ghost_Material");


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
