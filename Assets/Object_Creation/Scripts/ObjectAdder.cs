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

                        Material ghostMaterial = Resources.Load<Material>("Materials/Ghost_Material");

                        Material material = previewObject.GetComponentInChildren<Renderer>().material;

                        material.SetFloat("_Mode", 3); // 3 for Transparent
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.EnableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_TRANSPARENT_ON");
                        material.renderQueue = 2900; // Set to transparent render queue
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
