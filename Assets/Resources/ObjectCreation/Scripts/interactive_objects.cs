using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum MountTag
{
    Wall,
    Floor,
    Ceiling,
    UPS,
    ServerBox,
    ServerRack,
    Undefined,
    ForLock
}

public enum ObjectType
{
    Camera,
    Switch,
    Turnstile,
    AccessController,
    NVR,
    UPS,
    Battery,
    ServerBox,
    ServerRack,
    DoorLock,
    WallDoor,
    Computer
}

public interface ConnectableToUPS { }

[Serializable]
abstract public class InteractiveObject : MonoBehaviour
{
    public string id;
    public ObjectType type;
    public int maxConnections;
    public int powerConsumption;
    public float price;
    public List<ObjectType> connectableTypes; // list of connectable types
    public List<MountTag> mountTags; // list of tags on which object can be mounted
    public Transform connectionPoint;
    public RoomMetadata roomMetadata;

    public bool HasAvailablePorts()
    {
        return ConnectionsManager.Instance.GetEthernetConnections(this).Count < maxConnections; // Check if current connections are less than the maximum allowed
    }
}

[Serializable]
public class MyCamera : InteractiveObject
{
   
}

[Serializable]
public class Switch : InteractiveObject, ConnectableToUPS
{

    public int GetConnectedCamerasCount()
    {
        int camerasCount = 0;
        List<Connection> connections = ConnectionsManager.Instance.GetEthernetConnections(this);
        foreach (var connection in connections)
        {
            InteractiveObject connectedObject = connection.ObjectA == this ? connection.ObjectB : connection.ObjectA;
            if (connectedObject.type == ObjectType.Camera)
            {
                camerasCount++;
            }
        }
        return camerasCount;
    }

    public string serverRackId; // id of the server rack it's mounted to
    public Transform mountPoint; // reference to the mount point it's using

    // Add method to handle mounting
    public void MountToRack(ServerRack rack, Transform mountPoint)
    {
        serverRackId = rack.id;
        this.mountPoint = mountPoint;
        transform.SetParent(mountPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        rack.OccupyMountPoint(mountPoint, id);
    }

    // Add method to handle unmounting
    public void UnmountFromRack()
    {
        if (!string.IsNullOrEmpty(serverRackId))
        {
            ServerRack rack = ObjectManager.Instance.GetObject(serverRackId) as ServerRack;
            if (rack != null)
            {
                rack.FreeMountPoint(mountPoint, id);
            }
            serverRackId = null;
            mountPoint = null;
            transform.SetParent(null);
        }
    }
}
[Serializable]
public class Turnstile : InteractiveObject, ConnectableToUPS
{
    public bool CheckRoleIsAllowed(string role)
    {
        List<Connection> connections = ConnectionsManager.Instance.GetEthernetConnections(this);
        if (connections.Count > 0)
        {
            Connection currentConnection = connections[0];
            InteractiveObject potentialAccessController = currentConnection.ObjectA.type == ObjectType.AccessController
            ? currentConnection.ObjectA
            : currentConnection.ObjectB;
            AccessController accessController = potentialAccessController as AccessController;
            return accessController.IsRoleAllowed(role);
        }
        return false; // если нет подключения то по идее и возможность пытаться пройти не надо давать, он же не подключен с фига ли пропускать должен
    }
}

[Serializable]
public class DoorLock : InteractiveObject
{
}

[Serializable]
public class WallDoor : InteractiveObject
{
}

[Serializable]
public class AccessController : InteractiveObject, ConnectableToUPS
{
    public List<string> allowedRoles; // список допустимых ролей

    public bool IsRoleAllowed(string role)
    {
        return allowedRoles.Contains(role);
    }
}

[Serializable]
public class NVR : InteractiveObject, ConnectableToUPS
{
    public int maxChannels; // список допустимых ролей

    public int GetFreeChannelsCount()
    {
        int busyChannels = 0;
        List<Connection> connections = ConnectionsManager.Instance.GetEthernetConnections(this);
        foreach (var connection in connections)
        {
            // since the only connectable to NVR type is switch
            InteractiveObject connectedSwitch = connection.ObjectA == this ? connection.ObjectB : connection.ObjectA;
            busyChannels += ((Switch)connectedSwitch).GetConnectedCamerasCount();
        }
        return maxChannels - busyChannels;
    }

    public string serverRackId; // id of the server rack it's mounted to
    public Transform mountPoint; // reference to the mount point it's using

