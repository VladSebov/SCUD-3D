using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class CablePlacer : MonoBehaviour
{
    public GameObject cablePrefab; // Prefab for the cable
    //TODO() лучше сделать два материала для разных кабелей (ethernet и обычный), а при установке просто прозрачность менять
    public Material ethernetCableMaterial; // Material for unmounted (semi-transparent) cable
    public Material UPSCableMaterial; // Material for mounted (solid) cable

    private GameObject currentCable; // Active cable being placed
    private int currentCableType;
    private Material currentCableMaterial;
    private Vector3 lastPoint; // Last mounted point
    private bool isPlacingCable = false; // Flag to indicate active placement
    public Camera playerCam;


    private InteractiveObject connectingObject; // object which is being connected
    private List<Cable> placedCables = new List<Cable>(); // List of placed cables

    private void Update()
    {
        if (isPlacingCable)
        {
            Vector3 mousePosition = CableUtility.GetMouseWorldPosition(playerCam);
            Vector3 snappedPosition = CableUtility.SnapToGrid(mousePosition, lastPoint);

            if (currentCable != null)
            {
                UpdateCableSegment(lastPoint, snappedPosition);
            }

            // Confirm placement on left click
            if (Input.GetMouseButtonDown(0) && connectingObject != null)
            {
                MountCableSegment(snappedPosition);
            }

            // Cancel placement on right click
            if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftControl))
            {
                UndoLastCableSegment();
            }

            // Cancel placement on right click
            if (Input.GetKeyDown(KeyCode.X) && Input.GetKey(KeyCode.LeftControl))
            {
                CancelCableCreation();
            }
        }
    }

    // Method to start cable placement
    public void StartCablePlacement(InteractiveObject startObject, int cableType)
    {
        currentCableType = cableType;
        currentCableMaterial = cableType == CableType.Ethernet ? ethernetCableMaterial : UPSCableMaterial;
        connectingObject = startObject;
        Transform connectionPoint = startObject.connectionPoint;
        lastPoint = connectionPoint.position;
        isPlacingCable = true;

        // Instantiate the first cable segment
        CreateCableSegment(lastPoint, lastPoint, currentCableMaterial);
    }

    private void CreateCableSegment(Vector3 startPoint, Vector3 endPoint, Material material)
    {
        currentCable = Instantiate(cablePrefab);
        Cable cableScript = currentCable.GetComponent<Cable>();
        cableScript.Initialize(material);
        cableScript.UpdateCable(startPoint, endPoint);

        cableScript.cableStartPoint = startPoint;
        // Add the new cable segment to the list
        placedCables.Add(cableScript);
    }

    private void UpdateCableSegment(Vector3 startPoint, Vector3 endPoint)
    {
        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit hit))
        {
            List<string> targetTags = new List<string>();
            if (currentCableType == CableType.Ethernet)
                targetTags.Add("Connectable");
            else if (currentCableType == CableType.UPS)
            {
                targetTags.Add("UPS");
                if (connectingObject.type == ObjectType.UPS)
                    targetTags.Add("Connectable");
            }
            if (targetTags.Any(tag => hit.collider.CompareTag(tag)))
            {
                var hitObject = hit.collider.GetComponent<InteractiveObject>();
                if (connectingObject.name != hitObject.name)
                    currentCable.GetComponent<Cable>().UpdateCable(startPoint, hitObject.connectionPoint.position);
            }
            else
            {
                currentCable.GetComponent<Cable>().UpdateCable(startPoint, endPoint);
            }
        }
    }

    public void MountCableSegment(Vector3 endPoint)
    {
        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit hit))
        {
            List<string> targetTags = new List<string>();
            if (currentCableType == CableType.Ethernet)
                targetTags.Add("Connectable");
            else if (currentCableType == CableType.UPS)
            {
                targetTags.Add("UPS");
                if (connectingObject.type == ObjectType.UPS)
                    targetTags.Add("Connectable");
            }
            if (targetTags.Any(tag => hit.collider.CompareTag(tag)))
            {
                var hitObject = hit.collider.GetComponent<InteractiveObject>();
                if (connectingObject.name == hitObject.name) return;

                if (currentCableType == CableType.Ethernet)
                {
                    if (hitObject.HasAvailablePorts() && connectingObject.connectableTypes.Contains(hitObject.type))
                    {
                        if (CableUtility.IsConnectionBlockedByNVR(connectingObject, hitObject))
                            return;
                        GameObject combinedCable = CableUtility.CombineCableSegments(placedCables, connectingObject.name, hitObject.name);
                        float cableLength = CableUtility.CalculateTotalCableLength(placedCables);
                        Connection newConnection = new Connection(connectingObject, hitObject, combinedCable, currentCableType, cableLength);

                        ConnectionsManager.Instance.AddConnection(newConnection);
                        FinalizeConnection(connectingObject, hitObject);
                    }
                    else
                    {
                        Debug.Log("No available ports or incompatible types");
                    }
                }
                else if (currentCableType == CableType.UPS)
                {
                    GameObject combinedCable = CableUtility.CombineCableSegments(placedCables, connectingObject.name, hitObject.name);
                    float cableLength = CableUtility.CalculateTotalCableLength(placedCables);
                    Connection newConnection = new Connection(connectingObject, hitObject, combinedCable, currentCableType, cableLength);

                    ConnectionsManager.Instance.AddConnection(newConnection);
                    FinalizeConnection(connectingObject, hitObject);
                }

                currentCable = null;
                placedCables.Clear();
                connectingObject = null;
            }
            else
            {
                currentCable = null;
                lastPoint = endPoint;
                CreateCableSegment(lastPoint, lastPoint, currentCableMaterial);
            }
        }
    }


    // Method to cancel cable creation and reset to the original state
    public void CancelCableCreation()
    {
        if (isPlacingCable)
        {
            foreach (var cable in placedCables)
            {
                if (cable != null)
                {
                    Destroy(cable.gameObject); // Destroy the cable object
                }
            }
            placedCables.Clear();
            isPlacingCable = false;
            lastPoint = Vector3.zero;
            connectingObject = null;
            Debug.Log("Cable creation canceled and mounted cables destroyed.");
        }
    }

    private void UndoLastCableSegment()
    {
        if (placedCables.Count > 1)
        {
            Cable lastMountedCable = placedCables[placedCables.Count - 2];
            Cable currentCable = placedCables[placedCables.Count - 1];
            currentCable.cableStartPoint = lastMountedCable.cableStartPoint;
            lastPoint = lastMountedCable.cableStartPoint;
            Destroy(lastMountedCable.gameObject);
            placedCables.RemoveAt(placedCables.Count - 2);
        }
    }






    public void AutoMountCable(InteractiveObject objectA, InteractiveObject objectB, int cableType)
    {
        currentCableType = cableType;
        if (objectA == null || objectB == null)
        {
            Debug.LogError("Both objects must be provided for auto-mounting.");
            return;
        }

        // Validation checks
        if (CableUtility.IsConnectionBlockedByNVR(objectA, objectB)) return;
        if (!objectB.HasAvailablePorts() && !objectA.connectableTypes.Contains(objectB.type)) return;

        currentCableMaterial = cableType == CableType.Ethernet ? ethernetCableMaterial : UPSCableMaterial;
        Vector3 startPoint = objectA.connectionPoint.position;
        Vector3 endPoint = objectB.connectionPoint.position;

        // 1. Get nearest walls for both objects
        NearestWall nearestWallA = FindNearestWallHitPoint(startPoint);
        NearestWall nearestWallB = FindNearestWallHitPoint(endPoint);

        bool areObjectsOnTheSameFloor = nearestWallA.wallTopPoint.Value.y == nearestWallB.wallTopPoint.Value.y;

        if (!nearestWallA.wallBasePoint.HasValue || !nearestWallB.wallBasePoint.HasValue)
        {
            Debug.LogError("Could not find walls near objects.");
            return;
        }

        // 2. Mount cable from object A to its nearest wall
        CreateCableSegment(startPoint, nearestWallA.wallBasePoint.Value, currentCableMaterial);

        // 3.Mount cable up the wall A
        CreateCableSegment(nearestWallA.wallBasePoint.Value, nearestWallA.wallTopPoint.Value, currentCableMaterial);

        Debug.Log("nearestWallA.wallTopPoint.Value: " + nearestWallA.wallTopPoint.Value);
        Debug.Log("nearestWallB.wallTopPoint.Value: " + nearestWallB.wallTopPoint.Value);
        // 4. check if z or x coordinates are the same (which means that objects are on the same wall)
        if (Mathf.Abs(nearestWallA.wallTopPoint.Value.x - nearestWallB.wallTopPoint.Value.x) < 0.02f || Mathf.Abs(nearestWallA.wallTopPoint.Value.z - nearestWallB.wallTopPoint.Value.z) < 0.02f)
        {
            if (areObjectsOnTheSameFloor)
            {
                CreateCableSegment(nearestWallA.wallTopPoint.Value, nearestWallB.wallTopPoint.Value, currentCableMaterial);
                CreateCableSegment(nearestWallB.wallTopPoint.Value, nearestWallB.wallBasePoint.Value, currentCableMaterial);
            }
            else
            {
                Vector3 intermediatePoint = new Vector3(nearestWallB.wallTopPoint.Value.x, nearestWallA.wallTopPoint.Value.y, nearestWallB.wallTopPoint.Value.z);
                CreateCableSegment(nearestWallA.wallTopPoint.Value, intermediatePoint, currentCableMaterial);
                CreateCableSegment(intermediatePoint, nearestWallB.wallBasePoint.Value, currentCableMaterial);
            }
            CreateCableSegment(nearestWallB.wallBasePoint.Value, endPoint, currentCableMaterial);
            FinalizeConnection(objectA, objectB);
            return;
        }

        Vector3 currentPoint = nearestWallA.wallTopPoint.Value;
        //5.
        if (nearestWallA.wallDirection == WallDirection.zDirection)
        {
            currentPoint = new Vector3(nearestWallA.wallTopPoint.Value.x, nearestWallA.wallTopPoint.Value.y, nearestWallB.wallTopPoint.Value.z);
            CreateCableSegment(nearestWallA.wallTopPoint.Value, currentPoint, currentCableMaterial);
        }
        else if (nearestWallA.wallDirection == WallDirection.xDirection)
        {
            currentPoint = new Vector3(nearestWallB.wallTopPoint.Value.x, nearestWallA.wallTopPoint.Value.y, nearestWallA.wallTopPoint.Value.z);
            CreateCableSegment(nearestWallA.wallTopPoint.Value, currentPoint, currentCableMaterial);
        }

        bool areWallsParallel = nearestWallA.wallDirection == nearestWallB.wallDirection;
        Debug.Log("areWallsParallel: " + areWallsParallel);
        if (!areWallsParallel)
        {
            if (areObjectsOnTheSameFloor)
            {
                CreateCableSegment(currentPoint, nearestWallB.wallTopPoint.Value, currentCableMaterial);
                CreateCableSegment(nearestWallB.wallTopPoint.Value, nearestWallB.wallBasePoint.Value, currentCableMaterial);
            }
            else
            {
                Vector3 intermediatePoint = new Vector3(nearestWallB.wallTopPoint.Value.x, nearestWallA.wallTopPoint.Value.y, nearestWallB.wallTopPoint.Value.z);
                CreateCableSegment(currentPoint, intermediatePoint, currentCableMaterial);
                CreateCableSegment(intermediatePoint, nearestWallB.wallBasePoint.Value, currentCableMaterial);
            }
            CreateCableSegment(nearestWallB.wallBasePoint.Value, endPoint, currentCableMaterial);
            FinalizeConnection(objectA, objectB);
            return;
        }
        else
        {
            Vector3 direction = currentPoint - nearestWallA.wallTopPoint.Value;
            Vector3 directionOfCableForth;
            Vector3 directionOfCableBack;
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            {
                directionOfCableForth = direction.x > 0 ? Vector3.right : Vector3.left;
                directionOfCableBack = direction.x > 0 ? Vector3.left : Vector3.right;
            }
            else
            {
                directionOfCableForth = direction.z > 0 ? Vector3.forward : Vector3.back;
                directionOfCableBack = direction.z > 0 ? Vector3.back : Vector3.forward;
            }
            NearestWall nearestWallForth = FindNearestWallHitPoint(currentPoint, directionOfCableForth);
            NearestWall nearestWallBack = FindNearestWallHitPoint(currentPoint, directionOfCableBack);
            float distanceForth = nearestWallForth.wallBasePoint.HasValue ?
                Vector3.Distance(currentPoint, nearestWallForth.wallBasePoint.Value) : float.MaxValue;
            float distanceBack = nearestWallBack.wallBasePoint.HasValue ?
                Vector3.Distance(currentPoint, nearestWallBack.wallBasePoint.Value) : float.MaxValue;

            // Remove last cable segment
            if (placedCables.Count > 0)
            {
                Destroy(placedCables[placedCables.Count - 1].gameObject);
                placedCables.RemoveAt(placedCables.Count - 1);
            }

            NearestWall closerWall = distanceForth < distanceBack ? nearestWallForth : nearestWallBack;
            if (!closerWall.wallBasePoint.HasValue)
            {
                Debug.LogError("No valid wall found in either direction");
                return;
            }

            // Mount cable to closer wall's top
            CreateCableSegment(nearestWallA.wallTopPoint.Value, closerWall.wallTopPoint.Value, currentCableMaterial);
            // Calculate intermediate point that aligns with nearestWallB
            Vector3 intermediatePoint;
            if (nearestWallB.wallDirection == WallDirection.xDirection)
            {
                intermediatePoint = new Vector3(closerWall.wallTopPoint.Value.x, closerWall.wallTopPoint.Value.y, nearestWallB.wallTopPoint.Value.z);
            }
            else
            {
                intermediatePoint = new Vector3(nearestWallB.wallTopPoint.Value.x, closerWall.wallTopPoint.Value.y, closerWall.wallTopPoint.Value.z);
            }


            // Create remaining cable segments
            CreateCableSegment(closerWall.wallTopPoint.Value, intermediatePoint, currentCableMaterial);
            if (areObjectsOnTheSameFloor)
            {
                CreateCableSegment(intermediatePoint, nearestWallB.wallTopPoint.Value, currentCableMaterial);
                CreateCableSegment(nearestWallB.wallTopPoint.Value, nearestWallB.wallBasePoint.Value, currentCableMaterial);
            }
            else
            {
                Vector3 intermediatePoint2 = new Vector3(nearestWallB.wallTopPoint.Value.x, closerWall.wallTopPoint.Value.y, nearestWallB.wallTopPoint.Value.z);
                CreateCableSegment(intermediatePoint, intermediatePoint2, currentCableMaterial);
                CreateCableSegment(intermediatePoint2, nearestWallB.wallBasePoint.Value, currentCableMaterial);
            }
            CreateCableSegment(nearestWallB.wallBasePoint.Value, endPoint, currentCableMaterial);

            FinalizeConnection(objectA, objectB);
            return;
        }
    }

    private void FinalizeConnection(InteractiveObject objectA, InteractiveObject objectB)
    {
        GameObject combinedCable = CableUtility.CombineCableSegments(placedCables, objectA.name, objectB.name);
        float cableLength = CableUtility.CalculateTotalCableLength(placedCables);

        // Проверка длины кабеля
        int maxCableLength = RestrictionsManager.Instance.GetMaxCableLength();
        if (cableLength > maxCableLength)
        {
            Debug.LogError($"Cable length exceeds maximum allowed length: {cableLength} > {maxCableLength}");
            // Здесь можно добавить логику для отмены соединения или уведомления игрока
            foreach (var cable in placedCables)
            {
                Destroy(cable.gameObject);
            }
            placedCables.Clear();
            return; // Выход из метода, чтобы не добавлять соединение
        }

        Connection newConnection = new Connection(objectA, objectB, combinedCable, currentCableType, cableLength);
        ConnectionsManager.Instance.AddConnection(newConnection);

        // Очистка
        currentCable = null;
        placedCables.Clear();
        connectingObject = null;
    }


    private NearestWall FindNearestWallHitPoint(Vector3 origin, Vector3? preferredDirection = null)
    {
        int maxLoops = 20;
        float minDistance = float.MaxValue;
        NearestWall nearestWall = new NearestWall();

        // Cast rays in cardinal directions
        Vector3[] directions = preferredDirection.HasValue
            ? new Vector3[] { preferredDirection.Value }
            : new Vector3[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

        foreach (Vector3 direction in directions)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hit, 200f))
            {
                maxLoops--;
                if (maxLoops == 0) break;
                if (hit.collider.CompareTag("Wall"))
                {
                    float distance = Vector3.Distance(origin, hit.point);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestWall.wallBasePoint = hit.point;

                        Collider wallCollider = hit.collider;
                        var some = wallCollider.bounds.max;
                        nearestWall.wallTopPoint = new Vector3(hit.point.x, hit.collider.bounds.max.y - 0.05f, hit.point.z);
                        nearestWall.wallDirection = direction == Vector3.forward || direction == Vector3.back ? WallDirection.xDirection : WallDirection.zDirection;
                    }
                }
            }
        }

        return nearestWall;
    }
}

public class NearestWall
{
    public Vector3? wallBasePoint;
    public Vector3? wallTopPoint;
    public GameObject wall;
    public WallDirection wallDirection;
}

public enum WallDirection
{
    xDirection,
    zDirection,
}