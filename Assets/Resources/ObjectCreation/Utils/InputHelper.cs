using System.Linq;
using TMPro;
using UnityEngine;

public static class InputHelper
{
    public static bool IsTypingInInputField()
    {
        return Object.FindObjectsOfType<TMP_InputField>().Any(input => input.isFocused);
    }
}