    // Add method to handle mounting
    public void MountToRack(ServerRack rack, Transform mountPoint)
    {
        serverRackId = rack.id;
        this.mountPoint = mountPoint;
        transform.SetParent(mountPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        rack.OccupyMountPoint(mountPoint, id);
    }

    // Add method to handle unmounting
    public void UnmountFromRack()
    {
        if (!string.IsNullOrEmpty(serverRackId))
        {
            ServerRack rack = ObjectManager.Instance.GetObject(serverRackId) as ServerRack;
            if (rack != null)
            {
                rack.FreeMountPoint(mountPoint, id);
            }
            serverRackId = null;
            mountPoint = null;
            transform.SetParent(null);
        }
    }
}

[Serializable]
public class UPS : InteractiveObject
{
    public List<string> connectedBatteries; // подключенные АКБ
    public List<string> connectedDevices; // подключенные АКБ
    public int maxBatteries;
    public bool HasAvailablePlaceForBattery()
    {
        return connectedBatteries.Count < maxBatteries;
    }
}

[Serializable]
public class Battery : InteractiveObject
{
    public int powerWatts;
}


[Serializable]
public class ServerRack : InteractiveObject
{
    public List<string> placedDevices;
    public int maxPlacedDevices;
    private Transform[] mountPoints;
    private Dictionary<Transform, bool> mountPointStatus = new Dictionary<Transform, bool>();

    private void Start()
    {
        // Find the mount points container
        Transform mountPointsContainer = transform.Find("MountPoints");
        if (mountPointsContainer != null)
        {
            // Get all mount points except the container itself
            mountPoints = mountPointsContainer.GetComponentsInChildren<Transform>();

            // Initialize mount points dictionary
            foreach (Transform point in mountPoints)
            {
                if (point != mountPointsContainer)
                {
                    mountPointStatus[point] = false; // false = empty
                }
            }
        }
    }

    public bool HasAvailablePlace()
    {
        return placedDevices.Count < maxPlacedDevices &&
               mountPointStatus.Any(point => point.Value == false);
    }

    public Transform GetNextAvailableMountPoint()
    {
        foreach (var kvp in mountPointStatus)
        {
            if (!kvp.Value) // if not occupied
            {
                return kvp.Key;
            }
        }
        return null;
    }

    public Transform GetClosestMountPoint(Vector3 position)
    {
        Transform closest = null;
        float closestDistance = float.MaxValue;

        foreach (var kvp in mountPointStatus)
        {
            if (!kvp.Value) // if not occupied
            {
                float distance = Vector3.Distance(kvp.Key.position, position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = kvp.Key;
                }
            }
        }
        return closest;
    }

    public void OccupyMountPoint(Transform point, string deviceId)
    {
        if (mountPointStatus.ContainsKey(point))
        {
            mountPointStatus[point] = true;
            placedDevices.Add(deviceId);
        }
    }

    public void FreeMountPoint(Transform point, string deviceId)
    {
        if (mountPointStatus.ContainsKey(point))
        {
            mountPointStatus[point] = false;
            placedDevices.Remove(deviceId);
        }
    }
}


[Serializable]
public class ServerBox : InteractiveObject
{
    public List<string> placedDevices = new List<string>(); // Initialize the list
    public int maxPlacedDevices;
    private Transform[] mountPoints;
    private Dictionary<Transform, bool> mountPointStatus = new Dictionary<Transform, bool>();

    private void Start()
    {
        Transform mountPointsContainer = transform.Find("MountPoints");
        if (mountPointsContainer != null)
        {
            mountPoints = mountPointsContainer.GetComponentsInChildren<Transform>();
            
            // Clear and reinitialize the dictionary
            mountPointStatus.Clear();
            foreach (Transform point in mountPoints)
            {
                if (point != mountPointsContainer)
                {
                    mountPointStatus[point] = false;
                }
            }
        }
        
        // Clear placed devices list on start
        placedDevices.Clear();
    }

    public bool HasAvailablePlace()
    {
        Debug.Log($"ServerBox - Current placed devices count: {placedDevices.Count}, Max: {maxPlacedDevices}");
        return placedDevices.Count < maxPlacedDevices;
    }

    public Transform GetClosestMountPoint(Vector3 position)
    {
        Transform closest = null;
        float closestDistance = float.MaxValue;

        foreach (var kvp in mountPointStatus)
        {
            if (!kvp.Value) // if not occupied
            {
                float distance = Vector3.Distance(kvp.Key.position, position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = kvp.Key;
                }
            }
        }
        return closest;
    }

    public void OccupyMountPoint(Transform point, string deviceId)
    {
        if (mountPointStatus.ContainsKey(point) && !placedDevices.Contains(deviceId))
        {
            mountPointStatus[point] = true;
            placedDevices.Add(deviceId);
            Debug.Log($"ServerBox - Device {deviceId} added. Total devices: {placedDevices.Count}");
        }
    }

    public void FreeMountPoint(Transform point, string deviceId)
    {
        if (mountPointStatus.ContainsKey(point))
        {
            mountPointStatus[point] = false;
            placedDevices.Remove(deviceId);
            Debug.Log($"ServerBox - Device {deviceId} removed. Total devices: {placedDevices.Count}");
        }
    }
}

[Serializable]
public class Computer : InteractiveObject, ConnectableToUPS {}
