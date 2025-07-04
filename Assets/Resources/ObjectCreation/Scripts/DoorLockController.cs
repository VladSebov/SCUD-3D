using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DoorLockController : MonoBehaviour
{
    private HingeJoint DoorHingeJoint;
    public Rigidbody DoorRigidbody;
    private static string CurrentDoor;
    public int doorStatus = 1; // 0 = open; 1 = closed;
    public static bool IsInTrigger = false;
    public static GameObject LockOnWall;

    // Добавленные переменные для проверки взгляда
    private Camera playerCamera;
    private bool wasLookingAtDoor = false;

    void Start()
    {
        Debug.Log(this.gameObject.name);
        // Получаем компонент HingeJoint
        DoorRigidbody = GetComponentInChildren<Rigidbody>();
        DoorHingeJoint = GetComponentInChildren<HingeJoint>();

        if (DoorHingeJoint == null)
        {
            Debug.LogWarning("DoorHingeJoint не найден на объекте: " + gameObject.name);
        }
        if (DoorRigidbody == null)
        {
            Debug.LogWarning("DoorRigidBody не найден на объекте: " + gameObject.name);
        }

        // Получаем камеру игрока
        playerCamera = Camera.main;
    }

    void Update()
    {
        if (IsInTrigger)
        {
            bool isLookingNow = IsPlayerLookingAtDoor();

            if (isLookingNow && !wasLookingAtDoor)
            {
                ShowDoorMessage();
            }
            else if (!isLookingNow && wasLookingAtDoor)
            {
                HideDoorMessages();
            }

            wasLookingAtDoor = isLookingNow;
        }
    }

    void OnTriggerEnter(Collider PlayerCollider)
    {
        if (PlayerCollider.CompareTag("Player"))
        {

            CurrentDoor = gameObject.name;
            IsInTrigger = true;
            wasLookingAtDoor = IsPlayerLookingAtDoor();
            if (wasLookingAtDoor)
            {
                ShowDoorMessage();
            }
        }
    }

    private void OnTriggerStay(Collider PlayerCollider)
    {
        if (PlayerCollider.CompareTag("Player"))
        {
            if (LockOnWall != null && Input.GetKeyDown(KeyCode.E) && IsPlayerLookingAtDoor())
            {
                if (CheckCorrectConnection() == true)
                {
                    MessageManager.Instance.ShowEnterPanel();
                    MessageManager.Instance.EnterPanelCardButton.onClick.RemoveAllListeners();
                    MessageManager.Instance.EnterPanelCardButton.onClick.AddListener(() => CheckEnterMethod(0));
                    MessageManager.Instance.EnterPanelFingerButton.onClick.RemoveAllListeners();
                    MessageManager.Instance.EnterPanelFingerButton.onClick.AddListener(() => CheckEnterMethod(1));
                    MessageManager.Instance.EnterPanelPasswordButton.onClick.RemoveAllListeners();
                    MessageManager.Instance.EnterPanelPasswordButton.onClick.AddListener(() => CheckEnterMethod(2));
                }
            }
        }
    }

    private void OnTriggerExit(Collider PlayerCollider)
    {
        if (PlayerCollider.CompareTag("Player"))
        {
            IsInTrigger = false;
            wasLookingAtDoor = false;
            HideDoorMessages();
            DisableDoor();
        }
    }

    private bool IsPlayerLookingAtDoor()
    {
        if (playerCamera == null) return false;

        // Дополнительная проверка с Raycast для точности
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 150f) && hit.collider.gameObject.name == "Interior_Door")
        {
            return true;
        }
        else return false;

    }

    private void ShowDoorMessage()
    {
        if (LockOnWall != null && doorStatus == 1)
        {
            MessageManager.Instance.ShowHint("Для взаимодействия с электронным замком нажмите E.");
        }
        else if (LockOnWall != null && doorStatus == 0)
        {
            EnableDoor();
            MessageManager.Instance.ShowMessage("Дверь открыта");
        }
        else if (LockOnWall == null)
        {
            EnableDoor();
            MessageManager.Instance.ShowMessage("Для двери не установлен электронный замок. Дверь открыта");
        }
    }

    private void HideDoorMessages()
    {
        MessageManager.Instance.HideHint();
    }

    public void EnableDoor()
    {
        if (gameObject.name == CurrentDoor && DoorRigidbody != null)
        {
            DoorRigidbody.isKinematic = false;
            DoorHingeJoint.useLimits = false;
            Debug.Log("useLimits выключен у объекта: " + gameObject.name);
        }
    }

    public void DisableDoor()
    {
        if (gameObject.name == CurrentDoor && DoorRigidbody != null && !DoorRigidbody.isKinematic)
        {
            DoorHingeJoint.useLimits = true;
            Debug.Log("useLimits включен у объекта: " + gameObject.name);
        }
    }

    public void ChangeDoorStatusToOpen()
    {
        doorStatus = 0;
    }

    public void ChangeDoorStatusToClosed()
    {
        doorStatus = 1;
    }

    public GameObject GetDoorLock()
    {
        return LockOnWall;
    }

    public void SetDoorLock(GameObject DoorLock)
    {
        LockOnWall = DoorLock;
    }

    // 0 = card, 1 = finger, 2 = password
    public bool CheckEnterMethod(int method)
    {
        string device = LockOnWall.GetComponent<DoorLock>().id;
        string user = PlayerManager.Instance.GetUser().username;
        string date = ScudManager.Instance.russianDaysShort[(int)DateTime.Now.DayOfWeek]+", "+DateTime.Now.ToString("HH:mm");
        string action = "";
        bool status = false;
        bool deviceCorrect = false;
        bool userCorrect = false;
        bool scheduleCorrect = false;
        foreach (var accessGroup in ScudManager.Instance.GetAccessGroups())
        {
            //поиск дверного замка в группе доступа
            if (accessGroup.chosenDevices.Contains(device))
            {
                deviceCorrect = true;
                Debug.Log("accessGroup contains device");
            }
            //поиск пользователя в группе доступа
            if (accessGroup.chosenUsers.Contains(user))
            {
                userCorrect = true;
                Debug.Log("accessGroup contains user");
            }
            foreach (var io in accessGroup.chosenSchedules)
            {
                int.TryParse(io.startTime.Substring(0, 2), out int startTime);
                int.TryParse(DateTime.Now.ToString("HH"), out int nowTime);
                int.TryParse(io.endTime.Substring(0, 2), out int endTime);
                Debug.Log("starttime: " + startTime + " nowtime: " + nowTime + " endtime: " + endTime);
                Debug.Log("russianDaysIndex:" + Array.IndexOf(ScudManager.Instance.russianDays, io.day));
                Debug.Log("DayOfWeek:" + (int)DateTime.Now.DayOfWeek);
            }
            // проверка расписания в группе доступа
            if (accessGroup.chosenSchedules.Any(io =>
                int.TryParse(io.startTime.Substring(0, 2), out int startTime) &&
                int.TryParse(DateTime.Now.ToString("HH"), out int nowTime) &&
                int.TryParse(io.endTime.Substring(0, 2), out int endTime) &&
                startTime <= nowTime && endTime > nowTime && Array.IndexOf(ScudManager.Instance.russianDays, io.day) == (int)DateTime.Now.DayOfWeek))
            {
                scheduleCorrect = true;
                Debug.Log("accessGroup contains schedule");
            }
            if (deviceCorrect && userCorrect && scheduleCorrect)
            {
                status = true;
                break;
            }
        }
        if (PlayerManager.Instance.GetUser().chosenAccessTypes.Any(io => (int)io == method) == false || status == false)
        {
            status = false;
            if (method == 0) action = "Неизвестная карта";
            if (method == 1) action = "Неизвестный отпечаток";
            if (method == 2) action = "Неизвестный пароль";
            user = "-";
            Debug.Log("user's accessType isn't correct");
        }
        if (status == true && method == 2 && PlayerManager.Instance.GetUser().password != MessageManager.Instance.GetPasswordFromEnterPanel())
        {
            status = false;
            action = "Неверный пароль";
            Debug.Log("user's password isn't correct");
        }
        if (status == false)
        {
            MessageManager.Instance.ShowMessage("Доступ запрещен");
        }
        if (status == true)
        {
            MessageManager.Instance.ShowMessage("Доступ разрешен");
            EnableDoor();
            if (method == 0) action = "Вход по карте";
            if (method == 1) action = "Вход по отпечатку";
            if (method == 2) action = "Вход по па ролю";
        }
        ScudManager.Instance.AddLogItem(device, user, date, action);
        MessageManager.Instance.HideEnterPanel();
        return status;
    }

    public bool CheckCorrectConnection()
    {
        bool status = false;
        InteractiveObject obj = ObjectManager.Instance.GetObject(LockOnWall.name);
        Connection connection = ConnectionsManager.Instance.GetAllConnections(obj).FirstOrDefault();

        if (connection != null)
        {
            InteractiveObject controller = connection.ObjectA == obj ? connection.ObjectB : connection.ObjectA;
            if (controller is AccessController accessController && ConnectionsManager.Instance.GetConnectedObjectsByType(accessController, ObjectType.Computer).Any())
            {
                status = true;
            }
        }
        else
        {
            MessageManager.Instance.ShowMessage("Устройство не подключено к Контролеру");
        }
        return status;
    }
}