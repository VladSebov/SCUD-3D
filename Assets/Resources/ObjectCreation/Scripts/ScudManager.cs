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

    public GameObject createUserPanel;
    
    public TextMeshProUGUI createUserPanelHeader;
    //public Image createUserImage;
    public TMP_InputField userNameInputField; // Поле ввода названия роли
    public TMP_InputField PasswordInputField;

    [Header("Access Type Toggles")]
    public Toggle cardAccessToggle;
    public Toggle passwordAccessToggle;
    public Toggle fingerAccessToggle;
    public Button addRoleButton; // Кнопка для добавления роли
    public Button CreateUserButton;
    public Button CancelButton;

    public ScudSettings ScudSettings;

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
        ScudSettings = GetComponent<ScudSettings>();
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
        PlayerManager.Instance.SetUser(adminUser);
    }

    public void HideCreateUserPanel()
    {
        createUserPanel.SetActive(false);
    }

    public void FillUserForm(User user)
    {
        // Заполняем поля имени и пароля
        if (userNameInputField != null)
            userNameInputField.text = user.username;

        if (PasswordInputField != null)
            PasswordInputField.text = user.password;

        // Устанавливаем значения Toggle в соответствии с chosenAccessTypes
        if (cardAccessToggle != null)
            cardAccessToggle.isOn = user.chosenAccessTypes.Contains(AccessType.Card);

        if (passwordAccessToggle != null)
        {
            passwordAccessToggle.isOn = user.chosenAccessTypes.Contains(AccessType.Password);
            // Активируем поле пароля если нужно
            if (passwordAccessToggle.isOn && !string.IsNullOrEmpty(user.password))
            {
                PasswordInputField.gameObject.SetActive(true);
                PasswordInputField.text = user.password;
            }
            else
            {
                PasswordInputField.text = "";
            }
        }

        if (fingerAccessToggle != null)
            fingerAccessToggle.isOn = user.chosenAccessTypes.Contains(AccessType.Finger);

        // Показываем панель
        if (createUserPanel != null)
            createUserPanel.SetActive(true);

        // Фокусируемся на поле имени
        if (userNameInputField != null)
        {
            userNameInputField.Select();
            userNameInputField.ActivateInputField();
        }
    }

    /* // Метод для отмены ввода
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
    } */



    public List<User> GetUsers()
    {
        return new List<User>(users); // Возвращаем копию списка
    }

    private List<AccessType> GetSelectedAccessTypes()
    {
        List<AccessType> selectedTypes = new List<AccessType>();

        if (cardAccessToggle != null && cardAccessToggle.isOn)
            selectedTypes.Add(AccessType.Card);

        if (passwordAccessToggle != null && passwordAccessToggle.isOn)
            selectedTypes.Add(AccessType.Password);

        if (fingerAccessToggle != null && fingerAccessToggle.isOn)
            selectedTypes.Add(AccessType.Finger);

        return selectedTypes;
    }

    public void ConfirmAddUser()
    {
        string username = userNameInputField.text;
        string password = passwordAccessToggle.isOn ? PasswordInputField.text : ""; // Пароль только если выбран соответствующий тип доступа

        List<AccessType> accessTypes = GetSelectedAccessTypes();

        // Проверяем, выбран ли хотя бы один тип доступа
        if (accessTypes.Count == 0)
        {
            MessageManager.Instance.ShowMessage("Выберите хотя бы один тип доступа");
            return;
        }

        // Добавляем пользователя
        if (AddUser(username, password, accessTypes))
        {
            ResetUserForm();
            HideCreateUserPanel();
            ScudSettings.FillUsers();
        }
    }

    public void ResetUserForm()
    {
        userNameInputField.text = "";
        PasswordInputField.text = "";
        if (cardAccessToggle != null) cardAccessToggle.isOn = false;
        if (passwordAccessToggle != null) passwordAccessToggle.isOn = false;
        if (fingerAccessToggle != null) fingerAccessToggle.isOn = false;
        createUserPanel.SetActive(false);
    }


    public bool AddUser(string username, string password, List<AccessType> accessTypes = null, int imageId = 0)
    {
        // Проверка данных пользователя
        if (!CheckUserDataCorrect(username, password))
        {
            return false;
        }

        // Генерация уникального ID
        int newId = 1;
        if (users.Count > 0)
        {
            newId = users.Max(u => u.id) + 1;
        }

        // Создание и добавление пользователя
        User newUser = new User
        {
            id = newId,
            username = username,
            password = password,
            chosenAccessTypes = accessTypes ?? new List<AccessType>(),
            image = imageId
        };

        users.Add(newUser);
        PlayerManager.Instance.SetUser(newUser);
        Debug.Log($"Добавлен новый пользователь: {username} (ID: {newId})");
        return true;
    }

    private bool CheckUserDataCorrect(string username, string password)
    {
        // Проверка обязательных полей
        if (string.IsNullOrEmpty(username))
        {
            MessageManager.Instance.ShowMessage("Имя пользователя не может быть пустым");
            return false;
        }


        // Проверка уникальности имени
        if (users.Exists(u => u.username == username))
        {
            MessageManager.Instance.ShowMessage("Пользователь с таким именем уже существует");
            return false;
        }


        return true;
    }

    public void ActivatePasswordField()
    {
        if (PasswordInputField != null && passwordAccessToggle.isOn == true)
        {
            PasswordInputField.interactable = true;
        }
        else if (PasswordInputField != null && passwordAccessToggle.isOn == false)
        {
            PasswordInputField.interactable = false;
        }
        else
        {
            Debug.LogError("PasswordInputField не назначен в инспекторе!");
        }
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
