using UnityEngine;
using System.Collections.Generic;

public class CablePlacer : MonoBehaviour
{
    public GameObject cablePrefab; // Prefab for the cable
    public Material ethernetCableMaterial; // Material for unmounted (semi-transparent) cable
    public Material UPSCableMaterial; // Material for mounted (solid) cable

    private GameObject currentCable; // Active cable being placed
    private int currentCableType;
    private Material currentCableMaterial;
    private Vector3 lastPoint; // Last mounted point
    private Vector3 lastDirection = Vector3.zero;
    private bool isPlacingCable = false; // Flag to indicate active placement
    public Camera playerCam;

    private InteractiveObject connectingObject; // Object which is being connected
    private List<Cable> placedCables = new List<Cable>(); // List of placed cables
    private List<GameObject> spheres = new List<GameObject>();

    private float wallHeight = 1.2f;

    private void Update()
    {
        if (isPlacingCable)
        {
            Vector3 mousePosition = CableUtility.GetMouseWorldPosition(playerCam);
            Vector3 snappedPosition = CableUtility.SnapToGrid(mousePosition, lastPoint);

            if (currentCable != null)
            {
                if (CanPlaceCable(snappedPosition))
                {
                    UpdateCableSegment(lastPoint, snappedPosition);
                }
                else
                {
                    Debug.LogWarning("Запрещено прокладывать кабель в обратном направлении.");
                }
            }

            // Confirm placement on left click
            if (Input.GetMouseButtonDown(1))
            {
                if (CanPlaceCable(snappedPosition))
                {
                    MountCableSegment(snappedPosition);
                }
            }

            // Cancel placement on Ctrl+Z
            if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftControl))
            {
                UndoLastCableSegment();
            }

