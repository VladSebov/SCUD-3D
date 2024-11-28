using UnityEngine;

public class RoomMetadata : MonoBehaviour
{
    public int FloorNumber;
    public int RoomNumber;

    //TODO() remove this 
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider>().bounds.size);
    }
}