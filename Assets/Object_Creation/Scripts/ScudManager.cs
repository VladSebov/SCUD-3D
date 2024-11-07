using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ScudManager : MonoBehaviour
{
    private static ScudManager _instance;

    public static ScudManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Create a new GameObject to hold the manager if it doesn't exist
                GameObject managerObject = new GameObject("ScudManager");
                _instance = managerObject.AddComponent<ScudManager>();
            }
            return _instance;
        }
    }


    private List<string> roles = new List<string>();

    private void Awake()
    {
        // Ensure that there is only one instance of the manager
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keep the manager across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    // Метод для добавления роли
    public void AddRole(string role)
    {
        if (!roles.Contains(role))
        {
            roles.Add(role);
            Debug.Log($"Role '{role}' added.");
        }
        else
        {
            Debug.Log($"Role '{role}' already exists.");
        }
    }

    public void RemoveRole(string role)
    {

        if (roles.Contains(role))
        {
            // Remove the object from the dictionary
            roles.Remove(role);
        }
        else
        {
            Debug.LogWarning($"Role {role} does not exist in the manager.");
        }
    }

    // Метод для получения всех ролей
    public List<string> GetRoles()
    {
        return new List<string>(roles); // Возвращаем копию списка ролей
    }

    // Метод для обновления допустимых ролей для Turnstile
    public void UpdateTurnstileRoles(string interactiveObjectId, List<string> roles)
    {
        InteractiveObject obj = ObjectManager.Instance.GetObject(interactiveObjectId);
        if (obj is Turnstile turnstile)
            {
                turnstile.allowedRoles = roles;
                Debug.Log($"Updated allowed roles for Turnstile with ID {interactiveObjectId}.");
            }
    }
}

