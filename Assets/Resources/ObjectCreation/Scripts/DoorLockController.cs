using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DoorLockController : MonoBehaviour
{
    private HingeJoint DoorHingeJoint;
    public Rigidbody DoorRigidbody;
    private static string CurrentDoor;
    private int status = 0;
    public static bool IsInTrigger = false;
    public GameObject LockOnWall;

    // Добавленные переменные для проверки взгляда
    private Camera playerCamera;
    private bool wasLookingAtDoor = false;

    void Start()
    {
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
            Debug.Log("finding bug 00");
            Debug.Log(playerCamera.gameObject.name);
            CurrentDoor = gameObject.name;
            IsInTrigger = true;
            wasLookingAtDoor = IsPlayerLookingAtDoor();

            if (wasLookingAtDoor)
            {
                Debug.Log("finding bug 01");
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
                MessageManager.Instance.ShowEnterPanel();
            }
            else if (LockOnWall == null && IsPlayerLookingAtDoor())
            {
                EnableDoor();
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
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 150f) && hit.collider.gameObject.name == "Interior_Door_Frame")
        {
            return true;
        }
        else return false;
        
    }

    private void ShowDoorMessage()
    {
        if (LockOnWall != null)
        {
            MessageManager.Instance.ShowHint("Для взаимодействия с электронным замком нажмите E.");
        }
        else
        {
            EnableDoor();
            MessageManager.Instance.ShowMessage("Для двери не установлен электронный замок");
        }
    }

    private void HideDoorMessages()
    {
        MessageManager.Instance.HideHint();
    }

    public void hideEnterPanel1()
    {
        MessageManager.Instance.ShowMessage("Доступ запрещен");
        DisableDoor();
        MessageManager.Instance.HideEnterPanel();
    }

    public void hideEnterPanel2() 
    {
        MessageManager.Instance.ShowMessage("Доступ разрешен");
        EnableDoor();
    }

    void EnableDoor()
    {
        if (gameObject.name == CurrentDoor && DoorRigidbody != null)
        {
            DoorRigidbody.isKinematic = false;
            DoorHingeJoint.useLimits = false;
            Debug.Log("useLimits выключен у объекта: " + gameObject.name);
        }
    }

    void DisableDoor()
    {
        if (gameObject.name == CurrentDoor && DoorRigidbody != null && !DoorRigidbody.isKinematic)
        {
            DoorHingeJoint.useLimits = true;
            Debug.Log("useLimits включен у объекта: " + gameObject.name);
        }
    }

    void CheckRoleEnabled()
    {
        InteractiveObject obj = ObjectManager.Instance.GetObject(LockOnWall.name);
        Connection connection = ConnectionsManager.Instance.GetAllConnections(obj).FirstOrDefault();
        
        if (connection != null)
        {
            InteractiveObject controller = connection.ObjectA == obj ? connection.ObjectB : connection.ObjectA;
            if (controller is AccessController accessController)
            {
                if (LockOnWall != null && status == 1)
                {
                    MessageManager.Instance.ShowMessage("Доступ запрещен");
                    DisableDoor();
                }
                if (LockOnWall != null && status == 2)
                {
                    MessageManager.Instance.ShowMessage("Доступ разрешен");
                    EnableDoor();
                }
            }
        }
        else
        {
            MessageManager.Instance.ShowMessage("Устройство не подключено к Контролеру");
            DisableDoor();
        }
    }
}