using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ColorChange : MonoBehaviour
{
    public TMP_Text textMeshPro; // Assign your Text Mesh Pro object in the Inspector
    public Button button; // Assign your Button object in the Inspector

    private Color[] colors; // Array of colors to cycle through
    private int currentColorIndex = 0; // Index of the current color



    void Start()
    {
        // Initialize the array with the colors you want to cycle through
        colors = new Color[]
        {
            new Color(0, 0, 1, 1), // Blue
            new Color(1, 0, 0, 1), // Red
            new Color(0, 1, 0, 1), // Green
            new Color(1, 1, 0, 1), // Yellow
            new Color(1, 1, 1, 1), // White
            new Color(0, 0, 0, 1)  // Black
        };

        // Set initial color
        ChangeColor(colors[currentColorIndex]);
    }

    public void CycleColor()
    {
        // Move to the next color index
        currentColorIndex = (currentColorIndex + 1) % colors.Length;
        // Change the color
        ChangeColor(colors[currentColorIndex]);
    }

    private void ChangeColor(Color color)
    {
        if (textMeshPro != null)
        {
            textMeshPro.color = color;
        }
        if (button != null)
        {
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = color;
            }
        }
    }
}
