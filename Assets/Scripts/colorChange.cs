using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ColorChange : MonoBehaviour
{
    public TMP_Text textMeshPro; // Assign your Text Mesh Pro object in the Inspector

    public void ChangeColorBlue()
    {
        // change to blue
        textMeshPro.color = new Color (0, 0, 1, 1);
    }
    public void ChangeColorRed()
    {
        // change to red
        textMeshPro.color = new Color (1, 0, 0, 1);
    }
    public void ChangeColorGreen()
    {
        // change to green
        textMeshPro.color = new Color (0, 1, 0, 1);
    }
    public void ChangeColorYellow()
    {
        // change to yellow
        textMeshPro.color = new Color (1, 1, 0, 1);
    }
    public void ChangeColorWhite()
    {
        // change to white
        textMeshPro.color = new Color (1, 1, 1, 1);
    }
    public void ChangeColorBlack()
    {
        // change to black
        textMeshPro.color = new Color (0, 0, 0, 1);
    }
}
