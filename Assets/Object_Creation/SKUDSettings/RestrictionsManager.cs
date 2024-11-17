
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RestrictionsManager : MonoBehaviour
{
    private static RestrictionsManager _instance;

    public static RestrictionsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Create a new GameObject to hold the manager if it doesn't exist
                GameObject managerObject = new GameObject("RestrictionsManager");
                _instance = managerObject.AddComponent<RestrictionsManager>();
            }
            return _instance;
        }
    }

    private List<Restriction> restrictions = new List<Restriction>();

    private void Awake()
    {
        // Ensure that there is only one instance of the manager
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keep the manager across scenes
            InitializeRestrictions();
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    private void InitializeRestrictions()
    {
        restrictions.Add(new Restriction { type = RestrictionType.MaxPrice, value = 10000, name = "Максимальная стоимость" });
        restrictions.Add(new Restriction { type = RestrictionType.MaxCameras, value = 5, name = "Максимально допустимое кол-во камер" });
        restrictions.Add(new Restriction { type = RestrictionType.MaxRoles, value = 3, name = "Максимально допустимое кол-во ролей" });
    }

    public void SetRestrictions(List<Restriction> newRestrictions)
    {
        restrictions = newRestrictions;
    }

    public List<Restriction> GetRestrictions()
    {
        return restrictions;
    }

    public bool CheckCameraAvailable()
    {
        int maxCameras = restrictions.Find(r => r.type == RestrictionType.MaxCameras).value;
        int currentCamerasCount = ObjectManager.Instance.GetAllObjects()
           .Where(io => io.type == ObjectType.Camera)
           .ToList()
           .Count;

        return currentCamerasCount < maxCameras;
    }

    public bool CheckRoleAvailable()
    {
        int maxRoles = restrictions.Find(r => r.type == RestrictionType.MaxRoles).value;
        int currentRolesCount = ScudManager.Instance.GetRoles().Count;

        return currentRolesCount < maxRoles;
    }

     public bool CheckThereIsEnoughMoney()
    {
        int maxPrice = restrictions.Find(r => r.type == RestrictionType.MaxPrice).value;
        float currentOverallPrice = ObjectManager.Instance.GetTotalPrice();

        return currentOverallPrice < maxPrice;
    }
}


public enum RestrictionType
{
    MaxPrice,
    MaxCameras,
    MaxRoles
}

[System.Serializable]
public class Restriction
{
    public RestrictionType type;
    public int value;
    public string name;
    public Restriction Clone()
    {
        return new Restriction
        {
            type = this.type,
            name = this.name,
            value = this.value
        };
    }
}
