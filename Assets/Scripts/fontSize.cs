using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class fontStyle : MonoBehaviour
{
    public TMP_Text textMeshPro; // Assign your Text Mesh Pro object in the Inspector
    public TMP_Dropdown dropdown; // Assign your Text Mesh Pro Dropdown object in the Inspector

    public void ChangeFontStyle()
    {
        int style = dropdown.value;
        switch (style)
        {
            case 0:
                textMeshPro.fontStyle = FontStyles.Normal;
                break;
            case 1:
                textMeshPro.fontStyle = FontStyles.Bold;
                break;
            case 2:
                textMeshPro.fontStyle = FontStyles.Italic;
                break;
            case 3:
                textMeshPro.fontStyle = FontStyles.Bold | FontStyles.Italic;
                break;
            case 4:
                textMeshPro.fontStyle = FontStyles.Underline;
                break;
            case 5:
                textMeshPro.fontStyle = FontStyles.Strikethrough;
                break;
            case 6:
                textMeshPro.fontStyle = FontStyles.UpperCase;
                break;
        }
    }
}
