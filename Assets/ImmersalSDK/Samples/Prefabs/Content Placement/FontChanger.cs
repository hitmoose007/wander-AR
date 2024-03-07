using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FontAndButtonStyler : MonoBehaviour
{
    public TMP_Text textMeshPro; // Assign your Text Mesh Pro object in the Inspector
    public Button button; // Assign your Button object in the Inspector

    // Fonts are in Assets/TextMesh Pro/Examples & Extras / Resources

    public void ChangeToAntonFont()
    {
        if (textMeshPro != null)
        {
            textMeshPro.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Anton SDF");
        }

        if (button != null)
        {
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Anton SDF");
            }
        }
    }

    public void ChangeToArialFont()
    {
        if (textMeshPro != null)
        {
            textMeshPro.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Arial SDF");
        }

        if (button != null)
        {
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Arial SDF");
            }
        }
    }

    public void ChangeToRobotoBoldFont()
    {
        if (textMeshPro != null)
        {
            textMeshPro.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Roboto-Bold SDF");
        }

        if (button != null)
        {
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Roboto-Bold SDF");
            }
        }
    }

    public void ChangeToBangersFont()
    {
        if (textMeshPro != null)
        {
            textMeshPro.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Bangers SDF");
        }

        if (button != null)
        {
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Bangers SDF");
            }
        }
    }

    public void ChangeToOswaldBoldFont()
    {
        if (textMeshPro != null)
        {
            textMeshPro.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Oswald-Bold SDF");
        }

        if (button != null)
        {
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Oswald-Bold SDF");
            }
        }
    }

    public void ChangeToElectronicHighwaySignFont()
    {
        if (textMeshPro != null)
        {
            textMeshPro.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Electronic Highway Sign SDF");
        }

        if (button != null)
        {
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Electronic Highway Sign SDF");
            }
        }
    }
}
