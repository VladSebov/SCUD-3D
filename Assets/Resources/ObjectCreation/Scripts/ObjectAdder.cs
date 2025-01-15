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
        public UPSSettingsManager UPSSettingsManager; // скрипт для objectSettings
        public CatalogManager CatalogManager;

        public ScudSettings ScudSettings;
        public Guide Guide;

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
            UPSSettingsManager = GetComponent<UPSSettingsManager>();
            CatalogManager = GetComponent<CatalogManager>();
            Guide = GetComponent<Guide>();
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
                // if (Input.GetKeyDown(KeyCode.T))
                // {
                //     objectPrefab = hit.collider.gameObject;
                //     gameState = 3;
                // }
                if (Input.GetKeyDown(KeyCode.X))
                {
                    ObjectManager.Instance.RemoveObject(hit.collider.gameObject.name);
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

        void CreatePreviewObject(RaycastHit hit, Vector3 previewPosition)
        {
            // Check if the hit collider matches any of the mountTags of the object
            MountTag mountTag = GetMountTagFromCollider(hit.collider);
            ChangeSurfaceColor(hit, mountTag);
            if (objectData.mountTags.Contains(mountTag.ToString()))
            {
                if (previewObject == null)
                    CreatePreview(previewPosition, hit, mountTag);
                else
                    MovePreview(hit, mountTag);
            }
        }

        MountTag GetMountTagFromCollider(Collider collider)
        {
            if (collider.CompareTag("Wall")) return MountTag.Wall;
            if (collider.CompareTag("Floor")) return MountTag.Floor;
            if (collider.CompareTag("Ceiling")) return MountTag.Ceiling;
            if (collider.CompareTag("UPS")) return MountTag.UPS;
            return MountTag.Undefined; // Default fallback
        }

        void ChangeSurfaceColor(RaycastHit hit, MountTag tag)
        {
            if (previousSelection == null) previousSelection = hit.collider.gameObject;
            else if (previousSelection != null && hit.collider.gameObject != previousSelection)
            {
                Renderer renderer = hit.collider.gameObject.GetComponentInChildren<Renderer>();
                //Set the color based on tag validity
                if (objectData.mountTags.Contains(tag.ToString()))
                {
                    renderer.material.color = Color.green;
                }
                else
                {
                    renderer.material.color = Color.red;
                }
                previousSelection.GetComponentInChildren<Renderer>().material.color = Color.white;
                previousSelection = hit.collider.gameObject;
            }
        }

        void CreatePreview(Vector3 previewPosition, RaycastHit hit, MountTag tag)
        {
            previewObject = Instantiate(objectPrefab, previewPosition, Quaternion.identity);
            ChangeLayerRecursively(previewObject.transform, 2);
            UpdateMaterial(previewObject);
        }

        void MovePreview(RaycastHit hit, MountTag tag)
        {
            Vector3 previewPosition = hit.point;
            previewObject.transform.position = previewPosition;
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

        void CreateObject(Transform transform, Collider collider)
        {
            // check if UPS has space when adding battery
            if (objectData.type == ObjectType.Battery.ToString())
            {
                UPS parentUPS = collider.GetComponent<UPS>();
                if (!parentUPS.HasAvailablePlaceForBattery())
                {
                    Debug.Log("У ИБП нет свободных мест под АКБ");
                    return;
                }
            }
            objectPrefab = Instantiate(objectPrefab, transform.position, transform.rotation);
            ObjectManager.Instance.AddObject(objectData, objectPrefab, collider); // creates an object 
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

            if (ObjectSettingsManager.objectSettings.activeSelf || UPSSettingsManager.UPSSettings.activeSelf || ScudSettings.scudSettings.activeSelf || CatalogManager.isItemsVisible || Guide.scudGuide.activeSelf)
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
                    //objectPrefab.gameObject.SetActive(false);
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
            ObjectSettingsManager.ShowMenu(obj);
        }
    }
}


