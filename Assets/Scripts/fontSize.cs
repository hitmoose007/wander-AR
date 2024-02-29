using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FontAndButtonStyler : MonoBehaviour
{
    public TMP_Text textMeshPro; // Assign your Text Mesh Pro object in the Inspector
    public Button button; // Assign your Button object in the Inspector

    private int currentStyleIndex = 0;
    private FontStyles[] availableStyles = { FontStyles.Normal, FontStyles.Bold, FontStyles.Italic,
                                             FontStyles.Bold | FontStyles.Italic, FontStyles.Underline,
                                             FontStyles.Strikethrough, FontStyles.UpperCase };

    // Start is called before the first frame update
    void Start()
    {
        // Initialize text style
        ApplyStyleToText();
        // Initialize button style
        ApplyStyleToButton();
    }

    public void CycleFontStyle()
    {
        // Increment style index
        currentStyleIndex = (currentStyleIndex + 1) % availableStyles.Length;

        // Apply style to text
        ApplyStyleToText();

        // Apply style to button
        ApplyStyleToButton();
    }

    private void ApplyStyleToText()
    {
        if (textMeshPro != null)
        {
            textMeshPro.fontStyle = availableStyles[currentStyleIndex];
        }
    }

    private void ApplyStyleToButton()
    {
        if (button != null)
        {
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            Debug.Log("Button Text: " + buttonText);
            if (buttonText != null)
            {
                buttonText.fontStyle = availableStyles[currentStyleIndex];
            }
        }
    }

}
