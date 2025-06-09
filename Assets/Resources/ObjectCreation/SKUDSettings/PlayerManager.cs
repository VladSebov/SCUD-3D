using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private static PlayerManager _instance;

    public static PlayerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Создаем новый GameObject для менеджера, если его нет
                GameObject managerObject = new GameObject("PlayerManager");
                _instance = managerObject.AddComponent<PlayerManager>();
            }
            return _instance;
        }
    }

    private User user; // Ссылка на объект User

    private void Awake()
    {
        // Гарантируем, что существует только один экземпляр менеджера
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // Опционально: сохраняем менеджер между сценами
        }
        else
        {
            Destroy(gameObject); // Уничтожаем дубликаты
        }
    }

    // Методы для работы с User
    public User GetUser()
    {
        return user;
    }
    
    public void SetUser(User newUser)
    {
        user = newUser;
    }
}
