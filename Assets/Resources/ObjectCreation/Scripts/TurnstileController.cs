using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using SCUD3D;

public class TurnstileController : MonoBehaviour
{
    private HingeJoint TurnstileHingeJoint;

    private Rigidbody TurnstileRigidbody;

    private static string CurrentTurnstile;

    private float CompareAngle = 25f;

    public static bool IsInTrigger = false;

    public int doorStatus = 1; // 0 = open; 1 = closed;

    // Добавленные переменные для проверки взгляда
    private Camera playerCamera;
    private bool wasLookingAtDoor = false;

    void Start()
    {
        // Получаем компонент HingeJoint
        TurnstileRigidbody = GetComponentInChildren<Rigidbody>();
        TurnstileHingeJoint = GetComponentInChildren<HingeJoint>();
        if (TurnstileHingeJoint == null)
        {
            Debug.LogWarning("TurnstileHingeJoint не найден на объекте: " + gameObject.name);
        }
        if (TurnstileRigidbody == null)
        {
            Debug.LogWarning("TurnstileRigidBody не найден на объекте: " + gameObject.name);
        }
        playerCamera = Camera.main;
    }

    void Update()
    {
        if (TurnstileHingeJoint != null && TurnstileHingeJoint.useLimits == false && math.abs(TurnstileHingeJoint.angle) < CompareAngle && !IsInTrigger) DisableTurnstile();
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
            CurrentTurnstile = gameObject.name;
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
            if (Input.GetKeyDown(KeyCode.E) && IsPlayerLookingAtDoor())
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
            DisableTurnstile();
        }
    }

    private bool IsPlayerLookingAtDoor()
    {
        if (playerCamera == null) return false;

        // Дополнительная проверка с Raycast для точности
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 150f) && hit.collider.gameObject.name == CurrentTurnstile)
        {
            return true;
        }
        else return false;

    }

    private void ShowDoorMessage()
    {
        if (doorStatus == 1)
        {
            MessageManager.Instance.ShowHint("Для взаимодействия с турникетом нажмите E.");
        }
        else if (doorStatus == 0)
        {
            EnableTurnstile();
            MessageManager.Instance.ShowMessage("Открыто");
        }
    }

    private void HideDoorMessages()
    {
        MessageManager.Instance.HideHint();
    }

    void EnableTurnstile()
    {
        if (gameObject.name == CurrentTurnstile && TurnstileRigidbody != null)
        {
            TurnstileHingeJoint.useLimits = false;
            TurnstileRigidbody.isKinematic = false;
            Debug.Log("isKinematic выключен у объекта: " + gameObject.name);
        }
    }

    void DisableTurnstile()
    {
        if (gameObject.name == CurrentTurnstile && TurnstileRigidbody != null && TurnstileRigidbody.isKinematic == false)
        {
            TurnstileHingeJoint.useLimits = true;
            Debug.Log("isKinematic включен у объекта: " + gameObject.name);
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

    public bool CheckEnterMethod(int method)
    {
        string device = CurrentTurnstile;
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
            EnableTurnstile();
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
        InteractiveObject obj = ObjectManager.Instance.GetObject(CurrentTurnstile);
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
