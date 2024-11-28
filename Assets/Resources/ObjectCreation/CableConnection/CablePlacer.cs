using UnityEngine;
using System.Collections.Generic;

public class CablePlacer : MonoBehaviour
{
    public GameObject cablePrefab; // Prefab for the cable
    public Material unmountedMaterial; // Material for unmounted (semi-transparent) cable
    public Material mountedMaterial; // Material for mounted (solid) cable

    private GameObject currentCable; // Active cable being placed
    private Vector3 lastPoint; // Last mounted point
    private bool isPlacingCable = false; // Flag to indicate active placement
    public Camera playerCam;

    private InteractiveObject connectingObject; // object which is being connected
    private List<Cable> placedCables = new List<Cable>(); // List of placed cables

    // Method to start cable placement
    public void StartCablePlacement(InteractiveObject startObject)
    {
        connectingObject = startObject;
        Transform connectionPoint = startObject.connectionPoint;
        lastPoint = connectionPoint != null ? connectionPoint.position : startObject.gameObject.transform.position;
        isPlacingCable = true;

        // Instantiate the first cable segment
        CreateCableSegment(lastPoint, lastPoint, unmountedMaterial);
    }

    private void Update()
    {
        if (isPlacingCable)
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            Vector3 snappedPosition = SnapToGrid(mousePosition, lastPoint);

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

    // Method to cancel cable creation and reset to the original state
    public void CancelCableCreation()
    {
        // If a cable is being placed, we cancel it
        if (isPlacingCable)
        {
            // Destroy the current cable segment if it exists
            if (currentCable != null)
            {
                Destroy(currentCable);
                currentCable = null;
            }

            // Destroy any mounted cables
            foreach (var cable in placedCables)
            {
                if (cable != null)
                {
                    Destroy(cable.gameObject); // Destroy the cable object
                }
            }

            // Clear the list of placed cables
            placedCables.Clear();

            // Reset the placement flag to stop placing cables
            isPlacingCable = false;

            // Optionally, reset the last point to the original start position
            lastPoint = Vector3.zero;

            // Optionally, you can also reset the connecting object (if needed)
            connectingObject = null;

            Debug.Log("Cable creation canceled and mounted cables destroyed.");
        }
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
            if (hit.collider.CompareTag("Connectable"))
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
            if (hit.collider.CompareTag("Connectable"))
            {
                if (connectingObject != null && connectingObject.HasAvailablePorts()) //TODO() check if object type is in connectableTypes
                {
                    var hitObject = hit.collider.GetComponent<InteractiveObject>();
                    currentCable.GetComponent<Cable>().SetMounted();

                    GameObject combinedCable = CombineCableSegments(placedCables, connectingObject.name, hitObject.name);
                    Connection newConnection = new Connection(connectingObject, hitObject, combinedCable);

                    Debug.Log(CalculateTotalCableLength());
                    // Create and save the connection
                    ConnectionsManager.Instance.AddConnection(newConnection);

                    currentCable = null;
                    placedCables.Clear();
                }
                else
                {
                    Debug.Log("No available ports!");
                }
            }
            else
            {
                currentCable.GetComponent<Cable>().SetMounted();
                currentCable = null;

                lastPoint = endPoint;
                CreateCableSegment(lastPoint, lastPoint, unmountedMaterial);
            }
        }
    }


    // Utility: Undo the last cable segment
    private void UndoLastCableSegment()
    {
        if (placedCables.Count > 1)
        {
            // Get the last cable segment and its length
            Cable lastMountedCable = placedCables[placedCables.Count - 2];
            Cable currentCable = placedCables[placedCables.Count - 1];
            currentCable.cableStartPoint = lastMountedCable.cableStartPoint;
            lastPoint = lastMountedCable.cableStartPoint;
            // Destroy the last cable segment
            Destroy(lastMountedCable.gameObject);
            // Remove it from the list
            placedCables.RemoveAt(placedCables.Count - 2);
        }
    }

    // Utility: Snap to straight lines (90-degree angles)
    private Vector3 SnapToGrid(Vector3 mousePosition, Vector3 startPosition)
    {
        Vector3 direction = mousePosition - startPosition;

        // Snap to the axis with the greatest distance
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

    // Utility: Get mouse position in world space
    private Vector3 GetMouseWorldPosition()
    {
        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit hit))
        {
            return hit.point;
        }

