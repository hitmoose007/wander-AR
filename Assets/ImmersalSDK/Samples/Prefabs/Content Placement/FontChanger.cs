using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class font : MonoBehaviour
{
    public TMP_Text textMeshPro; // Assign your Text Mesh Pro object in the Inspector
    public TMP_Dropdown dropdown; // Assign your Text Mesh Pro Dropdown object in the Inspector

    //fonts are in Assets/TextMesh Pro/Examples & Extras / Resources
    public void Changefont()
    {
        int style = dropdown.value;
        switch (style)
        {
            case 0:
                textMeshPro.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Anton SDF");
                break;
            case 1:
                textMeshPro.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Arial SDF");
                break;
            case 2:
                textMeshPro.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Roboto-Bold SDF");
                break;
            case 3:
                textMeshPro.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Bangers SDF");
                break;
            case 4:
                textMeshPro.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Oswald-Bold SDF");
                break;
            case 5:
                textMeshPro.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Electronic Highway Sign SDF");
                break;
        }
    }
}