using System.Collections;
using System.Collections.Generic;
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
    }
    void OnTriggerEnter(Collider PlayerCollider)
    {
        // Проверяем, какой объект вошел в триггер
        if (PlayerCollider.CompareTag("Player")) // Предположим, что у объекта есть тег "Player"
        {
            CurrentTurnstile = gameObject.name;
            IsInTrigger = true;
            Debug.Log("Нажмите E, чтобы приложить карту к устройству: " + CurrentTurnstile);
        }

    }

    private void OnTriggerStay(Collider PlayerCollider)
    {
        // Здесь можно выполнять действия, пока объект находится в триггере
        //Debug.Log("Объект " + PlayerCollider.gameObject.name + " находится в триггере.");
        if (PlayerCollider.CompareTag("Player")) // Предположим, что у объекта есть тег "Player"
        {
            if (Input.GetKeyDown(KeyCode.E))
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
            //DisableTurnstile();
        }

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
            TurnstileRigidbody.isKinematic = true;
            Debug.Log("isKinematic включен у объекта: " + gameObject.name);
        }
    }

    void CheckRoleEnabled()
    {   
        InteractiveObject obj = ObjectManager.Instance.GetObject(gameObject.name);
        Connection connection = ConnectionsManager.Instance.GetAllConnections(obj).FirstOrDefault();
        if (connection != null)
        {
            InteractiveObject controller = connection.ObjectA == obj ? connection.ObjectB : connection.ObjectA;
            if (controller is AccessController accessController)
            {
                if (accessController.allowedRoles.First() == PlayerManager.Instance.GetRole()) {
                    Debug.Log("Можно проходить");
                    EnableTurnstile();
                }
                else if (accessController.allowedRoles.First() != PlayerManager.Instance.GetRole()) {
                    Debug.Log("Нет доступа для пользователя: " + PlayerManager.Instance.GetRole());
                    DisableTurnstile();
                }
            }
        }
        else if (connection == null)
        {
            Debug.Log("Устройство не подключено к Контролеру");
            DisableTurnstile();
        }
    }



    void Update()
    {
        if (TurnstileRigidbody != null && TurnstileRigidbody.isKinematic == false && math.abs(TurnstileHingeJoint.angle) < CompareAngle && !IsInTrigger) DisableTurnstile();
        //Debug.Log(TurnstileHingeJoint.angle);
    }
}
