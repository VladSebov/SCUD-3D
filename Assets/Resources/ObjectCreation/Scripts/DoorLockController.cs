using System.Linq;
using UnityEngine;

public class DoorLockController : MonoBehaviour
{
    private HingeJoint DoorHingeJoint;

    private Rigidbody DoorRigidbody;

    private static string CurrentDoor;

    public static bool IsInTrigger = false;

    public GameObject LockOnWall;

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
    }
    void OnTriggerEnter(Collider PlayerCollider)
    {
        // Проверяем, какой объект вошел в триггер
        if (PlayerCollider.CompareTag("Player")) // Предположим, что у объекта есть тег "Player"
        {
            CurrentDoor = gameObject.name;
            IsInTrigger = true;
            if (LockOnWall != null) Debug.Log("Нажмите E, чтобы приложить карту к устройству: " + CurrentDoor);
            else if (LockOnWall == null)
            {
                EnableDoor();
                Debug.Log("На двери: " + CurrentDoor + " не установлен замок");
            }
        }

    }

    private void OnTriggerStay(Collider PlayerCollider)
    {
        // Здесь можно выполнять действия, пока объект находится в триггере
        //Debug.Log("Объект " + PlayerCollider.gameObject.name + " находится в триггере.");
        if (PlayerCollider.CompareTag("Player")) // Предположим, что у объекта есть тег "Player"
        {
            if (LockOnWall != null && Input.GetKeyDown(KeyCode.E))
            {
                CheckRoleEnabled();
            }
        }
    }

    private void OnTriggerExit(Collider PlayerCollider)
    {
        if (PlayerCollider.CompareTag("Player")) // Предположим, что у объекта есть тег "Player"
        {
            IsInTrigger = false;
            DisableDoor();
        }
    }

    void EnableDoor()
    {
        if (gameObject.name == CurrentDoor && DoorRigidbody != null)
        {
            DoorHingeJoint.useLimits = false;
            Debug.Log("useLimits выключен у объекта: " + gameObject.name);
        }
    }

    void DisableDoor()
    {
        if (gameObject.name == CurrentDoor && DoorRigidbody != null && DoorRigidbody.isKinematic == false)
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
                if (PlayerManager.Instance.GetRole() == null)
                {
                    Debug.Log("Для пользователя не выбрана роль");
                    DisableDoor();
                }
                else if (accessController.allowedRoles == null)
                {
                    Debug.Log("Не выбрана роль на контролере");
                    DisableDoor();
                }
                else if (accessController.allowedRoles.FirstOrDefault() != PlayerManager.Instance.GetRole())
                {
                    Debug.Log("Нет доступа для роли: " + PlayerManager.Instance.GetRole());
                    DisableDoor();
                }
                else if (accessController.allowedRoles.FirstOrDefault() == PlayerManager.Instance.GetRole())
                {
                    Debug.Log("Можно проходить");
                    EnableDoor();
                }

            }
        }
        else if (connection == null)
        {
            Debug.Log("Устройство не подключено к Контролеру");
            DisableDoor();
        }
    }



}
