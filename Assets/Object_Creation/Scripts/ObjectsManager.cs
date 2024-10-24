using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ObjectManager : MonoBehaviour
{
    private static ObjectManager _instance;

    public static ObjectManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Create a new GameObject to hold the manager if it doesn't exist
                GameObject managerObject = new GameObject("ObjectManager");
                _instance = managerObject.AddComponent<ObjectManager>();
            }
            return _instance;
        }
    }


    private Dictionary<string, InteractiveObject> gameObjects = new Dictionary<string, InteractiveObject>();

    private Dictionary<ObjectType, int> idCounters = new Dictionary<ObjectType, int>();

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

    // Constructor or initialization
    private void Start()
    {
        // Initialize counters for each object type
        foreach (ObjectType type in Enum.GetValues(typeof(ObjectType)))
        {
            idCounters[type] = 0;
        }
    }

    public string GenerateId(ObjectType objectType)
    {
        // Generate a new ID based on the current count and increment the counter
        string newId = $"{objectType.ToString().ToLower()}{idCounters[objectType]}";
        idCounters[objectType]++;
        return newId;
    }

    // Example of how to create an object with generated ID
    public string AddObject(ObjectType objectType, GameObject gameObject)
    {
        InteractiveObject newObject = null;

        switch (objectType)
        {
            case ObjectType.Camera:
                newObject = new MyCamera();
                break;
            case ObjectType.Switch:
                newObject = new Switch();
                break;
            case ObjectType.Turnstile:
                newObject = new Turnstile();
                break;
        }

        if (newObject != null)
        {
            newObject.id = GenerateId(objectType);
            if (!gameObjects.ContainsKey(newObject.id))
            {
                newObject.gameObject = gameObject;
                gameObjects[newObject.id] = newObject;
            }
        }
        return newObject?.id; // return id to use it as name in prefab
    }

    public void RemoveObject(GameObject obj)
    {
        // Check if the GameObject has a name that corresponds to an ID in the dictionary
        string id = obj.name; // Assuming the GameObject's name is the same as its ID

        if (gameObjects.ContainsKey(id))
        {
            // Remove the object from the dictionary
            gameObjects.Remove(id);

            // Destroy the GameObject in the scene
            Destroy(obj);
        }
        else
        {
            Debug.LogWarning($"Object with ID {id} does not exist in the manager.");
        }
    }

    public void ConnectObjects(string id1, string id2)
    {
        if (gameObjects.ContainsKey(id1) && gameObjects.ContainsKey(id2))
        {
            gameObjects[id1].connections.Add(id2);
            gameObjects[id2].connections.Add(id1); // Assuming bidirectional connection
        }
    }

    public InteractiveObject GetObject(string id)
    {
        gameObjects.TryGetValue(id, out InteractiveObject obj);
        return obj;
    }

    public List<InteractiveObject> GetAllObjects()
    {
        return new List<InteractiveObject>(gameObjects.Values);
    }
}

