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
            if (LockOnWall != null)
                MessageManager.Instance.ShowHint("Для взаимодействия с электронным замком нажмите E.");
            else if (LockOnWall == null)
            {
                EnableDoor();
                MessageManager.Instance.ShowMessage("Для двери не установлен электронный замок");
            }
        }

    }

    public void hideEnterPanel1()
    {
        MessageManager.Instance.ShowMessage("Доступ запрещен");
        DisableDoor();
        MessageManager.Instance.HideEnterPanel();
    	
    }
    public void hideEnterPanel2() {
    	MessageManager.Instance.ShowMessage("Доступ разрешен");
        EnableDoor();
    }

    private void OnTriggerStay(Collider PlayerCollider)
    {
        // Здесь можно выполнять действия, пока объект находится в триггере
        //Debug.Log("Объект " + PlayerCollider.gameObject.name + " находится в триггере.");
        if (PlayerCollider.CompareTag("Player")) // Предположим, что у объекта есть тег "Player"
        {
            if (LockOnWall != null && Input.GetKeyDown(KeyCode.E))
            {
                MessageManager.Instance.ShowEnterPanel();
            
            }
            else if (LockOnWall == null)
            {
                EnableDoor();
            }
        }
    }

    private void OnTriggerExit(Collider PlayerCollider)
    {
        if (PlayerCollider.CompareTag("Player")) // Предположим, что у объекта есть тег "Player"
        {
            IsInTrigger = false;
            MessageManager.Instance.HideHint();
            DisableDoor();
        }
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
        else if (connection == null)
        {
            MessageManager.Instance.ShowMessage("Устройство не подключено к Контролеру");
            DisableDoor();
        }
    }



}
