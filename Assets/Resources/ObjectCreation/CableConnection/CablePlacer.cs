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
                if (connectingObject != null && connectingObject.HasAvailablePorts())
                {
                    var hitObject = hit.collider.GetComponent<InteractiveObject>();
                    currentCable.GetComponent<Cable>().SetMounted();
                    currentCable = null;

                    Debug.Log(CalculateTotalCableLength());

                    // Connect objects and pass total cable length to ObjectManager
                    ObjectManager.Instance.ConnectObjects(connectingObject.id, hitObject.id);
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
}
