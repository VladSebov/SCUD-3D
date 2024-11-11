using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEditor.IMGUI.Controls;
using UnityEngine.UI;

public class ScudSettings : MonoBehaviour
{
    public GameObject scudSettings;
    public TextMeshProUGUI MenuHeader;
    public GameObject MenuAccessButton;
    public GameObject MenuRolesButton;
    public GameObject MenuCamerasButton;
    public Button AddButton;

    public Button DeleteButton;
    public ScrollRect ScrollView;
    public GameObject CameraViewer;
    public GameObject ItemPrefab;
    public int SelectedMenu = 0;
    private string currentCamera;

    private string selectedRole;
    private string selectedAccessDeviceId;
    public AvailableRolesMenuManager AvailableRolesMenuManager; // скрипт для available roles


    void Start()
    {
        AvailableRolesMenuManager = GetComponent<AvailableRolesMenuManager>();
    }

    void ShowCameraView(string CameraId, bool status)
    {
        currentCamera = CameraId;
        var cameraGameObject = ObjectManager.Instance.GetObject(CameraId);
        cameraGameObject.gameObject.GetComponentInChildren<Camera>().enabled = status;
        CameraViewer.SetActive(status);
    }

    public void UpdateRolesMenu()
    {
        FillRoles();
        // Update button visibility
        DeleteButton.interactable = !string.IsNullOrEmpty(selectedRole);
    }

    public void SelectRole(string Role)
    {
        selectedRole = Role;
        UpdateRolesMenu();
    }

    public void DeleteRole()
    {
        if (!string.IsNullOrEmpty(selectedRole))
        {
            ScudManager.Instance.RemoveRole(selectedRole);
            selectedRole = null;
            UpdateRolesMenu();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            CameraViewer.SetActive(false);
            SelectedMenu = 0;
            scudSettings.SetActive(!scudSettings.activeSelf);
            Cursor.visible = true;
        }
        if (CameraViewer.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            ShowCameraView(currentCamera, false);
        }
    }

    public void FillCameras()
    {
        var cameras = ObjectManager.Instance.GetAllObjects()
    .Where(io => io.type == ObjectType.Camera) // Фильтруем объекты типа Camera
    .ToList();
        // Clear existing items in the scroll view
        foreach (Transform child in ScrollView.content)
        {
            Destroy(child.gameObject);
        }

        // Populate the scroll view with connected device IDs
        foreach (var camera in cameras)
        {
            GameObject item = Instantiate(ItemPrefab, ScrollView.content);
            item.GetComponentInChildren<TextMeshProUGUI>().text = camera.id;
            Button button = item.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => ShowCameraView(camera.id, true));
        }
    }

    public void FillAccessDevices()
    {
        var accessDevices = ObjectManager.Instance.GetAllObjects()
            .Where(io => io.type == ObjectType.Turnstile || io.type == ObjectType.Terminal) // Фильтруем объекты типа Camera
            .ToList();
        // Clear existing items in the scroll view
        foreach (Transform child in ScrollView.content)
        {
            Destroy(child.gameObject);
        }

        // Populate the scroll view with connected device IDs
        foreach (var device in accessDevices)
        {
            GameObject item = Instantiate(ItemPrefab, ScrollView.content);
            item.GetComponentInChildren<TextMeshProUGUI>().text = device.id;
            Button button = item.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => { ShowAvailableRoles(device);});
        }
    }

    public void ShowAvailableRoles(InteractiveObject accessDevice) // You can modify this to get input from the user
    {
        AvailableRolesMenuManager.ShowMenu(accessDevice);
    }

    public void FillRoles()
    {
        var roles = ScudManager.Instance.GetRoles();
        // Clear existing items in the scroll view
        foreach (Transform child in ScrollView.content)
        {
            Destroy(child.gameObject);
        }

        // Populate the scroll view with connected device IDs
        foreach (var role in roles)
        {
            GameObject item = Instantiate(ItemPrefab, ScrollView.content);
            item.GetComponentInChildren<TextMeshProUGUI>().text = role;
            Button button = item.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => { SelectRole(role); });
        }
    }

    public void OnAddRoleClick()
    {
        ScudManager.Instance.AddRole();
        FillRoles();
    }

    public void ViewMenu(int SelectedMenu)
    {
        switch (SelectedMenu)
        {
            case 0:
                MenuHeader.text = "Список устройств доступа";
                AddButton.gameObject.SetActive(false);
                DeleteButton.gameObject.SetActive(false);
                FillAccessDevices();
                break;
            case 1:
                MenuHeader.text = "Список ролей";
                AddButton.gameObject.SetActive(true);
                DeleteButton.gameObject.SetActive(true);
                FillRoles();
                break;
            case 2:
                MenuHeader.text = "Список камер";
                AddButton.gameObject.SetActive(false);
                DeleteButton.gameObject.SetActive(false);
                FillCameras();
                break;
        }
    }
}
