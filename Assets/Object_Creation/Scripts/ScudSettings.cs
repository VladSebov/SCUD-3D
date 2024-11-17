using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEditor.IMGUI.Controls;
using UnityEngine.UI;

public class ScudSettings : MonoBehaviour
{
    public Button AccessSettingsButton;
    public Button RolesSettingsButton;
    public Button CamerasSettingsButton;
    public Button RestrictionsSettingsButton;
    public GameObject AccessSettingsContent;
    public GameObject RolesSettingsContent;
    public GameObject CamerasSettingsContent;
    public GameObject RestrictionsSettingsContent;
    public GameObject scudSettings;

    //Access
    public ScrollRect AccessDevicesScroll;
    public GameObject AccessDeviceItem;
    private string selectedAccessDeviceId;

    //Roles
    public ScrollRect RolesScroll;
    public GameObject RoleItem;
    public Button AddRoleButton;
    public Button DeleteRoleButton;
    public AvailableRolesMenuManager AvailableRolesMenuManager; // скрипт для available roles
    private string selectedRole;

    //Cameras
    public ScrollRect CamerasScroll;
    public GameObject CameraItem;
    public GameObject CameraViewer;
    private string currentCamera;

    //Restrictions
    public ScrollRect RestrictionsScroll;
    public GameObject RestrictionItem;

    void Start()
    {
        AvailableRolesMenuManager = GetComponent<AvailableRolesMenuManager>();
        AccessSettingsButton.onClick.AddListener(ShowAccessSettingsContent);
        RolesSettingsButton.onClick.AddListener(ShowRolesSettingsContent);
        CamerasSettingsButton.onClick.AddListener(ShowCamerasSettingsContent);
        RestrictionsSettingsButton.onClick.AddListener(ShowRestrictionsSettingsContent);
        ShowAccessSettingsContent(); //show access settings by default
    }

    private void ShowAccessSettingsContent(){
        HideAllContent();
        AccessSettingsContent.SetActive(true);
        FillAccessDevices();
    }

    private void ShowRolesSettingsContent(){
        HideAllContent();
        RolesSettingsContent.SetActive(true);
        FillRoles();
    }

    private void ShowCamerasSettingsContent(){
        HideAllContent();
        CamerasSettingsContent.SetActive(true);
        FillCameras();
    }

    private void ShowRestrictionsSettingsContent(){
        HideAllContent();
        RestrictionsSettingsContent.SetActive(true);

    }

    private void HideAllContent()
    {
        AccessSettingsContent.SetActive(false);
        RolesSettingsContent.SetActive(false);
        CamerasSettingsContent.SetActive(false);
        RestrictionsSettingsContent.SetActive(false);
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
        DeleteRoleButton.interactable = !string.IsNullOrEmpty(selectedRole);
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
        foreach (Transform child in CamerasScroll.content)
        {
            Destroy(child.gameObject);
        }

        // Populate the scroll view with connected device IDs
        foreach (var camera in cameras)
        {
            GameObject item = Instantiate(CameraItem, CamerasScroll.content);
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
        foreach (Transform child in AccessDevicesScroll.content)
        {
            Destroy(child.gameObject);
        }

        // Populate the scroll view with connected device IDs
        foreach (var device in accessDevices)
        {
            GameObject item = Instantiate(AccessDeviceItem, AccessDevicesScroll.content);
            item.GetComponentInChildren<TextMeshProUGUI>().text = device.id;
            Button button = item.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => { ShowAvailableRoles(device); });
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
        foreach (Transform child in RolesScroll.content)
        {
            Destroy(child.gameObject);
        }

        // Populate the scroll view with connected device IDs
        foreach (var role in roles)
        {
            GameObject item = Instantiate(RoleItem, RolesScroll.content);
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
}
