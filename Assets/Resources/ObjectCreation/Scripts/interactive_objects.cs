using System;
using System.Collections.Generic;
using UnityEngine;

public enum MountTag
{
    Wall,
    Floor,
    Ceiling,
    UPS,
    Undefined
}

public enum ObjectType
{
    Camera,
    Switch,
    Turnstile,
    AccessController,
    NVR,
    UPS,
    Battery
}

public interface ConnectableToUPS{}

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
    public string serverRackId; // id серверной стойки, на которую он установлен

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
}
[Serializable]
public class Turnstile : InteractiveObject, ConnectableToUPS
{    public bool CheckRoleIsAllowed(string role)
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
public class AccessController : InteractiveObject, ConnectableToUPS
{    public List<string> allowedRoles; // список допустимых ролей

    public bool IsRoleAllowed(string role)
    {
        return allowedRoles.Contains(role);
    }
}

[Serializable]
public class NVR : InteractiveObject, ConnectableToUPS
{    public int maxChannels; // список допустимых ролей

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
