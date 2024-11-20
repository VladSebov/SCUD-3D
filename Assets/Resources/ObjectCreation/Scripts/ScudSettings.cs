using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SCUD3D;
using Unity.VisualScripting;

public class ScudSettings : MonoBehaviour
{
    public Button AccessSettingsButton;
    public Button RolesSettingsButton;
    public Button CamerasSettingsButton;
    public Button RestrictionsSettingsButton;
    public Button UserSettingsButton;

    public GameObject AccessSettingsContent;
    public GameObject RolesSettingsContent;
    public GameObject CamerasSettingsContent;
    public GameObject RestrictionsSettingsContent;
    public GameObject UserSettingsContent;
    public GameObject scudSettings;
    public CamerasSettingsManager CamerasSettingsManager;


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

    //Restrictions
    public ScrollRect RestrictionsScroll;
    public GameObject RestrictionItem;
    private List<Restriction> restrictionsCopy;
    public Button SaveRestrictionsButton;
    public Button CancelRestrictionsButton;

    //User
    public ScrollRect UserRolesScroll;
    public GameObject UserRoleItem;
    private string selectedUserRole;
    public Button SaveUserSettingsButton;
    public Button CancelUserSettingsButton;

    void Start()
    {
        AvailableRolesMenuManager = GetComponent<AvailableRolesMenuManager>();
        CamerasSettingsManager = GetComponent<CamerasSettingsManager>();
        AccessSettingsButton.onClick.AddListener(ShowAccessSettingsContent);
        RolesSettingsButton.onClick.AddListener(ShowRolesSettingsContent);
        CamerasSettingsButton.onClick.AddListener(ShowCamerasSettingsContent);
        RestrictionsSettingsButton.onClick.AddListener(ShowRestrictionsSettingsContent);
        UserSettingsButton.onClick.AddListener(ShowUserSettingsContent);
        ShowAccessSettingsContent(); //show access settings by default

        CancelRestrictionsButton.onClick.AddListener(FillRestrictions);
        SaveRestrictionsButton.onClick.AddListener(SaveRestrictions);
    }

    private void SaveRestrictions()
    {
        if (!CheckRestrictionsCorrect())
        {
            Debug.Log("Новые ограничения противоречат текущему состоянию системы");
            return;
        }
        RestrictionsManager.Instance.SetRestrictions(restrictionsCopy);
    }
    private bool CheckRestrictionsCorrect()
    {
        // Assume all restrictions are correct initially
        bool allRestrictionsMet = true;

        foreach (Restriction restriction in restrictionsCopy)
        {
            switch (restriction.type)
            {
                case RestrictionType.MaxPrice:
                    if (ObjectManager.Instance.GetTotalPrice() > restriction.value)
                    {
                        allRestrictionsMet = false; // Not met
                    }
                    break;

                case RestrictionType.MaxRoles:
                    if (ScudManager.Instance.GetRoles().Count > restriction.value)
                    {
                        allRestrictionsMet = false; // Not met
                    }
                    break;

                case RestrictionType.MaxCameras:
                    if (ObjectManager.Instance.GetObjectsCountByType(ObjectType.Camera) > restriction.value)
                    {
                        allRestrictionsMet = false; // Not met
                    }
                    break;
            }
        }

        return allRestrictionsMet; // Return the final result
    }

    private void ResetButtonColors()
    {
        AccessSettingsButton.interactable = true;
        RolesSettingsButton.interactable = true;
        CamerasSettingsButton.interactable = true;
        RestrictionsSettingsButton.interactable = true;
        UserSettingsButton.interactable = true;
    }

    private void ShowAccessSettingsContent()
    {
        ResetButtonColors();
        AccessSettingsButton.interactable = false;
        HideAllContent();
        AccessSettingsContent.SetActive(true);
        FillAccessDevices();
    }

    private void ShowRolesSettingsContent()
    {
        ResetButtonColors();
        RolesSettingsButton.interactable = false;
        HideAllContent();
        RolesSettingsContent.SetActive(true);
        FillRoles();
    }

