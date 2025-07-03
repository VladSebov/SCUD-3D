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
    	public GameObject EnterPanel;
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
        private Color originalColor;
        private Color originalSurfaceColor;

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

            var hitObject = hit.collider.gameObject;

            GameObject parentObject = hitObject.transform.parent != null
                ? hitObject.transform.parent.gameObject
                : hitObject;

            if (gameObjects.Contains(parentObject))
            {
                if (Input.GetKeyDown(KeyCode.X))
                {
                    if (InputHelper.IsTypingInInputField())
                        return;
                    ObjectManager.Instance.RemoveObject(parentObject.name);
                }
            }
            if (previousSelection == null) {
                previousSelection = hit.collider.gameObject;
                originalColor = previousSelection.GetComponentInChildren<Renderer>().material.color;
            }

            if (gameObjects.Contains(hit.collider.gameObject) && hit.collider.gameObject == previousSelection)
            {
                Material material = hit.collider.gameObject.GetComponentInChildren<Renderer>().material;
                material.color = Color.HSVToRGB(200 / 360f, 0.7f, 1f);
                if (Input.GetKeyDown(KeyCode.X))
                {
                    if (InputHelper.IsTypingInInputField())
                        return;
                    ObjectManager.Instance.RemoveObject(hit.collider.gameObject.name);
                }
            }

            else if (previousSelection != null && hit.collider.gameObject != previousSelection)
            {
                previousSelection.GetComponentInChildren<Renderer>().material.color = originalColor;
                previousSelection = hit.collider.gameObject;
                originalColor = previousSelection.GetComponentInChildren<Renderer>().material.color;
            }
        }

        void CreatePreviewObject(RaycastHit hit, Vector3 previewPosition)
        {
            // Check if the hit collider matches any of the mountTags of the object
            MountTag mountTag = GetMountTagFromCollider(hit.collider);
            //ChangeSurfaceColor(hit, mountTag);
            if (objectData.mountTags.Contains(mountTag.ToString()))
            {
                if (previewObject == null)
                    CreatePreview(previewPosition, hit, mountTag);
                else
                    MovePreview(hit, mountTag);
                // Handle server rack mounting for both switches and NVRs
                if (mountTag == MountTag.ServerRack &&
                    (objectData.type == ObjectType.Switch.ToString() || objectData.type == ObjectType.NVR.ToString()))
                {
                    ServerRack serverRack = hit.collider.GetComponent<ServerRack>();
                    if (serverRack != null)
                    {
                        Transform closestMount = serverRack.GetClosestMountPoint(hit.point);
                        if (closestMount != null)
                        {
                            previewObject.transform.position = closestMount.position;
                            previewObject.transform.rotation = closestMount.rotation;

                            // Optional: Add visual feedback
                            if (previewObject.GetComponent<Renderer>() != null)
                            {
                                previewObject.GetComponent<Renderer>().material.color = Color.green;
                            }
                        }
                    }
                }
            }
        }

        MountTag GetMountTagFromCollider(Collider collider)
        {
            if (collider.CompareTag("Wall")) return MountTag.Wall;
            if (collider.CompareTag("Floor")) return MountTag.Floor;
            if (collider.CompareTag("Ceiling")) return MountTag.Ceiling;
            if (collider.CompareTag("UPS")) return MountTag.UPS;
            if (collider.CompareTag("ServerBox")) return MountTag.ServerBox;
            if (collider.CompareTag("ServerRack")) return MountTag.ServerRack;
            if (collider.CompareTag("ForLock")) return MountTag.ForLock;
            return MountTag.Undefined; // Default fallback
        }

        void ChangeSurfaceColor(RaycastHit hit, MountTag tag)
        {
            if (previousSelection == null) {
                previousSelection = hit.collider.gameObject;
                originalColor = previousSelection.GetComponentInChildren<Renderer>().material.color;
            }
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
                previousSelection.GetComponentInChildren<Renderer>().material.color = originalColor;
                previousSelection = hit.collider.gameObject;
                originalColor = previousSelection.GetComponentInChildren<Renderer>().material.color;
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
                    MessageManager.Instance.ShowMessage("ИБП не имеет свободных мест под АКБ");
                    Destroy(previewObject); // Удаляем объект предварительного просмотра
                    gameState = 0;
                    return;
                }
            }
            if (objectData.type == ObjectType.DoorLock.ToString())
            {
                DoorLockController ParentDoorWall = collider.GetComponentInParent<DoorLockController>();
                if (ParentDoorWall.GetDoorLock() != null)
                {
                    MessageManager.Instance.ShowMessage("У этой двери уже есть электронный замок");
                    Destroy(previewObject); // Удаляем объект предварительного просмотра
                    gameState = 0;
                    return;
                }
            }
            // Check for ServerRack placement
            ServerRack serverRack = collider.GetComponent<ServerRack>();
            if (serverRack != null)
            {
                if (objectData.type == ObjectType.Switch.ToString() || objectData.type == ObjectType.NVR.ToString())
                {
                    if (!serverRack.HasAvailablePlace())
                    {
                        Debug.Log("У серверной стойки нет свободного места");
                        return;
                    }

                    Transform mountPoint = serverRack.GetClosestMountPoint(transform.position);
                    if (mountPoint != null)
                    {
                        CreateMountedDevice(mountPoint, serverRack, null);
                        return;
                    }
                }
            }

            // Check for ServerBox placement
            ServerBox serverBox = collider.GetComponent<ServerBox>();
            if (serverBox != null)
            {
                if (objectData.type == ObjectType.Switch.ToString()) // Only switches, no NVRs
                {
                    if (!serverBox.HasAvailablePlace())
                    {
                        Debug.Log("У серверного ящика нет свободного места");
                        return;
                    }

                    Transform mountPoint = serverBox.GetClosestMountPoint(transform.position);
                    if (mountPoint != null)
                    {
                        CreateMountedDevice(mountPoint, null, serverBox);
                        return;
                    }
                }
                else if (objectData.type == ObjectType.NVR.ToString())
                {
                    Debug.Log("NVR нельзя установить в серверный ящик");
                    return;
                }
            }


            objectPrefab = Instantiate(objectPrefab, transform.position, transform.rotation);
            ObjectManager.Instance.AddObject(objectData, objectPrefab, collider); // creates an object 
            if (objectData.type == ObjectType.DoorLock.ToString())
            {
                DoorLockController ParentDoorWall = collider.GetComponentInParent<DoorLockController>();
                ParentDoorWall.SetDoorLock(objectPrefab);
                objectPrefab.GetComponent<DoorLock>().ParentDoorWallLockController = ParentDoorWall;
            }
            Destroy(previewObject); // Удаляем объект предварительного просмотра
            gameState = 0;

        }

        private void CreateMountedDevice(Transform mountPoint, ServerRack rack = null, ServerBox box = null)
        {
            GameObject newObject = Instantiate(objectPrefab, mountPoint.position, mountPoint.rotation);
            string newDeviceId = System.Guid.NewGuid().ToString();

            InteractiveObject interactiveObj = newObject.GetComponent<InteractiveObject>();
            if (interactiveObj != null)
            {
                interactiveObj.id = newDeviceId;
            }

            newObject.transform.SetParent(mountPoint);

            if (rack != null)
            {
                rack.OccupyMountPoint(mountPoint, newDeviceId);
            }
            else if (box != null)
            {
                box.OccupyMountPoint(mountPoint, newDeviceId);
            }

            ObjectManager.Instance.AddObject(objectData, newObject, rack != null ? rack.gameObject.GetComponent<Collider>() : box.gameObject.GetComponent<Collider>());
            Destroy(previewObject);
            gameState = 0;
        }


        // Add this method to handle removal of objects from rack
        public void RemoveObjectFromRack(GameObject obj)
        {
            ServerRack serverRack = obj.GetComponentInParent<ServerRack>();
            ServerBox serverBox = obj.GetComponentInParent<ServerBox>();
            Transform mountPoint = obj.transform.parent;
            InteractiveObject interactiveObj = obj.GetComponent<InteractiveObject>();

            if (serverRack != null && interactiveObj != null)
            {
                serverRack.FreeMountPoint(mountPoint, interactiveObj.id);
            }
            else if (serverBox != null && interactiveObj != null)
            {
                serverBox.FreeMountPoint(mountPoint, interactiveObj.id);
            }

            obj.transform.SetParent(null); // Unparent before destroying
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

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!(ObjectSettingsManager.objectSettings.activeSelf || UPSSettingsManager.UPSSettings.activeSelf || ScudSettings.scudSettings.activeSelf || CatalogManager.isItemsVisible || CatalogManager.customObjectForm.activeSelf || CatalogManager.PanelItems.activeSelf || CatalogManager.PanelPreview.activeSelf || CatalogManager.PanelInfo.activeSelf || CatalogManager.ExitMenu.activeSelf || Guide.guideMenu.activeSelf))
                    CatalogManager.ShowExitMenu();
            }

            if (EnterPanel.activeSelf || ObjectSettingsManager.objectSettings.activeSelf || UPSSettingsManager.UPSSettings.activeSelf || ScudSettings.scudSettings.activeSelf || CatalogManager.isItemsVisible || CatalogManager.ExitMenu.activeSelf || Guide.guideMenu.activeSelf)
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
                        if (interactiveObject != null && interactiveObject.type != ObjectType.ServerRack)
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
                else if (Input.GetKeyDown(KeyCode.Escape))
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
                else if (Input.GetKeyDown(KeyCode.Escape))
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


