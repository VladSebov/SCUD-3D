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

    public Button StatisticsButton;

    public GameObject AccessSettingsContent;
    public GameObject RolesSettingsContent;
    public GameObject CamerasSettingsContent;
    public GameObject RestrictionsSettingsContent;
    public GameObject UserSettingsContent;
    public GameObject StatisticsContent;

    public GameObject scudSettings;
    public CamerasSettingsManager CamerasSettingsManager;

    //Statistics
    public TextMeshProUGUI TotalPriceText;
    public TextMeshProUGUI TotalAmountText;
    public TextMeshProUGUI EthernetCableLengthText;
    public TextMeshProUGUI UPSCableLengthText;
    public ScrollRect StatisticsScroll;
    public GameObject StatisticsItemPrefab;

    public ScrollRect ConnectedToUPSScroll;
    public GameObject ConnectedUPSItemPrefab;
    public TextMeshProUGUI AutonomyDurationText;

    public TextMeshProUGUI PowerConsumptionText;
    public TextMeshProUGUI UPSPercentageText;

    //Access
    public ScrollRect AccessControllersScroll;
    public GameObject AccessControllerItem;
    public TextMeshProUGUI accessHintText; // Add this field

    private string selectedAccessControllerId;

    //Roles
    public ScrollRect RolesScroll;
    public GameObject RoleItem;
    public Button AddRoleButton;
    public Button DeleteRoleButton;
    public TextMeshProUGUI rolesHintText; // Add this field
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

    public GameObject AddUserItem;

    public GameObject createUserPanel;

    public Button CreateUserButton;
    public Button ApplyUserButton;
    public GameObject DeleteUserButton;

    public TextMeshProUGUI createUserPanelHeader;

    public ScrollRect AccessGroupsScroll;
    public GameObject AccessGroupItem;
    public GameObject AddAccessGroupItem;

    public GameObject createAccessGroupsPanel;
    public TextMeshProUGUI createAccessGroupsPanelHeader;

    public TMP_InputField accessGroupNameInputField;

    public ScrollRect AccessGroupsUsersScroll;
    public ScrollRect AccessGroupsDevicesScroll;
    public GameObject AccessGroupUserItem;
    public GameObject AccessGroupDeviceItem;
    public Button CreateAccessGroupButton;
    public Button DeleteAccessGroupButton;
    public TextMeshProUGUI userHintText; // Add this field
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
        StatisticsButton.onClick.AddListener((ShowStatisticsContent));

        CancelRestrictionsButton.onClick.AddListener(FillRestrictions);
        SaveRestrictionsButton.onClick.AddListener(SaveRestrictions);
    }





    private void SaveRestrictions()
    {
        if (!CheckRestrictionsCorrect())
        {
            MessageManager.Instance.ShowMessage("Новые ограничения противоречат текущему состоянию системы");
            return;
        }
        RestrictionsManager.Instance.SetRestrictions(restrictionsCopy);
        FillRestrictions();
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
                    if (ScudManager.Instance.GetUsers().Count > restriction.value)
                    {
                        allRestrictionsMet = false; // Not met
                    }
                    break;

                case RestrictionType.MaxCameras:
                    if (ObjectManager.Instance.GetObjectsByType(ObjectType.Camera).Count > restriction.value)
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
        Color targetColor = new Color32(0, 36, 63, 255); // HEX #00243F

        AccessSettingsButton.interactable = true;
        RolesSettingsButton.interactable = true;
        CamerasSettingsButton.interactable = true;
        RestrictionsSettingsButton.interactable = true;
        UserSettingsButton.interactable = true;
        StatisticsButton.interactable = true;

        SetButtonTextColor(AccessSettingsButton, targetColor);
        SetButtonTextColor(RolesSettingsButton, targetColor);
        SetButtonTextColor(CamerasSettingsButton, targetColor);
        SetButtonTextColor(RestrictionsSettingsButton, targetColor);
        SetButtonTextColor(UserSettingsButton, targetColor);
        SetButtonTextColor(StatisticsButton, targetColor);
    }

    private void SetButtonTextColor(Button button, Color color)
    {
        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.color = color;
        }
        else
        {
            Debug.LogWarning($"Не найден TextMeshProUGUI у кнопки {button.name}");
        }
    }

    private void ShowAccessSettingsContent()
    {
        ResetButtonColors();
        AccessSettingsButton.interactable = false;
        SetButtonTextColor(AccessSettingsButton, Color.white);
        HideAllContent();
        AccessSettingsContent.SetActive(true);
        FillAccessControllers();
    }

    private void ShowRolesSettingsContent()
    {
        ResetButtonColors();
        RolesSettingsButton.interactable = false;
        SetButtonTextColor(RolesSettingsButton, Color.white);
        HideAllContent();
        RolesSettingsContent.SetActive(true);
        FillAccessGroups();
    }

    private void ShowCamerasSettingsContent()
    {
        ResetButtonColors();
        CamerasSettingsButton.interactable = false;
        SetButtonTextColor(CamerasSettingsButton, Color.white);
        HideAllContent();
        CamerasSettingsContent.SetActive(true);
        CamerasSettingsManager.FillCameras();
    }

    private void ShowRestrictionsSettingsContent()
    {
        ResetButtonColors();
        RestrictionsSettingsButton.interactable = false;
        SetButtonTextColor(RestrictionsSettingsButton, Color.white);
        HideAllContent();
        RestrictionsSettingsContent.SetActive(true);
        FillRestrictions();
    }

    private void ShowUserSettingsContent()
    {
        ResetButtonColors();
        UserSettingsButton.interactable = false;
        SetButtonTextColor(UserSettingsButton, Color.white);
        HideAllContent();
        UserSettingsContent.SetActive(true);
        FillUsers();
    }

    private void ShowStatisticsContent()
    {
        ResetButtonColors();
        StatisticsButton.interactable = false;
        SetButtonTextColor(StatisticsButton, Color.white);
        HideAllContent();
        StatisticsContent.SetActive(true);
        FillStatistics();
    }

    private void HideAllContent()
    {
        AccessSettingsContent.SetActive(false);
        RolesSettingsContent.SetActive(false);
        CamerasSettingsContent.SetActive(false);
        RestrictionsSettingsContent.SetActive(false);
        UserSettingsContent.SetActive(false);
        StatisticsContent.SetActive(false);
    }



    public void UpdateRolesMenu()
    {
        //FillRoles();
        // Update button visibility
        DeleteRoleButton.interactable = !string.IsNullOrEmpty(selectedRole);
    }

    public void SelectRole(string Role, Button button)
    {
        selectedRole = Role;
        DeleteRoleButton.interactable = !string.IsNullOrEmpty(selectedRole);
        //FillRoles();
    }

    /*     public void DeleteRole()
        {
            if (!string.IsNullOrEmpty(selectedRole))
            {
                ScudManager.Instance.RemoveRole(selectedRole);
                selectedRole = null;
                UpdateRolesMenu();
            }
        } */

    // Update is called once per frame
    void Update()
    {
        if (InputHelper.IsTypingInInputField())
            return;
        if (Input.GetKeyDown(KeyCode.P))
        {
            // destroy all cameras when closing form for optimization
            if (scudSettings.activeSelf)
                CamerasSettingsManager.HideAllCameraPanels();
            else
                ShowAccessSettingsContent(); //show access settings by default

            scudSettings.SetActive(!scudSettings.activeSelf);
        }
        // Check if the menu is active and the Escape key is pressed
        if (scudSettings.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CamerasSettingsManager.HideAllCameraPanels();
            scudSettings.SetActive(false);
        }
    }


    public void FillAccessControllers()
    {
        // Получаем все контроллеры доступа, подключенные к PC
        var accessControllers = ObjectManager.Instance.GetAllObjects()
            .Where(io => io.type == ObjectType.AccessController &&
                        ConnectionsManager.Instance.GetConnectedObjectsByType(io, ObjectType.Computer).Any())
            .ToList();


        // Clear existing items in the scroll view
        foreach (Transform child in AccessControllersScroll.content)
        {
            Destroy(child.gameObject);
        }

        if (accessControllers.Count == 0)
        {
            accessHintText.gameObject.SetActive(true);
            return;
        }

        accessHintText.gameObject.SetActive(false);

        // Populate the scroll view with connected device IDs
        foreach (var device in accessControllers)
        {
            GameObject item = Instantiate(AccessControllerItem, AccessControllersScroll.content);
            item.GetComponentInChildren<TextMeshProUGUI>().text = device.id;
            Button button = item.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => { ShowAvailableRoles(device); });
        }
    }

    public void ShowAvailableRoles(InteractiveObject accessController) // You can modify this to get input from the user
    {
        AvailableRolesMenuManager.ShowMenu(accessController);
    }

    /*     public void FillRoles()
        {
            var roles = ScudManager.Instance.GetRoles();
            // Clear existing items in the scroll view
            foreach (Transform child in RolesScroll.content)
            {
                Destroy(child.gameObject);
            }

            if (roles.Count == 0)
            {
                rolesHintText.gameObject.SetActive(true);
                return;
            }

            //rolesHintText.gameObject.SetActive(false);

            // Populate the scroll view with connected device IDs
            foreach (var role in roles)
            {
                GameObject item = Instantiate(RoleItem, RolesScroll.content);
                item.GetComponentInChildren<TextMeshProUGUI>().text = role;
                Button button = item.GetComponentInChildren<Button>();
                if (role == selectedRole)
                {
                    button.GetComponent<Image>().color = Color.gray;
                }
                GameObject createGroupItem = Instantiate(AddGroupItem, RolesScroll.content);
                button.onClick.AddListener(() => { SelectRole(role, button); });
            }
        } */

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
    }

    public void FillUsers()
    {
        var users = ScudManager.Instance.GetUsers();
        // Clear existing items in the scroll view
        foreach (Transform child in UserRolesScroll.content)
        {
            Destroy(child.gameObject);
        }

        // Populate the scroll view with connected device IDs
        foreach (var user in users)
        {
            GameObject item = Instantiate(UserRoleItem, UserRolesScroll.content);
            item.GetComponentInChildren<TextMeshProUGUI>().text = user.username;
            Button UserButton = item.GetComponentInChildren<Button>();
            UserButton.onClick.AddListener(() => { ShowEditUserPanel(user); });
        }
        GameObject createUserItem = Instantiate(AddUserItem, UserRolesScroll.content);
        Button createUserButton = createUserItem.GetComponentInChildren<Button>();
        createUserButton.onClick.AddListener(() => { ShowCreateUserPanel(); });
    }

    public void FillAccessGroups()
    {
        var accessGroups = ScudManager.Instance.GetAccessGroups();
        // Clear existing items in the scroll view
        foreach (Transform child in AccessGroupsScroll.content)
        {
            Destroy(child.gameObject);
        }

        // Populate the scroll view with connected device IDs
        foreach (var accessGroup in accessGroups)
        {
            GameObject item = Instantiate(AccessGroupItem, AccessGroupsScroll.content);
            item.GetComponentInChildren<TextMeshProUGUI>().text = accessGroup.name;
            Button AccessGroupButton = item.GetComponentInChildren<Button>();
            AccessGroupButton.onClick.AddListener(() => { ShowEditAccessGroups(accessGroup); });
        }
        GameObject createAccessGroupItem = Instantiate(AddAccessGroupItem, AccessGroupsScroll.content);
        Button createAccessGroupButton = createAccessGroupItem.GetComponentInChildren<Button>();
        createAccessGroupButton.onClick.AddListener(() => { ShowCreateAccessGroups(); });
    }

    public void ShowEditUserPanel(User user)
    {
        createUserPanel.SetActive(true);
        createUserPanelHeader.text = "Редактирование пользователя";
        if (user.id == 0)
        {
            ScudManager.Instance.userNameInputField.interactable = false;
            DeleteUserButton.SetActive(false);
        }
        if (user.id != 0)
        {
            ScudManager.Instance.userNameInputField.interactable = true;
            DeleteUserButton.SetActive(true);
        }
        ApplyUserButton.gameObject.SetActive(true);
        ScudManager.Instance.FillUserForm(user);
        CreateUserButton.onClick.RemoveAllListeners();
        CreateUserButton.onClick.AddListener(() => SaveUserChanges(ref user));
        ApplyUserButton.onClick.RemoveAllListeners();
        ApplyUserButton.onClick.AddListener(() => ScudManager.Instance.ApplyUser(ref user));
        DeleteUserButton.GetComponent<Button>().onClick.RemoveAllListeners();
        DeleteUserButton.GetComponent<Button>().onClick.AddListener(() => ScudManager.Instance.DeleteUser(ref user));
    }

    public void ShowCreateUserPanel()
    {
        createUserPanel.SetActive(true);
        DeleteUserButton.SetActive(false);
        createUserPanelHeader.text = "Создание пользователя";
        CreateUserButton.onClick.RemoveAllListeners();
        CreateUserButton.onClick.AddListener(() => ScudManager.Instance.ConfirmAddUser());
    }

    private void SaveUserChanges(ref User user)
    {
        if (user != null)
        {
            ScudManager.Instance.UpdateUserFromForm(ref user);
            ScudManager.Instance.ResetUserForm();
            MessageManager.Instance.ShowMessage("Изменения сохранены");
            FillUsers();
        }
    }

    private void SaveAccessGroupChanges(ref AccessGroup accessGroup)
    {
        if (accessGroup != null)
        {
            ScudManager.Instance.UpdateAccessGroupFromForm(ref accessGroup);
            ScudManager.Instance.HideCreateAccessGroupsPanel();
            MessageManager.Instance.ShowMessage("Изменения сохранены");
            FillAccessGroups();
        }
    }

    public void ShowCreateAccessGroups()
    {
        createAccessGroupsPanel.SetActive(true);
        createAccessGroupsPanelHeader.text = "Создание группы доступа";
        accessGroupNameInputField.text = "";
        FillUsersList();
        FillDevicesList();
        CreateAccessGroupButton.onClick.RemoveAllListeners();
        CreateAccessGroupButton.onClick.AddListener(() => ScudManager.Instance.ConfirmAddAccessGroup());
    }

    public void ShowEditAccessGroups(AccessGroup accessGroup)
    {
        createAccessGroupsPanel.SetActive(true);
        createAccessGroupsPanelHeader.text = "Редактирование группы доступа";
        accessGroupNameInputField.text = accessGroup.name;
        FillUsersList();
        FillDevicesList();
        FillAccessGroupListsToogles(accessGroup);
        CreateAccessGroupButton.onClick.RemoveAllListeners();
        CreateAccessGroupButton.onClick.AddListener(() => SaveAccessGroupChanges(ref accessGroup));
    }

    public void FillUsersList()
    {
        var users = ScudManager.Instance.GetUsers();
        // Clear existing items in the scroll view
        foreach (Transform child in AccessGroupsUsersScroll.content)
        {
            Destroy(child.gameObject);
        }

        // Populate the scroll view with connected device IDs
        foreach (var user in users)
        {
            GameObject item = Instantiate(AccessGroupUserItem, AccessGroupsUsersScroll.content);
            item.GetComponentInChildren<TextMeshProUGUI>().text = user.username;
        }
    }

    public void FillAccessGroupListsToogles(AccessGroup accessGroup)
    {
        foreach (Transform child in AccessGroupsUsersScroll.content)
        {
            if (accessGroup.chosenUsers.Contains(child.GetComponentInChildren<TextMeshProUGUI>().text)) child.GetComponentInChildren<Toggle>().isOn = true;
        }
        foreach (Transform child in AccessGroupsDevicesScroll.content)
        {
            if (accessGroup.chosenDevices.Contains(child.GetComponentInChildren<TextMeshProUGUI>().text)) child.GetComponentInChildren<Toggle>().isOn = true;
        }
    }

    public void FillDevicesList()
    {
        foreach (Transform child in AccessGroupsDevicesScroll.content)
        {
            Destroy(child.gameObject);
        }
        var controllers = ObjectManager.Instance.GetAllObjects()
            .Where(io => io.type == ObjectType.AccessController &&
                        ConnectionsManager.Instance.GetConnectedObjectsByType(io, ObjectType.Computer).Any())
            .ToList();
        foreach (var controller in controllers)
        {
            List<Connection> connections = ConnectionsManager.Instance.GetAllConnections(controller);
            foreach (var connection in connections)
            {
                if (connection != null)
                {
                    InteractiveObject device = connection.ObjectA == controller ? connection.ObjectB : connection.ObjectA;
                    if (device != null && (device.type == ObjectType.DoorLock || device.type == ObjectType.Turnstile))
                    {
                        GameObject item = Instantiate(AccessGroupDeviceItem, AccessGroupsDevicesScroll.content);
                        item.GetComponentInChildren<TextMeshProUGUI>().text = device.id;
                    }
                }
            }
        }
    }

    //Calculate Power
    public static float CalculateTotalPowerConsumption(List<InteractiveObject> allObjects)
    {
        float totalPowerConsumption = 0;

        foreach (var obj in allObjects)
        {
            if (obj is ConnectableToUPS)
            {
                if (obj is Switch switchObj)
                {
                    totalPowerConsumption += CalculateSwitchPowerConsumption(switchObj);
                }
                else
                {
                    totalPowerConsumption += obj.powerConsumption;
                }
            }
        }

        return totalPowerConsumption;
    }

    private static float CalculateSwitchPowerConsumption(Switch switchObj)
    {
        float switchPowerConsumption = switchObj.powerConsumption;
        List<Connection> connections = ConnectionsManager.Instance.GetEthernetConnections(switchObj);

        foreach (var connection in connections)
        {
            InteractiveObject connectedObject = connection.ObjectA == switchObj ? connection.ObjectB : connection.ObjectA;
            if (connectedObject is MyCamera camera)
            {
                switchPowerConsumption += camera.powerConsumption;
            }
        }

        return switchPowerConsumption;
    }

    //UPS
    private static float CalculateTotalBatteryPower(List<InteractiveObject> allObjects)
    {
        float totalPower = 0;

        foreach (var obj in allObjects)
        {
            if (obj is UPS ups)
            {
                foreach (string batteryId in ups.connectedBatteries)
                {
                    Battery battery = allObjects.Find(o => o is Battery && o.id == batteryId) as Battery;
                    if (battery != null)
                    {
                        totalPower += battery.powerWatts;
                    }
                }
            }
        }

        return totalPower;
    }



    public void FillStatistics()
    {
        float TotalPrice = 0;
        int TotalAmount = 0;

        foreach (Transform child in StatisticsScroll.content)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in ConnectedToUPSScroll.content)
        {
            Destroy(child.gameObject);
        }

        var objectTypes = System.Enum.GetValues(typeof(ObjectType));
        foreach (ObjectType type in objectTypes)
        {
            List<InteractiveObject> objects = ObjectManager.Instance.GetObjectsByTypeExt(type);
            if (objects.Count > 0)
            {
                // Calculate total price for this type
                float totalPrice = objects.Sum(obj => obj.price);

                // Instantiate a new statistics item
                GameObject item = Instantiate(StatisticsItemPrefab, StatisticsScroll.content);
                StatisticsItem statItem = item.GetComponent<StatisticsItem>();

                // Set the item's values
                statItem.SetValues(type.ToString(), objects.Count, totalPrice);

                // Accumulate total price and amount
                TotalPrice += totalPrice;
                TotalAmount += objects.Count;
            }
        }

        //UPS and Power
        List<InteractiveObject> allObjects = ObjectManager.Instance.GetAllObjects();
        float totalPowerConsumption = CalculateTotalPowerConsumption(allObjects);
        PowerConsumptionText.text = $"Энергопотребление: {totalPowerConsumption} Вт";
        float totalUPSPower = CalculateTotalBatteryPower(allObjects);

        if (totalUPSPower != 0 && totalPowerConsumption != 0)
        {
            float AutonomyDuration = totalUPSPower / totalPowerConsumption;
            AutonomyDurationText.text = $"Время автономной работы: {AutonomyDuration} ч";
        }
        else if (totalUPSPower == 0)
        {
            AutonomyDurationText.text = $"К ИБП не подключены АКБ";
        }
        else
        {
            AutonomyDurationText.text = $"Нет подключенных устройств";
        }

        //Connected To UPs
        // Gather all devices connected to any UPS
        List<UPS> upsObjects = allObjects.OfType<UPS>().ToList(); // Get all UPS objects

        // Create a list to hold all objects connected to any UPS, either directly or indirectly
        List<InteractiveObject> allConnectedDevices = new List<InteractiveObject>();

        // Get all interactive objects in the system
        List<InteractiveObject> connectableToUPSObjects = ObjectManager.Instance.GetAllObjects()
            .Where(io => io.type != ObjectType.UPS && io.type != ObjectType.Battery && io.type != ObjectType.ServerRack && io.type != ObjectType.ServerBox).ToList();
        foreach (InteractiveObject obj in connectableToUPSObjects)
        {
            // Check if the object is connected to a UPS directly or via a switch
            if (IsConnectedToUPSIndirectly(obj))
            {
                allConnectedDevices.Add(obj);
            }
        }

        int connectedTotalAmount = allConnectedDevices.Count();
        int connectableToUPSTotalAmount = connectableToUPSObjects.Count();
        if (TotalAmount != 0)
        {
            float UPSPercentage = ((float)connectedTotalAmount / connectableToUPSTotalAmount) * 100f;
            UPSPercentageText.text = $"Подключено к ИБП устройств:{UPSPercentage} %";
        }
        else
        {
            UPSPercentageText.text = $"Нет устройств";
        }

        // Group all connected devices by type and count them
        var groupedDevices = allConnectedDevices
            .GroupBy(device => device.type)
            .ToDictionary(group => group.Key, group => group.Count());

        // Populate the UPSConnectedScroll with the grouped types and counts
        foreach (var group in groupedDevices)
        {
            ObjectType deviceType = group.Key;
            int count = group.Value;

            // Instantiate a new UPSConnectedItem for each type
            GameObject connectedItem = Instantiate(ConnectedUPSItemPrefab, ConnectedToUPSScroll.content);
            ConnectedUPSItem connectedStatItem = connectedItem.GetComponent<ConnectedUPSItem>();

            // Set the connected item values (e.g., type name and count)
            connectedStatItem.SetValues(deviceType.ToString(), count);
        }
        //UPS con end

        // Cable count and connectors
        Dictionary<int, float> cableLengths = ConnectionsManager.Instance.GetTotalCableLengthsByType();
        float ethernetCableLength = cableLengths.ContainsKey(CableType.Ethernet) ? cableLengths[CableType.Ethernet] : 0f;
        float upsCableLength = cableLengths.ContainsKey(CableType.UPS) ? cableLengths[CableType.UPS] : 0f;
        EthernetCableLengthText.text = $"Длина Ethernet кабеля: {ethernetCableLength:F1} м";
        UPSCableLengthText.text = $"Длина UPS кабеля: {upsCableLength:F1} м";

        // Calculate number of Ethernet cables and connectors
        int ethernetCableCount = ConnectionsManager.Instance.CountEthernetCables();
        int connectorCount = ethernetCableCount * 2; // 2 connectors per cable
        float connectorPrice = 500f; // Price per connector
        float totalConnectorPrice = connectorCount * connectorPrice;

        // Add connector information to statistics
        if (connectorCount != 0)
        {
            GameObject connectorItem = Instantiate(StatisticsItemPrefab, StatisticsScroll.content);
            StatisticsItem connectorStatItem = connectorItem.GetComponent<StatisticsItem>();
            connectorStatItem.SetValues("Connector", connectorCount, totalConnectorPrice);
        }
        // Update total price and amount
        TotalPrice += totalConnectorPrice;
        TotalPriceText.text = $"Общая стоимость: {TotalPrice:F2}Р";
        TotalAmountText.text = $"Количество устройств: {TotalAmount}";
    }

    public bool IsConnectedToUPSIndirectly(InteractiveObject obj)
    {
        HashSet<InteractiveObject> visited = new HashSet<InteractiveObject>();
        Queue<InteractiveObject> toVisit = new Queue<InteractiveObject>();
        toVisit.Enqueue(obj);

        while (toVisit.Count > 0)
        {
            InteractiveObject current = toVisit.Dequeue();

            //if battery return true
            if (current.type == ObjectType.Battery)
                return true;

            // Skip if already visited
            if (visited.Contains(current))
                continue;

            visited.Add(current);

            // Get all connections for the current object
            List<Connection> connections = ConnectionsManager.Instance.GetAllConnections(current);

            foreach (var connection in connections)
            {
                InteractiveObject otherObject = connection.ObjectA == current ? connection.ObjectB : connection.ObjectA;

                // Check if the other object is a UPS
                if (otherObject.type == ObjectType.UPS)
                {
                    return true;
                }

                // If the other object is a switch, add it to the queue for further exploration
                if (otherObject.type == ObjectType.Switch && !visited.Contains(otherObject))
                {
                    toVisit.Enqueue(otherObject);
                }

                // If the other object is neither a UPS nor a Switch but hasn't been visited, continue exploring
                if (!visited.Contains(otherObject))
                {
                    toVisit.Enqueue(otherObject);
                }
            }
        }

        // If no connection to a UPS is found
        return false;
    }
}
