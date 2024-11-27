using UnityEngine;

public class Connection
{
    public InteractiveObject ObjectA; // First object
    public InteractiveObject ObjectB; // Second object
    public GameObject Cable;          // Cable GameObject representing the connection

    public Connection(InteractiveObject objectA, InteractiveObject objectB, GameObject cable)
    {
        ObjectA = objectA;
        ObjectB = objectB;
        Cable = cable;
    }

    // Check if this connection involves a specific object
    public bool InvolvesObject(InteractiveObject obj)
    {
        return ObjectA == obj || ObjectB == obj;
    }
}