using UnityEngine;
using System.Collections.Generic;
using UnityEditor.VersionControl;

public class CableType{
    public const int Ethernet = 0;
    public const int UPS = 1;
}

public static class CableUtility
{
    public static Vector3 SnapToGrid(Vector3 mousePosition, Vector3 startPosition)
    {
        Vector3 direction = mousePosition - startPosition;
        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y) && Mathf.Abs(direction.x) >= Mathf.Abs(direction.z))
        {
            direction.y = 0;
            direction.z = 0;
        }
        else if (Mathf.Abs(direction.y) >= Mathf.Abs(direction.x) && Mathf.Abs(direction.y) >= Mathf.Abs(direction.z))
        {
            direction.x = 0;
            direction.z = 0;
        }
        else
        {
            direction.x = 0;
            direction.y = 0;
        }
        return startPosition + direction;
    }

    public static float CalculateTotalCableLength(List<Cable> cableSegments)
    {
        float totalLength = 0f;
        foreach (var segment in cableSegments)
        {
            totalLength += segment.GetLength(); // Assuming `GetLength` is implemented in `Cable`
        }
        Debug.Log("total length"+totalLength*0.4f);
        return totalLength*0.4f;
    }

    public static GameObject CombineCableSegments(List<Cable> cableSegments, string objectAName, string objectBName)
    {
        string combinedName = $"Cable_{objectAName}_to_{objectBName}";
        GameObject combinedCable = new GameObject(combinedName);

        foreach (var segment in cableSegments)
        {
            segment.transform.SetParent(combinedCable.transform);
        }

        return combinedCable;
    }

    public static Vector3 GetMouseWorldPosition(Camera playerCam)
    {
        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit hit))
        {
            return hit.point;
        }
        return Vector3.zero;
    }


    public static bool IsConnectionBlockedByNVR(InteractiveObject objA, InteractiveObject objB)
    {
        if ((objA.type == ObjectType.Switch && objB.type == ObjectType.Camera) ||
            (objA.type == ObjectType.Camera && objB.type == ObjectType.Switch))
        {
            InteractiveObject switchObject = objA.type == ObjectType.Switch ? objA : objB;
            if (ConnectionsManager.Instance.GetConnectionsByType(switchObject, ObjectType.NVR).Count > 0)
            {
                MessageManager.Instance.ShowMessage("Отключите коммутатор от видеорегистратора, чтобы добавлять новые соединения");
                return true; // Connection is blocked
            }
        }
        return false; // Connection is allowed
    }
}