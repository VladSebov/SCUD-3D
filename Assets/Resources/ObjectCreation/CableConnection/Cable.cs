using UnityEngine;

public class Cable : MonoBehaviour
{
    private Transform cableTransform; // Parent GameObject (empty)
    private Transform cubeTransform; // The cube (child object representing the cable)
    private Vector3 previousEndPoint; // The endpoint of the previous cable segment
    private Vector3 previousDirection; // The direction of the previous cable segment

    public Vector3 cableStartPoint;

    public void Initialize(Material material)
    {
        // Get the cube (child object) transform inside the cable prefab
        cableTransform = transform;
        cubeTransform = transform.GetChild(0); // Assuming the cube is the first child of the parent

        if (cubeTransform == null)
        {
            Debug.LogError("Cube child object not found. Make sure the prefab structure is correct.");
            return;
        }

        // Set the material for the cube (cable segment)
        MeshRenderer renderer = cubeTransform.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }
        else
        {
            Debug.LogError("Renderer component is missing on the cube object.");
        }

        previousEndPoint = Vector3.zero;
        previousDirection = Vector3.zero;
    }

    public void UpdateCable(Vector3 startPoint, Vector3 endPoint)
    {
        // Set the position of the cable (midpoint between start and end)
        cableTransform.position = (startPoint + endPoint) / 2;

        // Calculate the direction vector from start to end point
        Vector3 direction = endPoint - startPoint;

        // Set the rotation of the cable segment to align with the direction
        cableTransform.rotation = Quaternion.LookRotation(direction);

        // Adjust the scale of the cube to match the length of the segment
        float length = (endPoint - startPoint).magnitude;
        Vector3 scale = cubeTransform.localScale;
        cubeTransform.localScale = new Vector3(scale.x, scale.y, length*2);
    }

    public void SetMounted()
    {
        if (cubeTransform == null)
        {
            Debug.LogError("Cube transform is not assigned.");
            return;
        }

        // Mark the cable segment as mounted by changing its color
        MeshRenderer renderer = cubeTransform.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.gray; // Change this to the appropriate material or color
        }
    }

    public float GetLength()
    {
        // Return the largest scale value to account for different orientations of the cube
        return Mathf.Max(cubeTransform.localScale.x, cubeTransform.localScale.y, cubeTransform.localScale.z);
    }
}
