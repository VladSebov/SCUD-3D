

using System.Collections.Generic;
using UnityEngine;

public class ConnectionsManager : MonoBehaviour
{
    private static ConnectionsManager _instance;

    public static ConnectionsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Create a new GameObject to hold the manager if it doesn't exist
                GameObject managerObject = new GameObject("ConnectionsManager");
                _instance = managerObject.AddComponent<ConnectionsManager>();
            }
            return _instance;
        }
    }

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


    private List<Connection> connections = new List<Connection>();


    // Add a new connection
    public void AddConnection(Connection connection)
    {
        connections.Add(connection);
    }

    // Get all connections for a specific object, excluding connections containing a UPS object
    public List<Connection> GetEthernetConnections(InteractiveObject obj)
    {
        return connections.FindAll(connection =>
            connection.InvolvesObject(obj) &&
            connection.ObjectA.type != ObjectType.UPS &&
            connection.ObjectB.type != ObjectType.UPS);
    }

    public int CountEthernetCables()
    {
        int count = 0;
        foreach (var connection in connections)
        {
            if (connection.CableType == CableType.Ethernet)
            {
                count++;
            }
        }
        return count;
    }


    public List<Connection> GetAllConnections(InteractiveObject obj)
    {
        return connections.FindAll(connection =>
            connection.InvolvesObject(obj));
    }

    // Check if the specified object has a connection with another object
    public bool HasConnection(InteractiveObject obj, InteractiveObject target)
    {
        List<Connection> objConnections = GetAllConnections(obj);
        foreach (var connection in objConnections)
        {
            if (connection.InvolvesObject(target))
            {
                return true; // Found a connection with the target object
            }
        }
        return false; // No connection found
    }

    // Remove a connection
    public void RemoveConnection(Connection connection)
    {
        // Destroy the cable GameObject
        if (connection.Cable != null)
        {
            Destroy(connection.Cable);
        }

        // Remove the connection from the list
        connections.Remove(connection);
    }

    // Count connections of a certain ObjectType for a specific InteractiveObject
    public List<Connection> GetConnectionsByType(InteractiveObject obj, ObjectType targetType)
    {
        // Get all connections for the object
        List<Connection> objConnections = GetAllConnections(obj);

        // Count how many connections involve the specified type
        List<Connection> connectionsByType = new List<Connection>();
        foreach (var connection in objConnections)
        {
            InteractiveObject otherObject = connection.ObjectA == obj ? connection.ObjectB : connection.ObjectA;
            if (otherObject.type == targetType)
            {
                connectionsByType.Add(connection);
            }
        }

        return connectionsByType;
    }

    public List<InteractiveObject> GetConnectedObjectsByType(InteractiveObject obj, ObjectType targetType)
    {
        // Get all connections for the object
        List<Connection> objConnections = GetAllConnections(obj);

        // Count how many connections involve the specified type
        List<InteractiveObject> connectedObjects = new List<InteractiveObject>();
        foreach (var connection in objConnections)
        {
            InteractiveObject otherObject = connection.ObjectA == obj ? connection.ObjectB : connection.ObjectA;
            if (otherObject.type == targetType)
            {
                connectedObjects.Add(otherObject);
            }
        }

        return connectedObjects;
    }


    public Dictionary<int, float> GetTotalCableLengthsByType()
    {
        Dictionary<int, float> totalLengthsByType = new Dictionary<int, float>();

        foreach (Connection connection in connections)
        {
            if (!totalLengthsByType.ContainsKey(connection.CableType))
            {
                totalLengthsByType[connection.CableType] = 0f;
            }
            totalLengthsByType[connection.CableType] += connection.Length;
        }

        return totalLengthsByType;
    }
}

