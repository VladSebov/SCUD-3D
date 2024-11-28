using System;
using System.Collections.Generic;

public enum ObjectType
{
    Camera, 
    Switch, 
    Turnstile, 
    AccessController,
    NVR
}


public static class IDManager
{
    private static Dictionary<ObjectType, int> idCounters = new Dictionary<ObjectType, int>();

    static IDManager()
    {
        Start();
    }

    // Constructor or initialization
    private static void Start()
    {
        // Initialize counters for each object type
        foreach (ObjectType type in Enum.GetValues(typeof(ObjectType)))
        {
            idCounters[type] = 0;
        }
    }

    public static string GenerateId(ObjectType objectType)
    {
        // Generate a new ID based on the current count and increment the counter
        string newId = $"{objectType.ToString().ToLower()}{idCounters[objectType]}";
        idCounters[objectType]++;
        return newId;
    }

}