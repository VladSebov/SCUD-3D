using UnityEngine;
using System.Collections.Generic;

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
            if (Input.GetMouseButtonDown(1))
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
            string targetTag = currentCableType == CableType.Ethernet ? "Connectable" : "UPS";
            if (hit.collider.CompareTag(targetTag))
            {
                var hitObject = hit.collider.GetComponent<InteractiveObject>();
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
        string targetTag = currentCableType == CableType.Ethernet ? "Connectable" : "UPS";
        if (hit.collider.CompareTag(targetTag))
        {
            var hitObject = hit.collider.GetComponent<InteractiveObject>();

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
            }

            currentCable = null;
            placedCables.Clear();
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











    private float wallHeight = 1.2f;
    public void AutoMountCable(InteractiveObject objectA, InteractiveObject objectB, int cableType)
    {
        if (objectA == null || objectB == null)
        {
            Debug.LogError("Both objects must be provided for auto-mounting.");
            return;
        }
        if (CableUtility.IsConnectionBlockedByNVR(objectA, objectB)) return;

        Vector3 startPoint = objectA.connectionPoint.position;
        Vector3 endPoint = objectB.connectionPoint.position;

        // Step 1: Find the nearest wall to objectA and mount to it
        Vector3? nearestWallHitPoint = FindNearestWallHitPoint(startPoint);
        if (!nearestWallHitPoint.HasValue)
        {
            Debug.LogError("No wall found near objectA.");
            return;
        }

        currentCableMaterial = cableType == CableType.Ethernet ? ethernetCableMaterial : UPSCableMaterial;

        Vector3 wallBasePoint = nearestWallHitPoint.Value;
        CreateCableSegment(startPoint, wallBasePoint, currentCableMaterial);

        // Step 2: Move vertically up the wall to the wall's top
        Vector3 wallTopPoint = wallBasePoint + Vector3.up * wallHeight; // Replace `wallHeight` with your actual wall height value
        CreateCableSegment(wallBasePoint, wallTopPoint, currentCableMaterial);

        // Step 3: Move towards objectB along the walls
        Vector3 currentPoint = wallTopPoint;
        while (Vector3.Distance(currentPoint, endPoint) > 1.0f) // Adjustable threshold
        {
            // Find the next wall in the direction of objectB
            Vector3 directionToB = (endPoint - currentPoint).normalized;
            Vector3? nextWallHitPoint = FindNearestWallHitPoint(currentPoint, directionToB);

            if (!nextWallHitPoint.HasValue)
            {
                Debug.LogError("No further walls found towards objectB.");
                break;
            }

            // Mount segment to the next wall
            Vector3 nextWallBasePoint = nextWallHitPoint.Value;
            Vector3 nextWallTopPoint = nextWallBasePoint + Vector3.up * wallHeight;

            CreateCableSegment(currentPoint, nextWallTopPoint, currentCableMaterial);
            currentPoint = nextWallTopPoint; // Update current point
        }

        // Step 4: Mount cable from the last wall top point to objectB's height along walls
        // Adjust horizontally along the X-axis towards objectB
        if (Mathf.Abs(endPoint.x - currentPoint.x) > 0.1f) // Threshold to avoid unnecessary steps
        {
            Vector3 horizontalPointX = new Vector3(endPoint.x, currentPoint.y, currentPoint.z);
            CreateCableSegment(currentPoint, horizontalPointX, currentCableMaterial);
            currentPoint = horizontalPointX;
        }

        // Adjust horizontally along the Z-axis towards objectB
        if (Mathf.Abs(endPoint.z - currentPoint.z) > 0.1f) // Threshold to avoid unnecessary steps
        {
            Vector3 horizontalPointZ = new Vector3(currentPoint.x, currentPoint.y, endPoint.z);
            CreateCableSegment(currentPoint, horizontalPointZ, currentCableMaterial);
            currentPoint = horizontalPointZ;
        }

        // Step 5: Adjust vertically to objectB's height
        if (Mathf.Abs(endPoint.y - currentPoint.y) > 0.1f) // Threshold to avoid unnecessary steps
        {
            Vector3 verticalPoint = new Vector3(currentPoint.x, endPoint.y, currentPoint.z);
            CreateCableSegment(currentPoint, verticalPoint, currentCableMaterial);
            currentPoint = verticalPoint;
        }

        // Final segment to objectB's connection point
        CreateCableSegment(currentPoint, endPoint, currentCableMaterial);

        GameObject combinedCable = CableUtility.CombineCableSegments(placedCables, objectA.name, objectB.name);
        float cableLength = CableUtility.CalculateTotalCableLength(placedCables);
        Connection newConnection = new Connection(objectA, objectB, combinedCable, currentCableType, cableLength);

        ConnectionsManager.Instance.AddConnection(newConnection);

        currentCable = null;
        placedCables.Clear();
    }

    private Vector3? FindNearestWallHitPoint(Vector3 origin, Vector3? preferredDirection = null)
    {
        float minDistance = float.MaxValue;
        Vector3? nearestHitPoint = null;
        Vector3? wallTopPoint = null;

        // Cast rays in cardinal directions
        Vector3[] directions = preferredDirection.HasValue
            ? new Vector3[] { preferredDirection.Value }
            : new Vector3[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

        foreach (Vector3 direction in directions)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hit, Mathf.Infinity))
            {
                if (hit.collider.CompareTag("Wall"))
                {
                    float distance = Vector3.Distance(origin, hit.point);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestHitPoint = hit.point; // Store the exact hit point
                                                     //wallTopPoint = ;
                    }
                }
            }
        }

        return nearestHitPoint;
    }
}
