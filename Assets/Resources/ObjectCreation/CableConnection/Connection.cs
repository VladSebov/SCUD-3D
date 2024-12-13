using UnityEngine;

public class Connection
{
    public InteractiveObject ObjectA; // First object
    public InteractiveObject ObjectB; // Second object
    public GameObject Cable;          // Cable GameObject representing the connection
    public int CableType;          // Cable GameObject representing the connection
    public float Length; //Cable length

    public Connection(InteractiveObject objectA, InteractiveObject objectB, GameObject cable, int cableType, float length)
    {
        ObjectA = objectA;
        ObjectB = objectB;
        Cable = cable;
        CableType = cableType;
        Length = length;
    }

    // Check if this connection involves a specific object
    public bool InvolvesObject(InteractiveObject obj)
    {
        return ObjectA == obj || ObjectB == obj;
    }
}