            // Cancel placement on Ctrl+X
            if (Input.GetKeyDown(KeyCode.X) && Input.GetKey(KeyCode.LeftControl))
            {
                CancelCableCreation();
            }
        }
    }

    public void StartCablePlacement(InteractiveObject startObject, int cableType)
    {
        currentCableType = cableType;
        currentCableMaterial = cableType == CableType.Ethernet ? ethernetCableMaterial : UPSCableMaterial;
        connectingObject = startObject;
        Transform connectionPoint = startObject.connectionPoint;
        lastPoint = connectionPoint.position;
        lastDirection = Vector3.zero;
        isPlacingCable = true;

        CreateCableSegment(lastPoint, lastPoint, currentCableMaterial);
    }

    private void CreateCableSegment(Vector3 startPoint, Vector3 endPoint, Material material)
    {
        currentCable = Instantiate(cablePrefab);
        Cable cableScript = currentCable.GetComponent<Cable>();
        cableScript.Initialize(material);
        cableScript.UpdateCable(startPoint, endPoint);

        cableScript.cableStartPoint = startPoint;
        placedCables.Add(cableScript);
    }


    private void UpdateCableSegment(Vector3 startPoint, Vector3 endPoint)
    {
        Vector3 direction = (endPoint - startPoint).normalized;
        if (Vector3.Dot(direction, lastDirection) > -0.9f) // Allow only forward or perpendicular movement
        {
            currentCable.GetComponent<Cable>().UpdateCable(startPoint, endPoint);
        }
    }

    public void MountCableSegment(Vector3 endPoint)
    {
        if (Physics.Raycast(playerCam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            string targetTag = currentCableType == CableType.Ethernet ? "Connectable" : "UPS";

            // Проверка попадания на устройство
            if (hit.collider.CompareTag(targetTag))
            {
                var hitObject = hit.collider.GetComponent<InteractiveObject>();

                if (currentCable != null)
                {
                    // Добавляем сферу между последними сегментами
                    AddSphereNode(lastPoint);

                    // Обновляем кабель до точки подключения
                    currentCable.GetComponent<Cable>().UpdateCable(lastPoint, hitObject.connectionPoint.position);

                    // Применяем текущий материал, чтобы избежать изменения цвета
                    currentCable.GetComponent<Cable>().SetMaterial(currentCableMaterial);

                    // Монтируем кабель
                    currentCable.GetComponent<Cable>().SetMounted();

                    // Применяем текущий материал снова, если SetMounted изменил его
                    currentCable.GetComponent<Cable>().SetMaterial(currentCableMaterial);

                    // Добавляем сферу в точке подключения
                    AddSphereNode(hitObject.connectionPoint.position);

                    // Создаем соединение
                    GameObject combinedCable = CableUtility.CombineCableSegments(placedCables, connectingObject.name, hitObject.name);
                    Connection newConnection = new Connection(connectingObject, hitObject, combinedCable, currentCableType);

                    ConnectionsManager.Instance.AddConnection(newConnection);

                    Debug.Log($"Cable connected between {connectingObject.name} and {hitObject.name}");
                }

                currentCable = null;
                placedCables.Clear();
            }
            else
            {
                // Привязка кабеля к поверхности
                Vector3 snappedPoint = CableUtility.SnapToGrid(hit.point, lastPoint);
                Vector3 direction = (snappedPoint - lastPoint).normalized;

                if (Vector3.Dot(direction, lastDirection) < -0.9f)
                {
                    Debug.LogWarning("Запрещено прокладывать кабель в обратном направлении.");
                    return;
                }

                // Обновляем текущий кабель
                if (currentCable != null)
                {
                    currentCable.GetComponent<Cable>().UpdateCable(lastPoint, snappedPoint);

                    // Применяем текущий материал
                    currentCable.GetComponent<Cable>().SetMaterial(currentCableMaterial);
                }

                // Создаем новый сегмент кабеля
                CreateCableSegment(lastPoint, snappedPoint, currentCableMaterial);

                // Добавляем сферу
                AddSphereNode(lastPoint);

                lastDirection = direction;
                lastPoint = snappedPoint;

                Debug.Log("Cable segment added.");
            }
        }
        else
        {
            Debug.LogWarning("Raycast не попал в поверхность.");
        }
    }





    private void AddSphereNode(Vector3 position)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        sphere.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f); // Уменьшаем размер сферы
        Renderer renderer = sphere.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = currentCableMaterial; // Применяем текущий материал кабеля
        }
        Destroy(sphere.GetComponent<Collider>()); // Убираем коллайдер для исключения взаимодействия

        spheres.Add(sphere); // Добавляем сферу в список
    }



    public void CancelCableCreation()
    {
        if (isPlacingCable)
        {
            foreach (var cable in placedCables)
            {
                if (cable != null)
                {
                    Destroy(cable.gameObject);
                }
            }
            placedCables.Clear();

            // Удаляем все сферы
            foreach (var sphere in spheres)
            {
                if (sphere != null)
                {
                    Destroy(sphere);
                }
            }
            spheres.Clear();

            isPlacingCable = false;
            lastPoint = Vector3.zero;
            connectingObject = null;

            Debug.Log("Cable creation canceled.");
        }
    }


    private void UndoLastCableSegment()
    {
        if (placedCables.Count > 1)
        {
            Cable lastMountedCable = placedCables[placedCables.Count - 2];
            Cable currentCable = placedCables[placedCables.Count - 1];

            // Удаляем последнюю сферу
            if (spheres.Count > 0)
            {
                GameObject lastSphere = spheres[spheres.Count - 1];
                Destroy(lastSphere);
                spheres.RemoveAt(spheres.Count - 1);
            }

            currentCable.cableStartPoint = lastMountedCable.cableStartPoint;
            lastPoint = lastMountedCable.cableStartPoint;
            Destroy(lastMountedCable.gameObject);
            placedCables.RemoveAt(placedCables.Count - 2);
        }
    }


    public void AutoMountCable(InteractiveObject objectA, InteractiveObject objectB, int cableType)
    {
        if (objectA == null || objectB == null)
        {
            Debug.LogError("Both objects must be provided for auto-mounting.");
            return;
        }
        if (CableUtility.IsConnectionBlockedByNVR(objectA, objectB)) return;

        Vector3 startPoint = objectA.connectionPoint.position;
        Vector3 endPoint = objectB.connectionPoint.position;

        Vector3? nearestWallHitPoint = FindNearestWallHitPoint(startPoint);
        if (!nearestWallHitPoint.HasValue)
        {
            Debug.LogError("No wall found near objectA.");
            return;
        }

        currentCableMaterial = cableType == CableType.Ethernet ? ethernetCableMaterial : UPSCableMaterial;

        Vector3 wallBasePoint = nearestWallHitPoint.Value;
        CreateCableSegment(startPoint, wallBasePoint, currentCableMaterial);

        Vector3 wallTopPoint = wallBasePoint + Vector3.up * wallHeight;
        CreateCableSegment(wallBasePoint, wallTopPoint, currentCableMaterial);

        Vector3 currentPoint = wallTopPoint;
        while (Vector3.Distance(currentPoint, endPoint) > 1.0f)
        {
            Vector3 directionToB = (endPoint - currentPoint).normalized;
            Vector3? nextWallHitPoint = FindNearestWallHitPoint(currentPoint, directionToB);

            if (!nextWallHitPoint.HasValue)
            {
                Debug.LogError("No further walls found towards objectB.");
                break;
            }

            Vector3 nextWallBasePoint = nextWallHitPoint.Value;
            Vector3 nextWallTopPoint = nextWallBasePoint + Vector3.up * wallHeight;

            CreateCableSegment(currentPoint, nextWallTopPoint, currentCableMaterial);
            currentPoint = nextWallTopPoint;
        }

        if (Mathf.Abs(endPoint.x - currentPoint.x) > 0.1f)
        {
            Vector3 horizontalPointX = new Vector3(endPoint.x, currentPoint.y, currentPoint.z);
            CreateCableSegment(currentPoint, horizontalPointX, currentCableMaterial);
            currentPoint = horizontalPointX;
        }

        if (Mathf.Abs(endPoint.z - currentPoint.z) > 0.1f)
        {
            Vector3 horizontalPointZ = new Vector3(currentPoint.x, currentPoint.y, endPoint.z);
            CreateCableSegment(currentPoint, horizontalPointZ, currentCableMaterial);
            currentPoint = horizontalPointZ;
        }

        if (Mathf.Abs(endPoint.y - currentPoint.y) > 0.1f)
        {
            Vector3 verticalPoint = new Vector3(currentPoint.x, endPoint.y, currentPoint.z);
            CreateCableSegment(currentPoint, verticalPoint, currentCableMaterial);
            currentPoint = verticalPoint;
        }

        CreateCableSegment(currentPoint, endPoint, currentCableMaterial);

        GameObject combinedCable = CableUtility.CombineCableSegments(placedCables, objectA.name, objectB.name);
        Connection newConnection = new Connection(objectA, objectB, combinedCable, currentCableType);

        ConnectionsManager.Instance.AddConnection(newConnection);

        currentCable = null;
        placedCables.Clear();
    }

    private bool CanPlaceCable(Vector3 newPosition)
    {
        Vector3 newDirection = (newPosition - lastPoint).normalized;
        if (Vector3.Dot(newDirection, lastDirection) < -0.9f)
        {
            return false;
        }
        return true;
    }

    private Vector3? FindNearestWallHitPoint(Vector3 origin, Vector3? preferredDirection = null)
    {
        float minDistance = float.MaxValue;
        Vector3? nearestHitPoint = null;

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
                        nearestHitPoint = hit.point;
                    }
                }
            }
        }

        return nearestHitPoint;
    }
}
