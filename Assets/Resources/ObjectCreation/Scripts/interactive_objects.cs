using System;
using System.Collections.Generic;
using UnityEngine;

public enum MountTag
{
    Wall,
    Floor,
    Ceiling
}

[System.Serializable]
abstract public class InteractiveObject : MonoBehaviour
{
    public string id;
    public ObjectType type;
    public int maxConnections;
    public List<ObjectType> connectableTypes; // list of connectable types
    public List<MountTag> mountTags; // list of tags on which object can be mounted
    public Transform connectionPoint;
    public RoomMetadata roomMetadata;

    public bool HasAvailablePorts()
    {
        return ConnectionsManager.Instance.GetConnections(this).Count < maxConnections; // Check if current connections are less than the maximum allowed
    }
}

[System.Serializable]
public class MyCamera : InteractiveObject
{
    public string viewAngle; // угол обзора (для примера)
}

[System.Serializable]
public class Switch : InteractiveObject
{
    public string serverRackId; // id серверной стойки, на которую он установлен

    public int GetConnectedCamerasCount()
    {
        int camerasCount = 0;
        List<Connection> connections = ConnectionsManager.Instance.GetConnections(this);
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
[System.Serializable]
public class Turnstile : InteractiveObject
{
    public bool CheckRoleIsAllowed(string role)
    {
        List<Connection> connections = ConnectionsManager.Instance.GetConnections(this);
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

[System.Serializable]
public class AccessController : InteractiveObject
{
    public List<string> allowedRoles; // список допустимых ролей

    public bool IsRoleAllowed(string role)
    {
        return allowedRoles.Contains(role);
    }
}

[System.Serializable]
public class NVR : InteractiveObject
{
    public int maxChannels; // список допустимых ролей

    public int GetFreeChannelsCount()
    {
        int busyChannels = 0;
        List<Connection> connections = ConnectionsManager.Instance.GetConnections(this);
        foreach (var connection in connections)
        {
            // since the only connectable to NVR type is switch
            InteractiveObject connectedSwitch = connection.ObjectA == this ? connection.ObjectB : connection.ObjectA;
            busyChannels += ((Switch)connectedSwitch).GetConnectedCamerasCount();
        }
        return maxChannels - busyChannels;
    }
}
