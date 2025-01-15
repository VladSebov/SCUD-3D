using System;
using System.Collections.Generic;
using StarterAssets;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using SCUD3D;
using UnityEngine.UI;

public class ObjectSettingsManager : MonoBehaviour
{
    public GameObject objectSettings; // Reference to the objectSettings
    public GameObject selectConnectionForm;
    public int currentCableType;
    public InteractiveObject interactiveObject; // Reference to the InteractiveObject
    public TextMeshProUGUI connectionCountText; // Reference to the Text for connection count
    public TextMeshProUGUI objectNameText; // Reference to the Text for name
    public ScrollRect scrollView; // Reference to the Scroll View
    public GameObject connectionItemPrefab; // Prefab for displaying connection items
    public Button deleteButton; // Reference to the delete button
    public Button addConnectionButton; // Reference to the add button
    private Connection selectedConnection; // Store the selected connection ID

    //UPS settings
    public GameObject UPSPanel;
    public TextMeshProUGUI UPSInfoText;
    public Button UPSActionButton;

    //Camera's rotating
    public GameObject CameraRotatingPanel;
    public Slider verticalRotatingSlider;
    public TextMeshProUGUI verticalAngleValue;
    public Slider horizontalRotatingSlider;
    public TextMeshProUGUI horizontalAngleValue;
    public RawImage cameraPreview;

    private CablePlacer CablePlacer;
    public MenuDevicesManager MenuDevicesManager; // скрипт для menuDevices
    public UPSSettingsManager UPSSettingsManager; // скрипт для menuDevices

    public void Start()
    {
        CablePlacer = GetComponent<CablePlacer>();
        MenuDevicesManager = GetComponent<MenuDevicesManager>();
        UPSSettingsManager = GetComponent<UPSSettingsManager>();
    }

    public void ShowSelectConnectionForm(int cableType)
    {
        currentCableType = cableType;
        selectConnectionForm.SetActive(true);
    }

    public void AddConnectionManually()
    {
        CloseMenu();
        CablePlacer.StartCablePlacement(interactiveObject, currentCableType);
    }

    public void ShowMenu(InteractiveObject obj)
    {
        interactiveObject = obj;
        if (interactiveObject.type == ObjectType.UPS)
        {
            UPSSettingsManager.ShowMenu((UPS)interactiveObject);
        }
        else
        {
            UpdateMenu();
            objectSettings.SetActive(true);

            // Show/hide panels based on object type
            if (interactiveObject.type == ObjectType.Camera)
            {
                UPSPanel.SetActive(false);
                CameraRotatingPanel.SetActive(true);
                SetupCameraPreview();
                UpdateSlidersForSelectedCamera();
            }
            else
            {
                UPSPanel.SetActive(true);
                CameraRotatingPanel.SetActive(false);
            }
        }
    }

    private void SetupCameraPreview()
    {
        Camera cameraComponent = interactiveObject.GetComponentInChildren<Camera>();
        cameraComponent.enabled = true;
        if (cameraComponent != null && cameraPreview != null)
        {
            RenderTexture renderTexture = new RenderTexture(1024, 1024, 32);
            cameraComponent.targetTexture = renderTexture;
            cameraPreview.texture = renderTexture;
        }
    }

    // Управление поворотом камеры
    public void OnVerticalAngleSliderChanged(float value)
    {
        Transform camera = interactiveObject.gameObject.transform.Find("GameObject");
        //Transform camera = cameraGameObject.gameObject.transform.Find("Camera");

        Vector3 currentRotation = camera.eulerAngles;
        Vector3 newRotation = new Vector3(Mathf.Clamp(value, -89f, 89f), currentRotation.y, currentRotation.z);

        camera.eulerAngles = newRotation;

        verticalAngleValue.text = $"{value:F1}°";
    }

    public void OnHorizontalAngleSliderChanged(float value)
    {
        Transform camera = interactiveObject.gameObject.transform.Find("GameObject");
        //Transform camera = cameraGameObject.gameObject.transform.Find("Camera");

        Vector3 currentRotation = camera.eulerAngles;
        Vector3 newRotation = new Vector3(currentRotation.x, value, currentRotation.z);

        camera.eulerAngles = newRotation;
        horizontalAngleValue.text = $"{value:F1}°";
    }

    // Обновление ползунков для выбранной камеры
    public void UpdateSlidersForSelectedCamera()
    {
        Transform camera = interactiveObject.gameObject.transform.Find("GameObject");
        //Transform camera = cameraGameObject.gameObject.transform.Find("Camera");
        if (camera == null) return;

        verticalRotatingSlider.value = NormalizeAngle(camera.eulerAngles.x);
        horizontalRotatingSlider.value = NormalizeAngle(camera.eulerAngles.y);

        // Format the text to show regular decimal numbers with 1 decimal place
        verticalAngleValue.text = $"{NormalizeAngle(camera.eulerAngles.x):F1}°";
        horizontalAngleValue.text = $"{NormalizeAngle(camera.eulerAngles.y):F1}°";
    }