    private void ShowCamerasSettingsContent()
    {
        ResetButtonColors();
        CamerasSettingsButton.interactable = false;
        HideAllContent();
        CamerasSettingsContent.SetActive(true);
        CamerasSettingsManager.FillCameras();
    }

    private void ShowRestrictionsSettingsContent()
    {
        ResetButtonColors();
        RestrictionsSettingsButton.interactable = false;
        HideAllContent();
        RestrictionsSettingsContent.SetActive(true);
        FillRestrictions();
    }

    private void ShowUserSettingsContent()
    {
        ResetButtonColors();
        UserSettingsButton.interactable = false;
        HideAllContent();
        UserSettingsContent.SetActive(true);
        FillUser();
    }

    private void HideAllContent()
    {
        AccessSettingsContent.SetActive(false);
        RolesSettingsContent.SetActive(false);
        CamerasSettingsContent.SetActive(false);
        RestrictionsSettingsContent.SetActive(false);
        UserSettingsContent.SetActive(false);
    }


    public void UpdateRolesMenu()
    {
        FillRoles();
        // Update button visibility
        DeleteRoleButton.interactable = !string.IsNullOrEmpty(selectedRole);
    }

    public void SelectRole(string Role, Button button)
    {
        selectedRole = Role;
        DeleteRoleButton.interactable = !string.IsNullOrEmpty(selectedRole);
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
            scudSettings.SetActive(!scudSettings.activeSelf);
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
            button.onClick.AddListener(() => { SelectRole(role, button);});
        }
    }

    public void FillRestrictions()
    {
        var restrictions = RestrictionsManager.Instance.GetRestrictions();
        restrictionsCopy = new List<Restriction>();
        foreach (var restriction in restrictions)
        {
            restrictionsCopy.Add(restriction.Clone());
        }

        // Clear existing items in the scroll view
        foreach (Transform child in RestrictionsScroll.content)
        {
            Destroy(child.gameObject);
        }

        // Populate the scroll view with connected device IDs
        for (int i = 0; i < restrictions.Count; i++)
        {
            var restriction = restrictions[i];
            GameObject item = Instantiate(RestrictionItem, RestrictionsScroll.content);
            item.GetComponentInChildren<TextMeshProUGUI>().text = restriction.name;

            // Get the TMP_InputField and set its initial value
            TMP_InputField inputField = item.GetComponentInChildren<TMP_InputField>();
            inputField.text = restriction.value.ToString();

            // Capture the current index in a local variable
            int currentIndex = i; // Create a local copy of the index
            inputField.onValueChanged.AddListener((value) => OnInputFieldValueChanged(currentIndex, value));
        }
    }

    private void OnInputFieldValueChanged(int index, string newValue)
    {
        if (int.TryParse(newValue, out int intValue))
        {
            restrictionsCopy[index].value = intValue;
        }
        else
        {
            // Handle the case where the input is not a valid integer
            Debug.LogWarning($"Invalid input for restriction at index {index}: {newValue}");
        }
    }

    public void FillUser()
    {
        var roles = ScudManager.Instance.GetRoles();
        if (selectedUserRole == null)
        {
            selectedUserRole = PlayerManager.Instance.GetRole() ?? string.Empty;
        }
        // Clear existing items in the scroll view
        foreach (Transform child in UserRolesScroll.content)
        {
            Destroy(child.gameObject);
        }

        // Populate the scroll view with connected device IDs
        foreach (var role in roles)
        {
            GameObject item = Instantiate(UserRoleItem, UserRolesScroll.content);
            item.GetComponentInChildren<TextMeshProUGUI>().text = role;
            Button button = item.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => { SelectUserRole(role); });
            if (role == selectedUserRole)
            {
                button.interactable = false;
            }
        }
    }

    public void SelectUserRole(string role)
    {
        selectedUserRole = role;
        FillUser();
    }

    public void SaveUserRole()
    {
        PlayerManager.Instance.SetRole(selectedUserRole);
        selectedUserRole = null;
        FillUser();
    }

    public void CancelUserRole()
    {
        selectedUserRole = null;
        FillUser();
    }

    public void OnAddRoleClick()
    {
        ScudManager.Instance.AddRole();
        FillRoles();
    }
}
