using UnityEngine;
using TMPro;

public class ConnectedUPSItem : MonoBehaviour
{
    public TextMeshProUGUI TypeText;
    public TextMeshProUGUI CountText;

    public void SetValues(string type, int count)
    {
        TypeText.text = type;
        CountText.text = count.ToString();
    }
}