    // Utility method to normalize angles (e.g., -180 to 180)
    private float NormalizeAngle(float angle)
    {
        angle = angle % 360; // Ensure the angle stays within 0 to 360
        if (angle > 180) angle -= 360; // Convert to -180 to 180 range
        return angle;
    }

    public void ShowAvailableDevices()
    {
        selectConnectionForm.SetActive(false);
        MenuDevicesManager.ShowMenu(interactiveObject, currentCableType);
    }

    void Update()
    {
        // Check if the menu is active and the Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }
    }

    public void CloseMenu()
    {
        selectConnectionForm.SetActive(false);
        objectSettings.SetActive(false);
        UPSSettingsManager.CloseMenu();
        selectedConnection = null;

        // Cleanup camera preview if it exists
        // if (cameraPreview != null && cameraPreview.texture != null)
        // {
        //     if (cameraPreview.texture is RenderTexture rt)
        //     {
        //         rt.Release();
        //         Destroy(rt);
        //     }
        //     cameraPreview.texture = null;
        // }
    }

    public void UpdateMenu()
    {
        int currentObjectConnectionsCount = ConnectionsManager.Instance.GetEthernetConnections(interactiveObject).Count;
        // Update connection count text
        connectionCountText.text = $"{currentObjectConnectionsCount} / {interactiveObject.maxConnections}";
        objectNameText.text = interactiveObject.name;
        FillConnections();

        FillUPSConnection();

        // Update button visibility
        deleteButton.interactable = selectedConnection != null;
        addConnectionButton.interactable = currentObjectConnectionsCount < interactiveObject.maxConnections;
    }

    public void SelectConnection(Connection connection)
    {
        selectedConnection = connection;
        UpdateMenu();
    }

    public void DeleteConnection()
    {
        if (selectedConnection != null)
        {
            ConnectionsManager.Instance.RemoveConnection(selectedConnection);
            selectedConnection = null;
            UpdateMenu();
        }
    }

    public void FillConnections()
    {
        // Clear existing items in the scroll view
        foreach (Transform child in scrollView.content)
        {
            Destroy(child.gameObject);
        }
        List<Connection> currentObjectconnections = ConnectionsManager.Instance.GetEthernetConnections(interactiveObject);
        // Populate the scroll view with connected device IDs
        foreach (var connection in currentObjectconnections)
        {
            InteractiveObject otherObject = connection.ObjectA == interactiveObject ? connection.ObjectB : connection.ObjectA;
            GameObject item = Instantiate(connectionItemPrefab, scrollView.content);
            item.GetComponentInChildren<TextMeshProUGUI>().text = otherObject.id;
            Button button = item.GetComponentInChildren<Button>();
            if (connection == selectedConnection)
            {
                button.GetComponent<Image>().color = Color.gray;
            }
            button.onClick.AddListener(() => SelectConnection(connection));
        }
    }
    public void FillUPSConnection()
    {
        // Check if the interactive object implements ConnectableToUPS
        if (interactiveObject is ConnectableToUPS connectableToUPS)
        {
            List<Connection> UPSConnections = ConnectionsManager.Instance.GetConnectionsByType(interactiveObject, ObjectType.UPS);
            // Check if connectedUPSId is not null
            if (UPSConnections.Count > 0)
            {
                InteractiveObject connectedUPS = UPSConnections[0].ObjectA.type == ObjectType.UPS ? UPSConnections[0].ObjectA : UPSConnections[0].ObjectB;

                UPSInfoText.text = $"Connected to UPS: {connectedUPS.name}";
                UPSActionButton.gameObject.SetActive(true);
                UPSActionButton.onClick.RemoveAllListeners(); // Clear previous listeners
                UPSActionButton.onClick.AddListener(() =>
                {
                    ConnectionsManager.Instance.RemoveConnection(UPSConnections[0]);
                    UpdateMenu(); // Refresh the menu
                });
                UPSActionButton.GetComponentInChildren<TextMeshProUGUI>().text = "Отключить";
            }
            else
            {
                UPSInfoText.text = "Not connected to any UPS";
                UPSActionButton.gameObject.SetActive(true);
                UPSActionButton.onClick.RemoveAllListeners(); // Clear previous listeners
                UPSActionButton.onClick.AddListener(() =>
                {
                    ShowSelectConnectionForm(CableType.UPS);
                });
                UPSActionButton.GetComponentInChildren<TextMeshProUGUI>().text = "Подключить";
            }
        }
        else
        {
            UPSInfoText.text = "This device doesn't support UPS";
            UPSActionButton.gameObject.SetActive(false);
        }
    }

}