using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScudManager : MonoBehaviour
{
    private static ScudManager _instance;

    public static ScudManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject managerObject = new GameObject("ScudManager");
                _instance = managerObject.AddComponent<ScudManager>();
            }
            return _instance;
        }
    }

    private int roleId = 0; // Переменная для уникальных идентификаторов ролей
    private List<string> roles = new List<string>();

    public GameObject roleInputPanel; // Панель для ввода названия роли
    public TMP_InputField roleNameInputField; // Поле ввода названия роли
    public Button addRoleButton; // Кнопка для добавления роли
    public Button CancelButton;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Скрываем панель при старте
        roleInputPanel.SetActive(false);

        // Привязываем метод к кнопке "Добавить роль"
        addRoleButton.onClick.AddListener(ShowRoleInputPanel);

        CancelButton.onClick.AddListener(CancelAddRole);
    }

    // Метод для отображения панели ввода
    public void ShowRoleInputPanel()
    {
        roleInputPanel.SetActive(true);
        roleNameInputField.Select(); // Устанавливаем фокус на поле ввода
        roleNameInputField.ActivateInputField(); // Активируем поле ввода
    }
    // Метод для отмены ввода
    public void CancelAddRole()
    {
        roleNameInputField.text = ""; // Очищаем поле ввода
        roleInputPanel.SetActive(false); // Скрываем панель
        roleNameInputField.DeactivateInputField(); // Деактивируем поле ввода
    }

    // Метод для добавления роли
    public void ConfirmAddRole()
    {
        string roleName = roleNameInputField.text;

        if (!string.IsNullOrEmpty(roleName))
        {
            AddRole(roleName); // Вызов метода добавления роли
            roleNameInputField.text = ""; // Очищаем поле ввода
            roleInputPanel.SetActive(false); // Скрываем панель после добавления
        }
        else
        {
            Debug.LogWarning("Название роли не может быть пустым.");
        }
    }

    // Метод для добавления роли с заданным названием
    public void AddRole(string roleName)
    {
        if (!RestrictionsManager.Instance.CheckRoleAvailable())
        {
            Debug.Log("Достигнуто максимальное количество ролей");
            return;
        }

        if (string.IsNullOrEmpty(roleName))
        {
            roleName = $"Новая роль #{roleId++}";
        }

        if (!roles.Contains(roleName))
        {
            roles.Add(roleName);
            Debug.Log($"Role '{roleName}' added.");
        }
        else
        {
            Debug.Log($"Role '{roleName}' already exists.");
        }
    }

    public void RemoveRole(string role)
    {
        if (roles.Contains(role))
        {
            roles.Remove(role);
            Debug.Log($"Role '{role}' removed.");
        }
        else
        {
            Debug.LogWarning($"Role {role} does not exist in the manager.");
        }
    }

    public List<string> GetRoles()
    {
        return new List<string>(roles);
    }

    public void UpdateAccessControllerRoles(string interactiveObjectId, List<string> roles)
    {
        InteractiveObject obj = ObjectManager.Instance.GetObject(interactiveObjectId);
        if (obj is AccessController accessController)
        {
            accessController.allowedRoles = roles;
            Debug.Log($"Updated allowed roles for Turnstile with ID {interactiveObjectId}.");
        }
    }
}
