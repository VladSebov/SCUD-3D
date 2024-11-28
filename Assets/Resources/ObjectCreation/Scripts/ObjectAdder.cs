using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using StarterAssets;
using UnityEngine.UI;

namespace SCUD3D
{
    public class ObjectAdder : MonoBehaviour
    {
        public Camera playerCam;
        public int gameState = 2; // 0 - objectSelection; 1 - objectCreation; 2 - AnySettings;
                                  // 3 - objectChangingPosition
        public LayerMask layermask;
        public GameObject objectPrefab; // префаб объекта
        public InteractiveObject currentObject; // текущий объект для настройки
        public ObjectSettingsManager ObjectSettingsManager; // скрипт для objectSettings
        public CatalogManager CatalogManager;

        public ScudSettings ScudSettings;
        public MenuDevicesManager MenuDevicesManager; // скрипт для menuDevices

        public CatalogItemData objectData; // данные объекта
        // public ObjectType objectType; // тип объекта
        // public List<ObjectType> connectableTypes; // типы объектов доступных для подключения

        public GameObject Ground;

        public StarterAssetsInputs inputs;

        private GameObject previewObject; // объект для предварительного просмотра
        public bool object_chosen = false;

        //public List<GameObject> CreatedObjects;

        private float t = 0f;

        private float deltat;
        private GameObject previousSelection = null;

        void Start()
        {
            GameObject otherObject = GameObject.FindWithTag("Player");
            if (otherObject != null)
            {
                inputs = otherObject.GetComponent<StarterAssetsInputs>();
            }
            deltat = Time.deltaTime / 2f;
            ScudSettings = GetComponent<ScudSettings>();
            ObjectSettingsManager = GetComponent<ObjectSettingsManager>();
            MenuDevicesManager = GetComponent<MenuDevicesManager>();
            CatalogManager = GetComponent<CatalogManager>();
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

        void SelectObject(RaycastHit hit)
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

        void ChangeSurfaceColor(RaycastHit hit, string tag)
        {
            if (previousSelection == null) previousSelection = hit.collider.gameObject;
            else if (previousSelection != null && hit.collider.gameObject != previousSelection)
            {
                if (hit.collider.gameObject.CompareTag(tag))
                {
                    hit.collider.gameObject.GetComponentInChildren<Renderer>().material.color = Color.green;
                }
                if (!hit.collider.gameObject.CompareTag(tag))
                {
                    hit.collider.gameObject.GetComponentInChildren<Renderer>().material.color = Color.red;
                }
                previousSelection.GetComponentInChildren<Renderer>().material.color = Color.white;
                previousSelection = hit.collider.gameObject;
            }
        }

        void CreatePreviewObject(RaycastHit hit, Vector3 previewPosition)
        {
            switch (objectData.type)
            {
                case "Turnstile":
                    ChangeSurfaceColor(hit, "Floor");
                    if (previewObject == null) CreatePreviewOnFloor(previewPosition, hit);
                    else if (previewObject != null) MovePreviewOnFloor(hit);
                    break;
                case "Camera":
                    ChangeSurfaceColor(hit, "Wall");
                    if (previewObject == null) CreatePreviewOnWall(previewPosition, hit);
                    else if (previewObject != null) MovePreviewOnWall(hit);
                    break;
                default:
                    ChangeSurfaceColor(hit, "Floor");
                    if (previewObject == null) CreatePreviewOnFloor(previewPosition, hit);
                    else if (previewObject != null) MovePreviewOnFloor(hit);
                    break;
            }
        }

        void CreatePreviewOnFloor(Vector3 previewPosition, RaycastHit hit)
        {
            if (hit.collider.gameObject.CompareTag("Floor"))
            {
                //previewPosition.y = hit.point.y;
                previewObject = Instantiate(objectPrefab, previewPosition, Quaternion.identity);
                ChangeLayerRecursively(previewObject.transform, 2);
                // Устанавливаем материал с полупрозрачностью
                UpdateMaterial(previewObject);
                //Material ghostMaterial = Resources.Load<Material>("Materials/Ghost_Material");
            }
        }

        void CreatePreviewOnWall(Vector3 previewPosition, RaycastHit hit)
        {
            if (hit.collider.gameObject.CompareTag("Wall"))
            {
                //previewPosition.z = hit.point.z;
                previewObject = Instantiate(objectPrefab, previewPosition, Quaternion.identity);
                ChangeLayerRecursively(previewObject.transform, 2);
                // Устанавливаем материал с полупрозрачностью
                UpdateMaterial(previewObject);
                //Material ghostMaterial = Resources.Load<Material>("Materials/Ghost_Material");
            }
        }


        void ChangeLayerRecursively(Transform parent, int layer)
        {
            // Устанавливаем слой для текущего объекта
            parent.gameObject.layer = layer;

            // Рекурсивно устанавливаем слой для всех дочерних объектов
            foreach (Transform child in parent)
            {
                ChangeLayerRecursively(child, layer);
            }
        }

        void MovePreviewOnFloor(RaycastHit hit)
        {
            if (hit.collider.gameObject.CompareTag("Floor"))
            {
                Vector3 previewPosition = hit.point;
                previewObject.transform.position = previewPosition;
                //previewObject.transform.eulerAngles = new Vector3(-90f, previewObject.transform.eulerAngles.y, previewObject.transform.eulerAngles.z);
            }
        }

        void MovePreviewOnWall(RaycastHit hit)
        {
            if (hit.collider.gameObject.CompareTag("Wall"))
            {
                Vector3 previewPosition = hit.point;
                previewObject.transform.position = previewPosition;
                //previewObject.transform.eulerAngles = new Vector3(-90f, previewObject.transform.eulerAngles.y, previewObject.transform.eulerAngles.z);
            }
        }

        void CreateObject(Transform transform, Collider collider)
        {
            RoomMetadata roomMetadata = collider.GetComponent<RoomMetadata>();
            if (roomMetadata == null)
            {
                Debug.LogError($"no roomMetadata found for {collider.name}");
                //TODO() remove later, cause RoomMetadata should be added to all enviroment
                roomMetadata = new RoomMetadata();
                roomMetadata.FloorNumber = 1;
                roomMetadata.RoomNumber = 1;//Default values
            }
            objectPrefab = Instantiate(objectPrefab, transform.position, transform.rotation);
            //objectPrefab.transform.eulerAngles = previewObject.transform.eulerAngles;
            //CreatedObjects.Add(objectPrefab);
            ObjectManager.Instance.AddObject(objectData, objectPrefab, roomMetadata); // creates an object 
            Destroy(previewObject); // Удаляем объект предварительного просмотра
            gameState = 0;
        }

        void Update()
        {
            if (previewObject != null)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    // Get the current rotation in Euler angles
                    Vector3 currentRotation = previewObject.transform.rotation.eulerAngles;
                    // Add 90 degrees to the y-axis
                    currentRotation.y += 90f;
                    // Set the new rotation
                    previewObject.transform.rotation = Quaternion.Euler(currentRotation);
                }
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    // Get the current rotation in Euler angles
                    Vector3 currentRotation = previewObject.transform.rotation.eulerAngles;
                    // Subtract 90 degrees from the y-axis
                    currentRotation.y -= 90f;
                    // Set the new rotation
                    previewObject.transform.rotation = Quaternion.Euler(currentRotation);
                }
            }
            RaycastHit hit;

