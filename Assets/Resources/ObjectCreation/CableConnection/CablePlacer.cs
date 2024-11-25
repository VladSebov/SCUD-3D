using UnityEngine;

public class CablePlacer : MonoBehaviour
{
    public GameObject cablePrefab; // Prefab for the cable
    public Material unmountedMaterial; // Material for unmounted (semi-transparent) cable
    public Material mountedMaterial; // Material for mounted (solid) cable

    private GameObject currentCable; // Active cable being placed
    private Vector3 lastPoint; // Last mounted point
    private bool isPlacingCable = false; // Flag to indicate active placement
    public Camera playerCam;

    // Method to start cable placement
    public void StartCablePlacement(Vector3 startPoint)
    {
        lastPoint = startPoint;
        isPlacingCable = true;

        // Instantiate the first cable segment
        CreateCableSegment(startPoint, startPoint, unmountedMaterial);
    }

    private void Update()
    {
        if (isPlacingCable)
        {
            // Update the cable to follow the mouse
            Vector3 mousePosition = GetMouseWorldPosition();
            Vector3 snappedPosition = SnapToGrid(mousePosition, lastPoint);

            if (currentCable != null)
            {
                UpdateCableSegment(lastPoint, snappedPosition);
            }

            // Confirm placement on left click
            if (Input.GetMouseButtonDown(0))
            {
                MountCableSegment(snappedPosition);
            }

            // Cancel placement on right click
            if (Input.GetMouseButtonDown(1))
            {
                CancelCablePlacement();
            }
        }
    }

    // Utility: Create a new cable segment
    private void CreateCableSegment(Vector3 startPoint, Vector3 endPoint, Material material)
    {
        currentCable = Instantiate(cablePrefab);
        Cable cableScript = currentCable.GetComponent<Cable>();
        cableScript.Initialize(material);
        cableScript.UpdateCable(startPoint, endPoint);
    }

    // Utility: Update the current cable segment
    private void UpdateCableSegment(Vector3 startPoint, Vector3 endPoint)
    {
        currentCable.GetComponent<Cable>().UpdateCable(startPoint, endPoint);
    }

    // Utility: Finalize and mount the current cable segment
    private void MountCableSegment(Vector3 endPoint)
    {
        if (currentCable != null)
        {
            // Finalize the current segment
            currentCable.GetComponent<Cable>().SetMounted();
            currentCable = null;
        }

        // Create a new cable segment starting from the last point
        lastPoint = endPoint;
        CreateCableSegment(lastPoint, lastPoint, unmountedMaterial);
    }

    // Utility: Cancel the cable placement
    public void CancelCablePlacement()
    {
        if (currentCable != null)
        {
            Destroy(currentCable);
        }
        currentCable = null;
        isPlacingCable = false;
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
}
