using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
abstract public class InteractiveObject : MonoBehaviour
{
    public string id;
    public ObjectType type;
    public int maxConnections;
    public List<ObjectType> connectableTypes; // list of connectable types
    public Transform connectionPoint;

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
}

[System.Serializable]
public class Turnstile : InteractiveObject
{
    public List<string> allowedRoles; // список допустимых ролей
}

[System.Serializable]
public class Terminal : InteractiveObject
{
    public List<string> allowedRoles; // список допустимых ролей
}

[System.Serializable]
public class Server : InteractiveObject
{
    public List<string> allowedRoles; // список допустимых ролей
}

[System.Serializable]
public class Reciever : InteractiveObject
{
    public List<string> allowedRoles; // список допустимых ролей
}

[System.Serializable]
public class ControlBox : InteractiveObject
{
    public List<string> allowedRoles; // список допустимых ролей
}