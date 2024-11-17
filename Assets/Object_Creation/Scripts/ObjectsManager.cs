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

    // Example of how to create an object with generated ID
    public void AddObject(CatalogItemData objectData, GameObject gameObject)
    {
        InteractiveObject newObject = null;

        // parse device type
        Enum.TryParse(objectData.type, true, out ObjectType type);

        // parse connectable types
        List<ObjectType> connectableTypes = new List<ObjectType>();
        foreach (var typeString in objectData.connectableTypes)
        {
            if (Enum.TryParse(typeString.ToString(), out ObjectType parsedConnectableType))
            {
                connectableTypes.Add(parsedConnectableType);
            }
        }

        switch (type)
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
            case ObjectType.Server:
                newObject = new Server();
                break;
            case ObjectType.ControlBox:
                newObject = new ControlBox();
                break;
            case ObjectType.Terminal:
                newObject = new Terminal();
                break;
            case ObjectType.Reciever:
                newObject = new Reciever();
                break;
        }

        if (newObject != null)
        {
            newObject.id = IDManager.GenerateId(type);
            if (!gameObjects.ContainsKey(newObject.id))
            {
                newObject.type = type;
                newObject.maxConnections = objectData.maxConnections;
                newObject.connectableTypes = connectableTypes;
                newObject.gameObject = gameObject;
                gameObjects[newObject.id] = newObject;

                gameObject.name = newObject.id; // assigns name 
            }
        }
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
            InteractiveObject obj1 = gameObjects[id1];
            InteractiveObject obj2 = gameObjects[id2];

            // Check if obj1 is already connected to obj2 and vice versa
            if (!obj1.connections.Contains(id2) && !obj2.connections.Contains(id1))
            {
                // Ensure both objects have available connection slots
                if (obj1.connections.Count < obj1.maxConnections && obj2.connections.Count < obj2.maxConnections)
                {
                    // Add the connection to both objects (bidirectional connection)
                    obj1.connections.Add(id2);
                    obj2.connections.Add(id1);
                }
                else
                {
                    Debug.LogWarning("One or both objects have reached their connection limit.");
                }
            }
            else
            {
                Debug.LogWarning($"Objects {id1} and {id2} are already connected.");
            }
        }
        else
        {
            Debug.LogWarning($"One or both objects with IDs {id1} and {id2} do not exist.");
        }
    }
    
    public void DisconnectObjects(string id1, string id2)
    {
        if (gameObjects.ContainsKey(id1) && gameObjects.ContainsKey(id2))
        {
            InteractiveObject obj1 = gameObjects[id1];
            InteractiveObject obj2 = gameObjects[id2];

            // Check if obj1 is connected to obj2 and vice versa
            if (obj1.connections.Contains(id2) && obj2.connections.Contains(id1))
            {
                // Remove the connection from both objects (bidirectional disconnection)
                obj1.connections.Remove(id2);
                obj2.connections.Remove(id1);
            }
            else
            {
                Debug.LogWarning($"Objects {id1} and {id2} are not connected.");
            }
        }
        else
        {
            Debug.LogWarning($"One or both objects with IDs {id1} and {id2} do not exist.");
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

    public List<string> GetAvailableDevicesIDs(string currentObjectId)
    {
        // Get the current object by its ID
        InteractiveObject currentObject = GetObject(currentObjectId);
        if (currentObject == null)
        {
            Debug.LogWarning($"Object with ID {currentObjectId} does not exist.");
            return null;
        }

        // List to hold available devices to connect
        List<string> availableDevices = new List<string>();

        // Iterate over all the objects in the gameObjects dictionary
        foreach (InteractiveObject obj in gameObjects.Values)
        {
            // Check if the object is not the current object itself
            if (obj.id == currentObject.id)
                continue;

            // Check if the object's type is connectable to the current object
            if (currentObject.connectableTypes.Contains(obj.type))
            {
                // Check if the object has available connection slots
                if (0 < obj.maxConnections)
                {
                    // Check if the object is not already connected to the current object
                    if (!currentObject.connections.Contains(obj.id))
                    {
                        // If all conditions are satisfied, add the object to the availableDevices list
                        availableDevices.Add(obj.id);
                    }
                }
            }
        }

        return availableDevices;
    }

    public float GetTotalPrice()
{
    float totalPrice = 0f;

    // Iterate through all objects in the gameObjects dictionary
    foreach (var obj in gameObjects.Values)
    {
        //totalPrice += obj.price; // Sum the price of each object
    }

    return totalPrice; // Return the total price
}
}

