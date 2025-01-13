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
    public TextMeshProUGUI hintText; // Add this field


    public void HideAllCameraPanels()
    {
        // Hide or deactivate the regular scroll view and its content
        CamerasScroll.gameObject.SetActive(false);
        foreach (Transform child in CamerasScroll.content)
        {
            // Disable each child game object to stop any rendering or updates
            child.gameObject.SetActive(false);

            // Remove RenderTexture from associated cameras (if any)
            var rawImage = child.GetComponentInChildren<RawImage>();
            if (rawImage != null && rawImage.texture is RenderTexture renderTexture)
            {
                renderTexture.Release();
                Destroy(renderTexture); // Cleanup RenderTexture
                rawImage.texture = null;
            }
        }

        // Hide or deactivate the big scroll view and its content
        CamerasBigPanel.SetActive(false);
        foreach (Transform child in CamerasBigScroll.content)
        {
            // Disable each child game object
            child.gameObject.SetActive(false);

            // Remove RenderTexture from associated cameras (if any)
            var rawImage = child.GetComponentInChildren<RawImage>();
            if (rawImage != null && rawImage.texture is RenderTexture renderTexture)
            {
                renderTexture.Release();
                Destroy(renderTexture); // Cleanup RenderTexture
                rawImage.texture = null;
            }
        }

        // Additional cleanup for any cameras that were rendering
        var cameras = ObjectManager.Instance.GetAllObjects()
            .Where(io => io.type == ObjectType.Camera)
            .ToList();
        foreach (var camera in cameras)
        {
            Camera cameraComponent = camera.gameObject.GetComponentInChildren<Camera>();
            if (cameraComponent != null)
            {
                cameraComponent.targetTexture = null; // Ensure the camera stops rendering
                cameraComponent.enabled = false;      // Disable the camera if needed
            }
        }
    }

    public void FillCameras()
    {
        CamerasScroll.gameObject.SetActive(true);
        var cameras = ObjectManager.Instance.GetConnectedCameras();

        // Clear existing items in the scroll view
        foreach (Transform child in CamerasScroll.content)
        {
            Destroy(child.gameObject);
        }

        if (cameras.Count == 0)
        {
            hintText.gameObject.SetActive(true);
            return;
        }

        hintText.gameObject.SetActive(false);

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

        if (cameras.Count == 0)
        {
            hintText.gameObject.SetActive(true);
            return;
        }

        hintText.gameObject.SetActive(false);

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