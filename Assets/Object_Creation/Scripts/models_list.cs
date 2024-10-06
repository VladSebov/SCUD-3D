using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class models_list : MonoBehaviour
{
    public cam cam;

    public float f;
    public string[] array = { "Cam_01", "Turnstile_01", "Shield_01" };
    // Start is called before the first frame update
    void Start()
    {
        cam.OnValueChanged.AddListener(UpdateValue);
    }

    void UpdateValue(float NewValue)
    {
        f = NewValue;
        Debug.Log("New value: " + NewValue);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
