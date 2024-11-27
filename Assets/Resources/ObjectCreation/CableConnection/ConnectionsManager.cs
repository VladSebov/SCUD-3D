

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

    // Get all connections for a specific object
    public List<Connection> GetConnections(InteractiveObject obj)
    {
        return connections.FindAll(connection => connection.InvolvesObject(obj));
    }

    // Check if the specified object has a connection with another object
    public bool HasConnection(InteractiveObject obj, InteractiveObject target)
    {
        List<Connection> objConnections = GetConnections(obj);
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
}