        return Vector3.zero;
    }

    private float CalculateTotalCableLength()
    {
        float totalLength = 0f;

        foreach (var segment in placedCables)
        {
            Cable cableScript = segment.GetComponent<Cable>();
            totalLength += cableScript.GetLength(); // You'll need to create this method in Cable
        }

        return totalLength;
    }

    public GameObject CombineCableSegments(List<Cable> cableSegments, string objectAName, string objectBName)
    {
        // Create a parent GameObject to hold all cable segments
        string combinedName = $"Cable_{objectAName}_to_{objectBName}";
        GameObject combinedCable = new GameObject(combinedName);

        // Parent all segments under this new GameObject
        foreach (var segment in cableSegments)
        {
            segment.transform.SetParent(combinedCable.transform);
        }

        // Optionally, remove the cable components or other data if not needed
        foreach (var segment in cableSegments)
        {
            Destroy(segment);
        }

        // Return the combined GameObject
        return combinedCable;
    }




    public void NewAutoMountCable(InteractiveObject objectA, InteractiveObject objectB)
    {
        if (objectA == null || objectB == null)
        {
            Debug.LogError("Both objects must be provided for auto-mounting.");
            return;
        }

        Vector3 startPoint = objectA.connectionPoint.position;
        Vector3 endPoint = objectB.connectionPoint.position;

        // Step 1: Find the nearest wall to objectA and mount to its hit point
        Vector3? wallBasePoint = FindNearestWallHitPoint(startPoint);
        if (!wallBasePoint.HasValue)
        {
            Debug.LogError("No wall found near objectA.");
            return;
        }

        CreateCableSegment(startPoint, wallBasePoint.Value, mountedMaterial);

    }
    private float wallHeight = 1.2f;
    public void AutoMountCable(InteractiveObject objectA, InteractiveObject objectB)
    {
        if (objectA == null || objectB == null)
        {
            Debug.LogError("Both objects must be provided for auto-mounting.");
            return;
        }

        Vector3 startPoint = objectA.connectionPoint.position;
        Vector3 endPoint = objectB.connectionPoint.position;

        // Step 1: Find the nearest wall to objectA and mount to it
        Vector3? nearestWallHitPoint = FindNearestWallHitPoint(startPoint);
        if (!nearestWallHitPoint.HasValue)
        {
            Debug.LogError("No wall found near objectA.");
            return;
        }

        Vector3 wallBasePoint = nearestWallHitPoint.Value;
        CreateCableSegment(startPoint, wallBasePoint, mountedMaterial);

        // Step 2: Move vertically up the wall to the wall's top
        Vector3 wallTopPoint = wallBasePoint + Vector3.up * wallHeight; // Replace `wallHeight` with your actual wall height value
        CreateCableSegment(wallBasePoint, wallTopPoint, mountedMaterial);

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

            CreateCableSegment(currentPoint, nextWallTopPoint, mountedMaterial);
            currentPoint = nextWallTopPoint; // Update current point
        }

        // Step 4: Mount cable from the last wall top point to objectB's height along walls
        // Adjust horizontally along the X-axis towards objectB
        if (Mathf.Abs(endPoint.x - currentPoint.x) > 0.1f) // Threshold to avoid unnecessary steps
        {
            Vector3 horizontalPointX = new Vector3(endPoint.x, currentPoint.y, currentPoint.z);
            CreateCableSegment(currentPoint, horizontalPointX, mountedMaterial);
            currentPoint = horizontalPointX;
        }

        // Adjust horizontally along the Z-axis towards objectB
        if (Mathf.Abs(endPoint.z - currentPoint.z) > 0.1f) // Threshold to avoid unnecessary steps
        {
            Vector3 horizontalPointZ = new Vector3(currentPoint.x, currentPoint.y, endPoint.z);
            CreateCableSegment(currentPoint, horizontalPointZ, mountedMaterial);
            currentPoint = horizontalPointZ;
        }

        // Step 5: Adjust vertically to objectB's height
        if (Mathf.Abs(endPoint.y - currentPoint.y) > 0.1f) // Threshold to avoid unnecessary steps
        {
            Vector3 verticalPoint = new Vector3(currentPoint.x, endPoint.y, currentPoint.z);
            CreateCableSegment(currentPoint, verticalPoint, mountedMaterial);
            currentPoint = verticalPoint;
        }

        // Final segment to objectB's connection point
        CreateCableSegment(currentPoint, endPoint, mountedMaterial);
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

public class ConnectingPoint
{
    public Vector3 center; // The center point
    public Vector3 top;    // The top point

    // Constructor to initialize the fields
    public ConnectingPoint(Vector3 center, Vector3 top)
    {
        this.center = center;
        this.top = top;
    }
}