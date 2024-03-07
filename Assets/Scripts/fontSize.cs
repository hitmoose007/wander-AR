using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class fontStyle : MonoBehaviour
{
    public TMP_Text textMeshPro; // Assign your Text Mesh Pro object in the Inspector

    public void SetNormalStyle()
    {
        textMeshPro.fontStyle = FontStyles.Normal;
    }

    public void SetBoldStyle()
    {
        textMeshPro.fontStyle = FontStyles.Bold;
    }

    public void SetItalicStyle()
    {
        textMeshPro.fontStyle = FontStyles.Italic;
    }

    public void SetBoldItalicStyle()
    {
        textMeshPro.fontStyle = FontStyles.Bold | FontStyles.Italic;
    }

    public void SetUnderlineStyle()
    {
        textMeshPro.fontStyle = FontStyles.Underline;
    }

    public void SetStrikethroughStyle()
    {
        textMeshPro.fontStyle = FontStyles.Strikethrough;
    }

    public void SetUpperCaseStyle()
    {
        textMeshPro.fontStyle = FontStyles.UpperCase;
    }
}
