
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.PlayerLoop;

public class CamerasSettingsManager : MonoBehaviour
{

    //Cameras
    public ScrollRect CamerasScroll;
    public ScrollRect CamerasBigScroll;
    public GameObject CamerasBigPanel;
    public Button ButtonFullScreen;
    public GameObject CameraViewItem;


    public void FillCameras()
    {
        var cameras = ObjectManager.Instance.GetAllObjects()
        .Where(io => io.type == ObjectType.Camera) // Фильтруем объекты типа Camera
        .ToList();
        // Clear existing items in the scroll view
        foreach (Transform child in CamerasScroll.content)
        {
            Destroy(child.gameObject);
        }

        // Populate the scroll view with connected device IDs
        foreach (var camera in cameras)
        {
            Camera cameraComponent = camera.gameObject.GetComponentInChildren<Camera>();
            cameraComponent.enabled = true;
            GameObject panel = Instantiate(CameraViewItem, CamerasScroll.content);
            RawImage rawImage = panel.GetComponentInChildren<RawImage>();
            panel.GetComponentInChildren<TextMeshProUGUI>().text = camera.id;
            if (rawImage != null)
            {
                RenderTexture renderTexture = new RenderTexture(1024, 1024, 32);
                cameraComponent.targetTexture = renderTexture;
                rawImage.texture = renderTexture;
            }
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CamerasBigPanel.activeSelf)
            {
                CamerasBigPanel.SetActive(false);
            }
        }
    }

    public void FillCamerasFullScreen()
    {
        CamerasBigPanel.SetActive(true);
        var cameras = ObjectManager.Instance.GetAllObjects()
        .Where(io => io.type == ObjectType.Camera) // Фильтруем объекты типа Camera
        .ToList();
        // Clear existing items in the scroll view
        foreach (Transform child in CamerasBigScroll.content)
        {
            Destroy(child.gameObject);
        }

        // Populate the scroll view with connected device IDs
        foreach (var camera in cameras)
        {
            Camera cameraComponent = camera.gameObject.GetComponentInChildren<Camera>();
            cameraComponent.enabled = true;
            GameObject panel = Instantiate(CameraViewItem, CamerasBigScroll.content);
            RawImage rawImage = panel.GetComponentInChildren<RawImage>();
            panel.GetComponentInChildren<TextMeshProUGUI>().text = camera.id;
            if (rawImage != null)
            {
                RenderTexture renderTexture = new RenderTexture(256, 256, 16);
                cameraComponent.targetTexture = renderTexture;
                rawImage.texture = renderTexture;
            }
        }
    }
}