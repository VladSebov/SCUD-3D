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

    public void AddObject(CatalogItemData objectData, GameObject gameObject, Collider collider)
    {
        RoomMetadata roomMetadata = collider.GetComponent<RoomMetadata>();
        if (roomMetadata == null)
        {
            Debug.LogError($"no roomMetadata found for {collider.name}");
            //TODO() remove later, cause RoomMetadata should be added to all enviroment
            roomMetadata = new RoomMetadata();
            roomMetadata.FloorNumber = 1;
            roomMetadata.RoomNumber = 1;//Default values
        }

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

        // Parse mount tags
        List<MountTag> mountTags = new List<MountTag>();
        foreach (var mountTagString in objectData.mountTags)
        {
            if (Enum.TryParse(mountTagString.ToString(), out MountTag parsedMountTag))
            {
                mountTags.Add(parsedMountTag);
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
            case ObjectType.AccessController:
                newObject = gameObject.AddComponent<AccessController>();
                break;
            case ObjectType.NVR:
                newObject = gameObject.AddComponent<NVR>();
                ((NVR)newObject).maxChannels = objectData.maxChannels;
                break;
            case ObjectType.UPS:
                newObject = gameObject.AddComponent<UPS>();
                ((UPS)newObject).maxBatteries = objectData.maxBatteries;
                ((UPS)newObject).connectedBatteries = new List<string>();
                ((UPS)newObject).connectedDevices = new List<string>();
                break;
            case ObjectType.Battery:
                newObject = gameObject.AddComponent<Battery>();
                ((Battery)newObject).powerWatts = objectData.powerWatts;
                break;
            case ObjectType.ServerRack:
                newObject = gameObject.AddComponent<ServerRack>();
                ((ServerRack)newObject).maxPlacedDevices = objectData.maxPlacedDevices;
                ((ServerRack)newObject).placedDevices = new List<string>();
                break;
            case ObjectType.ServerBox:
                newObject = gameObject.AddComponent<ServerBox>();
                ((ServerBox)newObject).maxPlacedDevices = objectData.maxPlacedDevices;
                ((ServerBox)newObject).placedDevices = new List<string>();
                break;
            case ObjectType.DoorLock:
                newObject = gameObject.AddComponent<DoorLock>();
                break;
            case ObjectType.WallDoor:
                newObject = gameObject.AddComponent<DoorLock>();
                break;
        }

        // Set up the InteractiveObject
        newObject.id = IDManager.GenerateId(type);
        newObject.type = type;
        newObject.maxConnections = objectData.maxConnections;
        newObject.connectableTypes = connectableTypes;
        newObject.mountTags = mountTags;
        newObject.powerConsumption = objectData.powerConsumption;
        newObject.connectionPoint = newObject.gameObject.transform.Find("ConnectionPoint");
        newObject.roomMetadata = roomMetadata;
        newObject.price = objectData.price;

        // if connecting battery to UPS 
        if (type == ObjectType.Battery)
        {
            UPS parentUPS = collider.GetComponent<UPS>();
            parentUPS.connectedBatteries.Add(newObject.id);
            newObject.gameObject.SetActive(false); // hide to show that battery installed in UPS
        }

        //if connecting to server rack
        if (collider.GetComponent<ServerRack>() != null || collider.GetComponent<ServerBox>() != null)
        {
            if (objectData.type == ObjectType.Switch.ToString() || objectData.type == ObjectType.NVR.ToString())
            {
                if (collider.GetComponent<ServerRack>() != null)
                {
                    ServerRack parentServerRack = collider.GetComponent<ServerRack>();
                    if (parentServerRack.HasAvailablePlace())
                    {
                        parentServerRack.placedDevices.Add(newObject.id);
                        Debug.Log(parentServerRack.placedDevices.Count);
                    }
                }

                if (collider.GetComponent<ServerBox>() != null && !(objectData.type.ToString() == ObjectType.NVR.ToString()))
                {
                    ServerBox parentServerBox = collider.GetComponent<ServerBox>();
                    if (parentServerBox.HasAvailablePlace() && collider.GetComponent<ServerBox>() != null)
                    {
                        parentServerBox.placedDevices.Add(newObject.id);
                    }
                }
            }
        }
        // Add to the dictionary
        if (!gameObjects.ContainsKey(newObject.id))
        {
            gameObjects[newObject.id] = newObject;
            gameObject.name = newObject.id; // Assign the ID as the name for easy debugging
        }
    }


    public void RemoveObject(string id)
    {
        if (gameObjects.ContainsKey(id))
        {
            GameObject gameObject = GetObject(id).gameObject;
            //Remove object connections
            List<Connection> objectConnections = ConnectionsManager.Instance.GetEthernetConnections(gameObjects[id]);
            foreach (Connection connection in objectConnections)
            {
                ConnectionsManager.Instance.RemoveConnection(connection);
            }
            // Remove the object from the dictionary
            gameObjects.Remove(id);

            // Destroy the GameObject in the scene
            Destroy(gameObject);
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
            totalPrice += obj.price; // Sum the price of each object

        }

        return totalPrice; // Return the total price
    }

    public int GetTotalAmount()
    {
        int totalAmount = 0;

        foreach (var obj in gameObjects.Values)
        {
            totalAmount += 1;
        }
        return totalAmount;

    }

    public List<string> GetObjectsByType(ObjectType type)
    {
        return GetAllObjects()
           .Where(io => io.type == type)
           .Select(io => io.id)
           .ToList();
    }
    public List<InteractiveObject> GetObjectsByTypeExt(ObjectType type)
    {
        return GetAllObjects()
           .Where(io => io.type == type)
           .ToList();
    }

    public List<InteractiveObject> GetConnectedCameras()
    {
        var connectedCameras = new HashSet<InteractiveObject>(); // Using HashSet to avoid duplicates

        // Get all NVRs
        var nvrs = GetObjectsByTypeExt(ObjectType.NVR);

        foreach (var nvr in nvrs)
        {
            // Get switches connected to this NVR
            var connectedSwitches = ConnectionsManager.Instance.GetConnectedObjectsByType(nvr, ObjectType.Switch);

            // For each switch, get connected cameras
            foreach (var switch_ in connectedSwitches)
            {
                var camerasConnectedToSwitch = ConnectionsManager.Instance.GetConnectedObjectsByType(switch_, ObjectType.Camera);

                foreach (var camera in camerasConnectedToSwitch)
                {
                    connectedCameras.Add(camera);
                }
            }
        }

        // Convert to list and sort by name/id
        return connectedCameras
            .OrderBy(camera => camera.id)
            .ToList();
    }
}

