using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CamerasSettingsManager : MonoBehaviour
{
    // UI компоненты
    [Header("UI Components")]
    public ScrollRect CamerasScroll;
    public ScrollRect CamerasBigScroll;
    public GameObject CamerasBigPanel;
    public Button ButtonFullScreen;
    public GameObject CameraViewItem;
    public TMP_Dropdown FloorDropdown; // Выпадающий список этажей
    public Slider VerticalAngleSlider;
    public Slider HorizontalAngleSlider;

    // Переменные
    private int currentPage = 0;
    private GameObject selectedCamera;
    private List<int> floorOptions;

    private void Start()
    {
        PopulateFloorDropdown();
        if (floorOptions.Count > 0)
        {
            FloorDropdown.value = 0;
            OnFloorDropdownChanged(0); // Заполняем камеры для первого этажа
        }
    }

    // Заполнение выпадающего списка этажей
    private void PopulateFloorDropdown()
    {
        floorOptions = ObjectManager.Instance.GetAllObjects()
            .Where(io => io.GetComponent<RoomMetadata>() != null)
            .Select(io => io.GetComponent<RoomMetadata>().FloorNumber)
            .Distinct()
            .OrderBy(floor => floor)
            .ToList();

        if (FloorDropdown == null)
        {
            Debug.LogError("FloorDropdown не назначен в инспекторе!");
            return;
        }

        FloorDropdown.ClearOptions();
        var options = floorOptions.Select(floor => $"Этаж {floor}").ToList();
        FloorDropdown.AddOptions(options);
    }

    // Заполнение списка камер
    public void FillCameras()
    {
        ClearScrollView(CamerasScroll);

        var connectedCameras = GetConnectedCameras();
        if (connectedCameras == null || connectedCameras.Count == 0)
        {
            Debug.LogWarning("Нет подключенных камер для отображения.");
            return;
        }

        foreach (var camera in connectedCameras)
        {
            AddCameraToScrollView(camera, CamerasScroll);
        }
    }

    // Заполнение списка камер по этажу
    public void FillCamerasByFloor(int selectedFloor)
    {
        ClearScrollView(CamerasScroll);

        var camerasOnFloor = ObjectManager.Instance.GetAllObjects()
            .Where(io => io.type == ObjectType.Camera &&
                         io.GetComponent<RoomMetadata>()?.FloorNumber == selectedFloor)
            .ToList();

        if (!camerasOnFloor.Any())
        {
            Debug.LogWarning($"Нет камер на этаже {selectedFloor}.");
            return;
        }

        foreach (var camera in camerasOnFloor)
        {
            AddCameraToScrollView(camera.gameObject, CamerasScroll);
        }
    }

    // Обработчик изменения выбора этажа
    public void OnFloorDropdownChanged(int selectedIndex)
    {
        if (selectedIndex < 0 || selectedIndex >= floorOptions.Count)
        {
            Debug.LogWarning("Некорректный выбор этажа.");
            return;
        }

        int selectedFloor = floorOptions[selectedIndex];
        FillCamerasByFloor(selectedFloor);
    }

    // Добавление камеры в ScrollView
    private void AddCameraToScrollView(GameObject cameraObject, ScrollRect scroll)
    {
        var interactiveObject = cameraObject.GetComponent<InteractiveObject>();
        var roomMetadata = cameraObject.GetComponent<RoomMetadata>();

        if (interactiveObject == null) return;

        string cameraId = interactiveObject.id;
        string roomInfo = roomMetadata != null
            ? $"Этаж {roomMetadata.FloorNumber}, Помещение {roomMetadata.RoomNumber}"
            : "Нет данных о помещении";

        GameObject panel = Instantiate(CameraViewItem, scroll.content);
        panel.GetComponentInChildren<TextMeshProUGUI>().text = $"{cameraId} ({roomInfo})";

        var cameraComponent = cameraObject.GetComponentInChildren<Camera>();
        if (cameraComponent != null)
        {
            var rawImage = panel.GetComponentInChildren<RawImage>();
            if (rawImage != null)
            {
                var renderTexture = new RenderTexture(256, 256, 16);
                cameraComponent.targetTexture = renderTexture;
                rawImage.texture = renderTexture;
            }
            else
            {
                Debug.LogWarning("RawImage не найден в префабе CameraViewItem.");
            }
        }
        else
        {
            Debug.LogWarning($"Камера не найдена на объекте {cameraId}.");
        }
    }

    // Управление поворотом камеры
    public void OnVerticalAngleSliderChanged(float value)
    {
        if (selectedCamera == null) return;

        var cameraComponent = selectedCamera.GetComponent<MyCamera>();
        cameraComponent?.SetVerticalAngle(value);
    }

    public void OnHorizontalAngleSliderChanged(float value)
    {
        if (selectedCamera == null) return;

        var cameraComponent = selectedCamera.GetComponent<MyCamera>();
        cameraComponent?.SetHorizontalAngle(value);
    }

    // Обновление ползунков для выбранной камеры
    public void UpdateSlidersForSelectedCamera(GameObject camera)
    {
        selectedCamera = camera;

        var cameraComponent = selectedCamera.GetComponent<MyCamera>();
        if (cameraComponent == null) return;

        VerticalAngleSlider.value = cameraComponent.GetVerticalAngle();
        HorizontalAngleSlider.value = cameraComponent.GetHorizontalAngle();
    }

    // Очистка ScrollView
    private void ClearScrollView(ScrollRect scroll)
    {
        foreach (Transform child in scroll.content)
        {
            Destroy(child.gameObject);
        }
    }

    // Получение подключенных камер через NVR
    private List<GameObject> GetConnectedCameras()
    {
        var nvrDevices = ObjectManager.Instance.GetAllObjects()
            .Where(io => io.type == ObjectType.NVR)
            .ToList();

        return nvrDevices
            .SelectMany(nvr => ConnectionsManager.Instance.GetEthernetConnections(nvr))
            .Select(connection => connection.ObjectA.type == ObjectType.Camera ? connection.ObjectA.gameObject : connection.ObjectB.gameObject)
            .Distinct()
            .ToList();
    }
}
