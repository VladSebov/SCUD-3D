using UnityEngine;

public class Cable : MonoBehaviour
{
    private Transform cableTransform; // This is the parent transform of the cable (the empty GameObject)
    private Transform cylinderTransform; // This will be the actual cylinder (child object)

    // Method to initialize the cable with a given material
    public void Initialize(Material material)
    {
        // Get the parent and the cylinder child
        cableTransform = transform;
        cylinderTransform = transform.GetChild(0); // Assuming the cylinder is the first child of the parent

        if (cylinderTransform == null)
        {
            Debug.LogError("Cylinder child object not found. Make sure the prefab structure is correct.");
            return;
        }

        // Set the material for the cable segment
        MeshRenderer renderer = cylinderTransform.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }
        else
        {
            Debug.LogError("Renderer component is missing on the cylinder object.");
        }
    }

    // Method to update the cable segment between two points
    public void UpdateCable(Vector3 startPoint, Vector3 endPoint)
    {
        if (cylinderTransform == null)
        {
            Debug.LogError("Cylinder transform is not assigned.");
            return;
        }

        // Set the position of the cable (midpoint between start and end)
        cableTransform.position = (startPoint + endPoint) / 2;

        // Calculate the direction vector from start to end point
        Vector3 direction = endPoint - startPoint;

        // Set the rotation of the cable segment to align with the direction
        cableTransform.rotation = Quaternion.LookRotation(direction);

        // Adjust the scale of the cable segment to match its length
        float length = direction.magnitude;
        Vector3 scale = cylinderTransform.localScale;
        cylinderTransform.localScale = new Vector3(scale.x, scale.y, length);
    }

    // Method to indicate the cable segment has been mounted (e.g., with a solid color)
    public void SetMounted()
    {
        if (cylinderTransform == null)
        {
            Debug.LogError("Cylinder transform is not assigned.");
            return;
        }

        // Change the color of the cable to indicate it's mounted
        MeshRenderer renderer = cylinderTransform.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.green; // You can replace this with any material to mark as mounted
        }
        else
        {
            Debug.LogError("Renderer component is missing on the cylinder object.");
        }
    }
}
