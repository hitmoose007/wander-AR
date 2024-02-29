using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FontAndButtonStyler : MonoBehaviour
{
    public TMP_Text textMeshPro; // Assign your Text Mesh Pro object in the Inspector
    public Button button; // Assign your Button object in the Inspector
    public TMP_FontAsset[] fonts; // Array of font assets to cycle through
    private int currentFontIndex = 0; // Index of the current font

    void Start()
    {
        // Initialize the array with the font assets used in the script
        fonts = new TMP_FontAsset[]
        {
            Resources.Load<TMP_FontAsset>("Fonts & Materials/Anton SDF"),
            Resources.Load<TMP_FontAsset>("Fonts & Materials/Arial SDF"),
            Resources.Load<TMP_FontAsset>("Fonts & Materials/Roboto-Bold SDF"),
            Resources.Load<TMP_FontAsset>("Fonts & Materials/Bangers SDF"),
            Resources.Load<TMP_FontAsset>("Fonts & Materials/Oswald-Bold SDF"),
            Resources.Load<TMP_FontAsset>("Fonts & Materials/Electronic Highway Sign SDF")
        };

        ChangeFont(fonts[currentFontIndex]);
    }
    public void CycleFont()
    {
        // Move to the next font index
        currentFontIndex = (currentFontIndex + 1) % fonts.Length;
        // Change the font
        ChangeFont(fonts[currentFontIndex]);
    }

    private void ChangeFont(TMP_FontAsset font)
    {
        if (textMeshPro != null)
        {
            textMeshPro.font = font;
        }
        if (button != null)
        {
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.font = font;
            }
        }
    }
}
