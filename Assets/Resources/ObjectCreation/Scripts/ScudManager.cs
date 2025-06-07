using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using System.Linq;

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

    private List<User> users = new List<User>();

    public GameObject userInputPanel; // Панель для ввода названия роли

    public TMP_InputField userNameInputField; // Поле ввода названия роли
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
        // Создаем администратора
        User adminUser = new User
        {
            id = 0,
            username = "administrator",
            chosenAccessTypes = new List<AccessType> { AccessType.Card, AccessType.Password, AccessType.Finger },
            password = "admin",
            image = 0
        };
        users.Add(adminUser);

        // Скрываем панель при старте
        userInputPanel.SetActive(false);

        // Привязываем метод к кнопке "Добавить роль"
        addRoleButton.onClick.AddListener(ShowUserInputPanel);
        CancelButton.onClick.AddListener(CancelAddRole);
    }

    // Метод для отображения панели ввода
    public void ShowUserInputPanel()
    {
        userInputPanel.SetActive(true);
        userNameInputField.Select(); // Устанавливаем фокус на поле ввода
        userNameInputField.ActivateInputField(); // Активируем поле ввода
    }

    // Метод для отмены ввода
    public void CancelAddRole()
    {
        userNameInputField.text = ""; // Очищаем поле ввода
        userInputPanel.SetActive(false); // Скрываем панель
        userNameInputField.DeactivateInputField(); // Деактивируем поле ввода
    }

    // Метод для добавления роли
    public void ConfirmAddRole()
    {
        string roleName = userNameInputField.text;

        if (!string.IsNullOrEmpty(roleName))
        {
            AddRole(roleName); // Вызов метода добавления роли
            userNameInputField.text = ""; // Очищаем поле ввода
            userInputPanel.SetActive(false); // Скрываем панель после добавления
        }
        else
        {
            MessageManager.Instance.ShowMessage("Название роли не может быть пустым.");
        }
    }

    // Метод для добавления роли с заданным названием
    public void AddRole(string roleName)
    {
        if (!RestrictionsManager.Instance.CheckRoleAvailable())
        {
            MessageManager.Instance.ShowMessage("Достигнуто максимальное количество ролей");
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
            MessageManager.Instance.ShowMessage("Роль с таким названием уже существует.");
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

    public bool AddUser(string username, string password, List<AccessType> accessTypes = null, int imageId = 0)
    {
        // Проверка обязательных полей
        if (string.IsNullOrEmpty(username))
        {
            MessageManager.Instance.ShowMessage("Имя пользователя не может быть пустым");
            return false;
        }

        if (string.IsNullOrEmpty(password))
        {
            MessageManager.Instance.ShowMessage("Пароль не может быть пустым");
            return false;
        }

        // Проверка уникальности имени пользователя
        if (users.Exists(u => u.username == username))
        {
            MessageManager.Instance.ShowMessage("Пользователь с таким именем уже существует");
            return false;
        }

        // Генерация уникального ID
        int newId = 1;
        if (users.Count > 0)
        {
            // Находим максимальный существующий ID и добавляем 1
            newId = users.Max(u => u.id) + 1;
        }

        // Создаем нового пользователя
        User newUser = new User
        {
            id = newId,
            username = username,
            password = password,
            chosenAccessTypes = accessTypes ?? new List<AccessType>(),
            image = imageId
        };

        // Добавляем в список
        users.Add(newUser);
        Debug.Log($"Добавлен новый пользователь: {username} (ID: {newId})");
        return true;
    }

    public List<User> GetUsers()
    {
        return new List<User>(users); // Возвращаем копию списка
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

public enum AccessType
{
    Card,
    Password,
    Finger
}

[System.Serializable]
public class User
{
    public int id;
    public string username;
    public List<AccessType> chosenAccessTypes;
    public string password;

    public int image;
}