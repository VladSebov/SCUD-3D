using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SCUD3D
{
    public class ObjectAdder : MonoBehaviour
    {
        public int gameState = 0; // 0 - objectSelection; 1 - objectCreation; 2 - objectSettings;
                                  // 3 - objectChangingPosition
        public LayerMask layermask;
        public GameObject objectPrefab; // префаб объекта
        public InteractiveObject currentObject; // текущий объект для настройки
        public ObjectSettingsManager ObjectSettingsManager; // скрипт для objectSettings
        public MenuDevicesManager MenuDevicesManager; // скрипт для objectSettings

        public CatalogItemData objectData; // данные объекта
        // public ObjectType objectType; // тип объекта
        // public List<ObjectType> connectableTypes; // типы объектов доступных для подключения

        public GameObject Ground;
        private GameObject previewObject; // объект для предварительного просмотра
        public bool object_chosen = false;

        //public List<GameObject> CreatedObjects;

        private float t = 0f;

        private float deltat;
        private GameObject previousSelection = null;

        void Start()
        {
            deltat = Time.deltaTime / 2f;
            ObjectSettingsManager = GetComponent<ObjectSettingsManager>();
            MenuDevicesManager = GetComponent<MenuDevicesManager>();
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
            material.color = Color.Lerp(Color.HSVToRGB(200 / 360f, 0.7f, 1f), Color.HSVToRGB(240 / 360f, 0.5f, 1f), t);
            if (t > 1 || t <= 0) deltat *= -1f; // Сброс значения для повторения
        }

        void SelectObject(Ray ray, RaycastHit hit)
        {
            var gameObjects = ObjectManager.Instance.GetAllObjects().Select(io => io.gameObject).ToList();
            if (gameObjects.Contains(hit.collider.gameObject))
            {
                ColorAnimation(hit.collider.gameObject);
                // if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
                // {
                //     objectSettings.SetActive(true);
                // }
                if (Input.GetKeyDown(KeyCode.T))
                {
                    objectPrefab = hit.collider.gameObject;
                    gameState = 3;
                }
                if (Input.GetKeyDown(KeyCode.X))
                {
                    ObjectManager.Instance.RemoveObject(hit.collider.gameObject);
                    //Destroy(hit.collider.gameObject);
                }
            }
            if (previousSelection == null) previousSelection = hit.collider.gameObject;
            else if (previousSelection != null && hit.collider.gameObject != previousSelection)
            {
                previousSelection.GetComponentInChildren<Renderer>().material.color = Color.white;
                previousSelection = hit.collider.gameObject;
            }
        }

        void CreatePreviewObject(Ray ray, RaycastHit hit, Vector3 previewPosition)
        {
            {
                previewPosition.y = Ground.transform.position.y;
                previewObject = Instantiate(objectPrefab, previewPosition, Quaternion.identity);
                previewObject.layer = 2;
                // Устанавливаем материал с полупрозрачностью
                UpdateMaterial(previewObject);
                //Material ghostMaterial = Resources.Load<Material>("Materials/Ghost_Material");
            }
        }

        void MovePreviewObject(Ray ray, RaycastHit hit)
        {
            if (hit.collider.name != previewObject.name)
            {
                Vector3 previewPosition = hit.point;
                previewPosition.y = Ground.transform.position.y;
                previewObject.transform.position = previewPosition;
                //previewObject.transform.eulerAngles = new Vector3(-90f, previewObject.transform.eulerAngles.y, previewObject.transform.eulerAngles.z);
            }
        }

        void CreateObject(Vector3 position)
        {
            objectPrefab = Instantiate(objectPrefab, position, Quaternion.identity);
            //objectPrefab.transform.eulerAngles = previewObject.transform.eulerAngles;
            //CreatedObjects.Add(objectPrefab);
            ObjectManager.Instance.AddObject(objectData, objectPrefab); // creates an object 
            Destroy(previewObject); // Удаляем объект предварительного просмотра
            gameState = 0;
        }

        void Update()
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Detect object click
            if (Input.GetMouseButtonDown(0))
            {

                if (Physics.Raycast(ray, out hit))
                {
                    GameObject clickedObject = hit.collider.gameObject;

                    // Check if the clicked object is interactive
                    InteractiveObject interactiveObject = ObjectManager.Instance.GetObject(clickedObject.name);
                    if (interactiveObject != null)
                    {
                        // Show configuration menu
                        currentObject = interactiveObject;
                        ShowObjectMenu(currentObject);
                    }
                }
            }

            if (gameState == 0)
            {
                if (Physics.Raycast(ray, out hit, 150f, layermask))
                    SelectObject(ray, hit);
            }
            if (gameState == 1)
            {
                if (Physics.Raycast(ray, out hit, 150f, layermask) && previewObject == null)
                {
                    CreatePreviewObject(ray, hit, hit.point);
                }
                else if (Physics.Raycast(ray, out hit, 150f, layermask) && previewObject != null)
                {
                    MovePreviewObject(ray, hit);
                }
                if (Input.GetMouseButtonDown(0))
                {
                    CreateObject(previewObject.transform.position);
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    Destroy(previewObject);
                    gameState = 0;
                }
            }
            if (gameState == 2)
            {

            }
            if (gameState == 3)
            {
                Vector3 previousPosition = objectPrefab.gameObject.transform.position;
                if (Physics.Raycast(ray, out hit, 150f, layermask) && previewObject == null)
                {
                    CreatePreviewObject(ray, hit, previousPosition);
                    objectPrefab.gameObject.SetActive(false);
                }
                else if (Physics.Raycast(ray, out hit, 150f, layermask) && previewObject != null) MovePreviewObject(ray, hit);
                if (Input.GetMouseButtonDown(0))
                {
                    Destroy(previewObject);
                    objectPrefab.gameObject.SetActive(true);
                    objectPrefab.gameObject.transform.position = previewObject.transform.position;

                    gameState = 0;
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    Destroy(previewObject);
                    objectPrefab.gameObject.SetActive(true);
                    gameState = 0;
                }

            }

        }

        public void ShowObjectMenu(InteractiveObject obj)
        {
            // Enable the configuration menu
            ObjectSettingsManager.ShowMenu(obj);
        }


        public void ShowAvailableDevices() // You can modify this to get input from the user
        {
            MenuDevicesManager.ShowMenu(currentObject);
        }
    }
}


