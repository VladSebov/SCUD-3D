using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class cam : MonoBehaviour
{
    public UnityEvent<float> OnValueChanged;

    public float value;
    public string model_name = "Cam_01";
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            value++;
            OnValueChanged.Invoke(this.value);
        }
    }
}