            if (ObjectSettingsManager.objectSettings.activeSelf || ScudSettings.scudSettings.activeSelf || CatalogManager.isItemsVisible)
            {
                inputs.SetInputsState(false);
                gameState = 2;
            }
            else
            {
                if (gameState == 2)
                {
                    gameState = 0;
                    inputs.SetInputsState(true);
                }
            }

            // Detect object click

            if (gameState == 0)
            {
                if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit, 150f, layermask))
                    SelectObject(hit);
                if (Input.GetMouseButtonDown(0))
                {

                    if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit))
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
            }
            if (gameState == 1)
            {
                if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit, 150f, layermask))
                {
                    CreatePreviewObject(hit, hit.point);
                }
                if (previewObject != null && Input.GetMouseButtonDown(0))
                {
                    CreateObject(previewObject.transform, hit.collider);
                    previousSelection.GetComponent<Renderer>().material.color = Color.white;
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    if (previewObject != null) Destroy(previewObject);
                    gameState = 0;
                    previousSelection.GetComponent<Renderer>().material.color = Color.white;
                }
            }
            if (gameState == 3)
            {
                Vector3 previousPosition = objectPrefab.gameObject.transform.position;
                if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit, 150f, layermask))
                {
                    CreatePreviewObject(hit, previousPosition);
                    objectPrefab.gameObject.SetActive(false);
                }
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


