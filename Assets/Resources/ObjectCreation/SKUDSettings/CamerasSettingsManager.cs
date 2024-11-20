
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CamerasSettingsManager : MonoBehaviour
{

    //Cameras
    public ScrollRect CamerasScroll;
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
                RenderTexture renderTexture = new RenderTexture(256, 256, 16);
                cameraComponent.targetTexture = renderTexture;
                rawImage.texture = renderTexture;
            }
        }
    }
}