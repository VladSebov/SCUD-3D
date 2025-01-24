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
                    if (ScudManager.Instance.GetRoles().Count > restriction.value)
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
        AccessSettingsButton.interactable = true;
        RolesSettingsButton.interactable = true;
        CamerasSettingsButton.interactable = true;
        RestrictionsSettingsButton.interactable = true;
        UserSettingsButton.interactable = true;
        StatisticsButton.interactable = true;
    }

    private void ShowAccessSettingsContent()
    {
        ResetButtonColors();
        AccessSettingsButton.interactable = false;
        HideAllContent();
        AccessSettingsContent.SetActive(true);
        FillAccessControllers();
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

    private void ShowStatisticsContent()
    {
        ResetButtonColors();
        StatisticsButton.interactable = false;
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
        FillRoles();
        // Update button visibility
        DeleteRoleButton.interactable = !string.IsNullOrEmpty(selectedRole);
    }

    public void SelectRole(string Role, Button button)
    {
        selectedRole = Role;
        DeleteRoleButton.interactable = !string.IsNullOrEmpty(selectedRole);
        FillRoles();
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

    public void FillRoles()
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

        rolesHintText.gameObject.SetActive(false);

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
            button.onClick.AddListener(() => { SelectRole(role, button); });
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

        if (roles.Count == 0)
        {
            userHintText.gameObject.SetActive(true);
            return;
        }

        userHintText.gameObject.SetActive(false);

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
        TotalPriceText.text = $"Общая стоимость: {TotalPrice:F2}₽";
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
        ScudManager.Instance.ConfirmAddRole();
        FillRoles();
    }
}
