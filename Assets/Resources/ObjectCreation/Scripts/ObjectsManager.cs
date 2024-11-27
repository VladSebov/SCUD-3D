using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public void AddObject(CatalogItemData objectData, GameObject gameObject)
    {
        InteractiveObject newObject = null;

        // Parse the object type
        Enum.TryParse(objectData.type, true, out ObjectType type);

        // Parse connectable types
        List<ObjectType> connectableTypes = new List<ObjectType>();
        foreach (var typeString in objectData.connectableTypes)
        {
            if (Enum.TryParse(typeString.ToString(), out ObjectType parsedConnectableType))
            {
                connectableTypes.Add(parsedConnectableType);
            }
        }

        // Attach the appropriate InteractiveObject subclass
        switch (type)
        {
            case ObjectType.Camera:
                newObject = gameObject.AddComponent<MyCamera>();
                break;
            case ObjectType.Switch:
                newObject = gameObject.AddComponent<Switch>();
                break;
            case ObjectType.Turnstile:
                newObject = gameObject.AddComponent<Turnstile>();
                break;
            case ObjectType.Server:
                newObject = gameObject.AddComponent<Server>();
                break;
            case ObjectType.ControlBox:
                newObject = gameObject.AddComponent<ControlBox>();
                break;
            case ObjectType.Terminal:
                newObject = gameObject.AddComponent<Terminal>();
                break;
            case ObjectType.Reciever:
                newObject = gameObject.AddComponent<Reciever>();
                break;
        }

        if (newObject != null)
        {
            // Set up the InteractiveObject
            newObject.id = IDManager.GenerateId(type);
            newObject.type = type;
            newObject.maxConnections = objectData.maxConnections;
            newObject.connectableTypes = connectableTypes;
            newObject.connectionPoint = newObject.gameObject.transform.Find("ConnectionPoint");

            // Add to the dictionary
            if (!gameObjects.ContainsKey(newObject.id))
            {
                gameObjects[newObject.id] = newObject;
                gameObject.name = newObject.id; // Assign the ID as the name for easy debugging
            }
        }
    }


    public void RemoveObject(GameObject obj)
    {
        // Check if the GameObject has a name that corresponds to an ID in the dictionary
        string id = obj.name; // Assuming the GameObject's name is the same as its ID

        if (gameObjects.ContainsKey(id))
        {
            //Remove object connections
            List<Connection> objectConnections = ConnectionsManager.Instance.GetConnections(gameObjects[id]);
            foreach (Connection connection in objectConnections){
                ConnectionsManager.Instance.RemoveConnection(connection);
            }
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
                    bool objectsAlreadyConnected = ConnectionsManager.Instance.HasConnection(currentObject, obj);
                    if (!objectsAlreadyConnected)
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

    public int GetObjectsCountByType(ObjectType type)
    {
        return GetAllObjects()
           .Where(io => io.type == type)
           .ToList()
           .Count;
    }
}

