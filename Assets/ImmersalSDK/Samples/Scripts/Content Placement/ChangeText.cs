using TMPro;
using UnityEngine;

public class UniqueTextChanger : MonoBehaviour
{
    public TMP_Text textMeshPro; // Assign your Text Mesh Pro object in the Inspector
    public TMP_InputField inputField; // Assign your Text Mesh Pro InputField object in the Inspector

    public void ChangeText()
    {
        string newText = inputField.text;
        textMeshPro.text = newText;
    }
}
