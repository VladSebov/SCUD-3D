using UnityEngine;
using TMPro;

public class StatisticsItem : MonoBehaviour
{
    public TextMeshProUGUI TypeText;
    public TextMeshProUGUI CountText;

    public TextMeshProUGUI PriceText;

    public void SetValues(string type, int count, float price)
    {
        TypeText.text = type;
        CountText.text = count.ToString();
        PriceText.text = price.ToString();
    }
}