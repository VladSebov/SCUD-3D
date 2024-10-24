using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
abstract public class InteractiveObject
{
    public string id;
    public ObjectType type;
    public List<string> connections; // list of connected devices id's
    public GameObject gameObject; // gameObject of object

}

[System.Serializable]
public class MyCamera : InteractiveObject
{
    internal static object main;
    public string viewAngle; // угол обзора (для примера)

    internal Ray ScreenPointToRay(Vector3 mousePosition)
    {
        throw new NotImplementedException();
    }
}

[System.Serializable]
public class Switch: InteractiveObject
{
    public string serverRackId; // id серверной стойки, на которую он установлен
}

[System.Serializable]
public class Turnstile: InteractiveObject
{
    public List<string> allowedRoles; // список допустимых ролей
}